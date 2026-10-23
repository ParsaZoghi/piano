using System;
using System.Collections;
using System.IO;
using Keystage.Core;
using Keystage.Game;
using Keystage.Input;
using Keystage.Remote;
using Keystage.Ui;
using UnityEngine;

namespace Keystage
{
    /// <summary>
    /// Command-line test hooks for automated checks of a built game. Nothing here
    /// runs unless the game is started with one of these arguments:
    ///   -ks-reset            forget saved settings and progress first
    ///   -ks-unlock           open every lesson
    ///   -ks-theme light|dark, -ks-lang en|fa, -ks-names letters|solfege
    ///   -ks-route ROUTE      first screen, e.g. "free" or "lesson/s2"
    ///   -ks-shots DIR        take a tour of the screens, save screenshots, quit
    ///   -ks-serve            start the phone server and log notes it receives
    /// </summary>
    public static class AutoTest
    {
        public static string StartRoute { get; private set; }

        /// <summary>True while an automated tour runs (the window may not have focus).</summary>
        public static bool Active => shotsDir != null;
        static string shotsDir;
        static bool serve, autoplay;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ReadArguments()
        {
            var args = Environment.GetCommandLineArgs();
            string Value(string name)
            {
                int i = Array.IndexOf(args, name);
                return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
            }
            bool Flag(string name) => Array.IndexOf(args, name) >= 0;

            if (Flag("-ks-reset"))
            {
                PlayerPrefs.DeleteKey("keystage.settings");
                PlayerPrefs.DeleteKey("keystage.progress");
                Progress.Reload();
            }
            string theme = Value("-ks-theme"), lang = Value("-ks-lang"), names = Value("-ks-names");
            if (theme != null || lang != null || names != null)
            {
                Settings.Update(d =>
                {
                    if (theme != null) d.theme = theme;
                    if (lang != null) d.lang = lang;
                    if (names != null) d.naming = names;
                }, "theme", "lang", "naming");
            }
            if (Flag("-ks-unlock")) Progress.UnlockAllForTesting();
            StartRoute = Value("-ks-route");
            shotsDir = Value("-ks-shots");
            serve = Flag("-ks-serve");
            autoplay = Flag("-ks-autoplay");
        }

        public static void Attach(KeystageApp app)
        {
            if (serve)
            {
                RemoteServer.Start();
                Debug.Log($"[ks] phone server: {RemoteServer.Status} {RemoteServer.Url}");
                InputBus.NoteOn += (midi, v, source) => Debug.Log($"[ks] note on {midi} from {source}");
                InputBus.NoteOff += (midi, source) => Debug.Log($"[ks] note off {midi} from {source}");
            }
            if (shotsDir != null) app.StartCoroutine(Tour(app));
            if (autoplay) app.StartCoroutine(Autoplay(app));
        }

        static IEnumerator Shot(string name)
        {
            yield return null;
            yield return new WaitForEndOfFrame();
            string path = Path.Combine(shotsDir, name + ".png");
            ScreenCapture.CaptureScreenshot(path);
            yield return null;
            yield return null;
            Debug.Log("[ks] shot " + path);
        }

        static IEnumerator Wait(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end) yield return null;
        }

        static IEnumerator Tap(int midi, float hold = 0.12f)
        {
            InputBus.Press(midi, 0.8f, "test");
            yield return Wait(hold);
            InputBus.Release(midi, "test");
        }

        /// <summary>
        /// Plays every lesson given in -ks-autoplay-lessons (comma separated) like a
        /// perfect student, logs each result line, then quits. Checks the real
        /// engines, timing windows and scoring in a built game.
        /// </summary>
        static IEnumerator Autoplay(KeystageApp app)
        {
            var args = Environment.GetCommandLineArgs();
            int k = Array.IndexOf(args, "-ks-autoplay-lessons");
            string list = k >= 0 && k + 1 < args.Length ? args[k + 1] : "n1,r2,s1";
            yield return Wait(1f);
            foreach (var id in list.Split(','))
            {
                app.Navigate("lesson/" + id);
                yield return Wait(0.3f);
                var screen = LessonScreen.Current;
                if (screen == null) continue;
                screen.TestBegin(false);
                var timed = screen.TestTimedEngine;
                var pressedAt = new System.Collections.Generic.Dictionary<Scene.RunwayNote, bool>();
                float guard = Time.realtimeSinceStartup + 240f;
                while (LessonScreen.Current == screen && Time.realtimeSinceStartup < guard)
                {
                    if (timed != null)
                    {
                        if (!timed.Running) break;
                        var next = timed.Next;
                        if (next != null && timed.SongTime >= next.time - 0.01f && !pressedAt.ContainsKey(next))
                        {
                            pressedAt[next] = true;
                            app.StartCoroutine(Tap(next.midi, next.dur * 0.8f / Mathf.Max(0.1f, Settings.Data.speed)));
                        }
                    }
                    else
                    {
                        int target = screen.TestFindTarget;
                        if (target >= 0) yield return Tap(target, 0.05f);
                        if (!FindRunning(screen)) break;
                    }
                    yield return null;
                }
                yield return Wait(1f);
            }
            Debug.Log("[ks] autoplay done");
            Application.Quit();
        }

        static bool FindRunning(LessonScreen screen) => screen.TestEngineRunning;

        static IEnumerator Tour(KeystageApp app)
        {
            Directory.CreateDirectory(shotsDir);
            yield return Wait(1.5f);
            string first = StartRoute ?? "menu";
            yield return Shot("01-" + first.Replace('/', '-'));

            if (StartRoute == null)
            {
                app.Navigate("free");
                yield return Wait(0.6f);
                InputBus.Press(60, 0.8f, "test");
                InputBus.Press(64, 0.8f, "test");
                InputBus.Press(67, 0.8f, "test");
                yield return Wait(0.3f);
                yield return Shot("02-free");
                InputBus.ReleaseAll("test");

                app.Navigate("lessons");
                yield return Wait(0.6f);
                yield return Shot("03-lessons");

                app.Navigate("lesson/n1");
                yield return Wait(0.6f);
                yield return Shot("04-n1-intro");
                LessonScreen.Current?.TestBegin(false);
                yield return Wait(0.4f);
                yield return Shot("05-n1-prompt");

                app.Navigate("lesson/r2");
                yield return Wait(0.4f);
                LessonScreen.Current?.TestBegin(false);
                yield return Wait(4.5f);
                yield return Tap(62, 0.1f);
                yield return Shot("06-r2-running");

                app.Navigate("lesson/s2");
                yield return Wait(0.4f);
                LessonScreen.Current?.TestBegin(true);
                yield return Wait(5.5f);
                yield return Shot("07-s2-demo");

                app.Navigate("free");
                yield return Wait(0.4f);
                app.OpenDevices();
                RemoteServer.Start();
                yield return Wait(0.6f);
                yield return Shot("08-devices");
                Debug.Log($"[ks] phone server: {RemoteServer.Status} {RemoteServer.Url}");
            }
            else if (StartRoute.StartsWith("lesson/"))
            {
                LessonScreen.Current?.TestBegin(false);
                yield return Wait(5f);
                yield return Shot("02-running");
            }

            Debug.Log("[ks] tour done");
            yield return Wait(0.5f);
            Application.Quit();
        }
    }
}
