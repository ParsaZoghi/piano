using System;
using System.Collections.Generic;
using Keystage.Audio;
using Keystage.Core;
using Keystage.Game;
using Keystage.Input;
using Keystage.Remote;
using Keystage.Scene;
using Keystage.Ui;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Keystage
{
    /// <summary>
    /// The whole game starts here (port of the web version's main.js + app.js).
    /// Put this on a GameObject with a UIDocument in the scene; it builds the
    /// 3D stage, the UI and the input devices, and routes between screens.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class KeystageApp : MonoBehaviour
    {
        // Camera keys. Letters are taken by the piano, so these use arrows, +, - and 0.
        static readonly (Key key, string action)[] CameraKeys =
        {
            (Key.UpArrow, "up"), (Key.DownArrow, "down"), (Key.LeftArrow, "left"), (Key.RightArrow, "right"),
            (Key.Equals, "in"), (Key.NumpadPlus, "in"), (Key.Minus, "out"), (Key.NumpadMinus, "out"),
            (Key.Digit0, "reset"), (Key.Numpad0, "reset"),
        };

        public static KeystageApp Instance { get; private set; }

        PianoStage stage;
        Synth synth;
        VisualElement app, catcher, screenLayer, modalLayer;
        Dock dock;
        DevicesDialog devices;
        Action unmount;
        string route = "menu";
        ScreenContext context;
        PointerControls pointers;

        public string Route => route;
        public PianoStage Stage => stage;

        void Awake()
        {
            Instance = this;
            InputBus.Reset();
            TypingInput.Blocked = false;
            ApplyPreferences();

            synth = gameObject.AddComponent<Synth>();
            stage = new GameObject("Stage").AddComponent<PianoStage>();
            var cam = Camera.main != null ? Camera.main : new GameObject("Main Camera") { tag = "MainCamera" }.AddComponent<Camera>();
            stage.Init(cam);
            stage.SetTheme(Settings.Data.theme);
            synth.SetVolume(Settings.Data.volume);

            // Every input source ends up here: make the sound and move the 3D key.
            InputBus.NoteOn += (midi, velocity, source) =>
            {
                synth.NoteOn(midi, velocity);
                stage.Keyboard.Press(midi, true);
            };
            InputBus.NoteOff += (midi, source) =>
            {
                synth.NoteOff(midi);
                stage.Keyboard.Press(midi, false);
            };

            MidiInput.Enable(); // no permission prompt on Windows, so MIDI is simply on
            Settings.Changed += OnSettingsChanged;
        }

        void Start()
        {
            Fonts.Init();
            var root = GetComponent<UIDocument>().rootVisualElement;
            root.styleSheets.Add(Resources.Load<StyleSheet>("UI/Keystage"));
            app = UiKit.Div("app", root);
            catcher = UiKit.Div("layer", app, true);
            screenLayer = UiKit.Div("layer", app);
            modalLayer = UiKit.Div("layer", app);
            modalLayer.style.display = DisplayStyle.None;
            ApplyRootClasses();
            app.RegisterCallback<GeometryChangedEvent>(_ => ApplyRootClasses());

            devices = new DevicesDialog(modalLayer);
            dock = new Dock(stage, OpenDevices);
            app.Insert(app.IndexOf(modalLayer), dock.Element);
            pointers = new PointerControls(stage, catcher, app);
            BlockArrowNavigation(root);

            context = new ScreenContext
            {
                Stage = stage,
                Layer = screenLayer,
                Navigate = Navigate,
                OpenDevices = OpenDevices,
            };
            Navigate(AutoTest.StartRoute ?? "menu");
            AutoTest.Attach(this);
        }

        static void ApplyPreferences()
        {
            I18n.SetLanguage(Settings.Data.lang);
            Notes.SetNaming(Settings.Data.naming);
        }

        void ApplyRootClasses()
        {
            if (app == null) return;
            app.EnableInClassList("theme-dark", Settings.Data.theme != "light");
            app.EnableInClassList("theme-light", Settings.Data.theme == "light");
            app.EnableInClassList("lang-fa", I18n.IsRtl);
            app.EnableInClassList("rtl", I18n.IsRtl);
            // Persian paragraphs read right to left (punctuation, mixed Latin words).
            app.languageDirection = I18n.IsRtl ? LanguageDirection.RTL : LanguageDirection.LTR;
            float w = app.layout.width, h = app.layout.height;
            if (!float.IsNaN(w) && w > 0)
            {
                app.EnableInClassList("narrow", w < 760);
                app.EnableInClassList("short", h < 560);
            }
            Fonts.Apply(app, W.Regular);
        }

        void OnSettingsChanged(string[] fields)
        {
            if (Settings.Has(fields, "theme")) stage.SetTheme(Settings.Data.theme);
            if (Settings.Has(fields, "volume")) synth.SetVolume(Settings.Data.volume);
            if (Settings.Has(fields, "lang") || Settings.Has(fields, "naming") || Settings.Has(fields, "theme")) RemoteServer.RefreshUrl();
            if (Settings.Has(fields, "lang") || Settings.Has(fields, "naming"))
            {
                // Language and note names are baked into every screen's text: rebuild it.
                ApplyPreferences();
                ApplyRootClasses();
                dock.Close();
                Navigate(route);
                if (devices.IsOpen) devices.Open();
                return;
            }
            ApplyRootClasses();
            if (!(fields.Length == 1 && fields[0] == "volume")) dock.Render();
        }

        public void Navigate(string to)
        {
            unmount?.Invoke();
            InputBus.ReleaseAll();
            synth.AllOff();
            route = to;
            var parts = to.Split('/');
            switch (parts[0])
            {
                case "free":
                    unmount = FreePlayScreen.Mount(context);
                    break;
                case "lessons":
                    unmount = LessonListScreen.Mount(context);
                    break;
                case "lesson":
                    unmount = LessonScreen.Mount(context, parts.Length > 1 ? parts[1] : "");
                    break;
                default:
                    route = "menu";
                    unmount = MenuScreen.Mount(context);
                    break;
            }
            dock.Render(); // the camera button only shows where the camera can move
        }

        public void OpenDevices()
        {
            dock.Close();
            devices.Open();
        }

        /// <summary>Arrow keys move the camera, so they must not also move UI focus.</summary>
        void BlockArrowNavigation(VisualElement root)
        {
            root.RegisterCallback<NavigationMoveEvent>(e =>
            {
                bool arrows = e.direction == NavigationMoveEvent.Direction.Left || e.direction == NavigationMoveEvent.Direction.Right ||
                              e.direction == NavigationMoveEvent.Direction.Up || e.direction == NavigationMoveEvent.Direction.Down;
                if (arrows && stage.Interactive && !devices.IsOpen)
                {
                    e.StopPropagation();
                    root.focusController?.IgnoreEvent(e);
                }
            }, TrickleDown.TrickleDown);
            // In free play Space is the sustain pedal, not "press the focused button".
            root.RegisterCallback<NavigationSubmitEvent>(e =>
            {
                var kb = Keyboard.current;
                if (route == "free" && kb != null && kb.spaceKey.isPressed)
                {
                    e.StopPropagation();
                    root.focusController?.IgnoreEvent(e);
                }
            }, TrickleDown.TrickleDown);
        }

        void Update()
        {
            TypingInput.Update();
            MidiInput.Update();
            RemoteServer.Update();

            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.escapeKey.wasPressedThisFrame)
            {
                if (devices.IsOpen) devices.Close();
                else if (dock.IsOpen) dock.Close();
                else LessonScreen.Current?.OnEscape();
            }
            bool modifier = kb.ctrlKey.isPressed || kb.altKey.isPressed;
            if (!stage.Interactive || devices.IsOpen || modifier) return;
            foreach (var (key, action) in CameraKeys)
            {
                if (!kb[key].wasPressedThisFrame) continue;
                switch (action)
                {
                    case "up": stage.Step(0, 1); break;
                    case "down": stage.Step(0, -1); break;
                    case "left": stage.Step(-1, 0); break;
                    case "right": stage.Step(1, 0); break;
                    case "in": stage.ZoomBy(1 / 1.2f); break;
                    case "out": stage.ZoomBy(1.2f); break;
                    default: stage.ResetCamera(); break;
                }
            }
        }

        void OnApplicationFocus(bool focused)
        {
            if (focused || AutoTest.Active) return;
            // Leaving the window: release held notes and pause a running lesson.
            TypingInput.ReleaseAll();
            LessonScreen.Current?.TogglePause(true);
        }

        void OnDestroy()
        {
            // The editor keeps static state between Play sessions (domain reload is off in
            // this project), so every subscription made by the screens must be undone here.
            unmount?.Invoke();
            unmount = null;
            devices?.Dispose();
            Settings.Changed -= OnSettingsChanged;
            MidiInput.Shutdown();
            RemoteServer.Stop();
            InputBus.Reset();
        }
    }

    /// <summary>
    /// Mouse and touch on the 3D stage (port of PianoScene.bindPointer). On a key:
    /// play it (drag across keys for a glissando). Anywhere else: one pointer
    /// moves the camera, two pointers pinch to zoom; the wheel zooms.
    /// </summary>
    class PointerControls
    {
        readonly PianoStage stage;
        readonly VisualElement catcher, app;
        readonly Dictionary<int, int> keyPointers = new Dictionary<int, int>();
        readonly Dictionary<int, Vector2> camPointers = new Dictionary<int, Vector2>();

        public PointerControls(PianoStage stage, VisualElement catcher, VisualElement app)
        {
            this.stage = stage;
            this.catcher = catcher;
            this.app = app;
            catcher.RegisterCallback<PointerDownEvent>(OnDown);
            catcher.RegisterCallback<PointerMoveEvent>(OnMove);
            catcher.RegisterCallback<PointerUpEvent>(e => Release(e.pointerId));
            catcher.RegisterCallback<PointerCancelEvent>(e => Release(e.pointerId));
            catcher.RegisterCallback<WheelEvent>(OnWheel);
        }

        /// <summary>Panel coordinates (top-left origin, scaled) to screen pixels (bottom-left origin).</summary>
        Vector2 ToScreen(Vector2 panel)
        {
            float sx = Screen.width / Mathf.Max(1f, app.layout.width);
            float sy = Screen.height / Mathf.Max(1f, app.layout.height);
            return new Vector2(panel.x * sx, Screen.height - panel.y * sy);
        }

        Vector2 Pixels(Vector2 panel)
        {
            float s = Screen.height / Mathf.Max(1f, app.layout.height);
            return panel * s;
        }

        void OnDown(PointerDownEvent e)
        {
            if (!stage.Interactive || e.button > 0) return;
            catcher.CapturePointer(e.pointerId);
            int midi = camPointers.Count > 0 ? -1 : stage.KeyAt(ToScreen(e.position));
            if (midi >= 0)
            {
                keyPointers[e.pointerId] = midi;
                InputBus.Press(midi, 0.8f, "pointer" + e.pointerId);
            }
            else camPointers[e.pointerId] = Pixels(e.position);
        }

        void OnMove(PointerMoveEvent e)
        {
            if (keyPointers.TryGetValue(e.pointerId, out int current))
            {
                int midi = stage.KeyAt(ToScreen(e.position));
                if (midi < 0 || midi == current) return;
                InputBus.Release(current, "pointer" + e.pointerId);
                keyPointers[e.pointerId] = midi;
                InputBus.Press(midi, 0.8f, "pointer" + e.pointerId);
                return;
            }
            if (!camPointers.TryGetValue(e.pointerId, out var last)) return;
            var now = Pixels(e.position);
            if (camPointers.Count == 1)
            {
                stage.PanByPixels(now.x - last.x, now.y - last.y);
                camPointers[e.pointerId] = now;
            }
            else if (camPointers.Count == 2)
            {
                var before = Pinch();
                camPointers[e.pointerId] = now;
                var after = Pinch();
                if (after.distance > 0) stage.ZoomBy(before.distance / after.distance);
                stage.PanByPixels(after.center.x - before.center.x, after.center.y - before.center.y);
            }
        }

        (float distance, Vector2 center) Pinch()
        {
            var points = new List<Vector2>(camPointers.Values);
            return (Vector2.Distance(points[0], points[1]), (points[0] + points[1]) / 2);
        }

        void Release(int pointerId)
        {
            if (keyPointers.TryGetValue(pointerId, out int midi)) InputBus.Release(midi, "pointer" + pointerId);
            keyPointers.Remove(pointerId);
            camPointers.Remove(pointerId);
            if (catcher.HasPointerCapture(pointerId)) catcher.ReleasePointer(pointerId);
        }

        void OnWheel(WheelEvent e)
        {
            if (!stage.Interactive) return;
            float dy = e.delta.y;
            // Mice report lines (about 1-3 per notch), touchpads report pixels.
            float factor = Mathf.Abs(dy) >= 20 ? Mathf.Exp(dy * 0.0012f) : Mathf.Exp(dy * 0.04f);
            stage.ZoomBy(factor);
            e.StopPropagation();
        }
    }
}
