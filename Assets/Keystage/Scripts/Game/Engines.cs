using System;
using System.Collections.Generic;
using Keystage.Audio;
using Keystage.Core;
using Keystage.Scene;
using UnityEngine;

namespace Keystage.Game
{
    /// <summary>What happened on a press, shown as feedback (Perfect, Late…).</summary>
    public struct Judgement
    {
        public string kind; // perfect good ok miss wrong extra short correct found
        public float offset; // seconds; + late, - early
        public int played;
    }

    public class LessonResult
    {
        public bool demo;
        public float accuracy;
        public int score, maxCombo, wrong, shortHolds, total, firstTry;
        public int perfect, good, ok, miss;
        public float meanOffset;
    }

    /// <summary>Shared surface of the two lesson engines.</summary>
    public interface ILessonEngine
    {
        int Score { get; }
        int Combo { get; }
        float Progress { get; }
        bool Running { get; }
        void Prepare(bool demo);
        void Start();
        void Update(float dt);
        void OnNoteOn(int midi);
        void OnNoteOff(int midi);
        void Dispose();
    }

    public class EngineCallbacks
    {
        public Action<Judgement> OnJudge;
        public Action<int> OnCountIn;
        public Action<LessonResult> OnFinish;
        public Action<int, Hint, int, int, bool> OnPrompt; // midi, hint, index, total, solved
    }

    /// <summary>Runs "rhythm" and "play" lessons: moves the runway and judges each press (port of TimedEngine.js).</summary>
    public class TimedEngine : ILessonEngine
    {
        // Timing windows in real seconds either side of the beat.
        const float Perfect = 0.09f, GoodWindow = 0.17f, OkWindow = 0.28f;
        static readonly Dictionary<float, string> BeatLabels = new Dictionary<float, string>
        {
            [0.5f] = "½", [1f] = "1", [1.5f] = "1½", [2f] = "2", [3f] = "3", [4f] = "4",
        };

        readonly Lesson lesson;
        readonly PianoStage stage;
        readonly EngineCallbacks callbacks;
        readonly bool anyKey;
        readonly float spb;
        readonly int meter;
        readonly List<RunwayNote> notes = new List<RunwayNote>();
        readonly float endTime;

        float t;
        int nextBeat, cursor, points, waitTries;
        readonly Dictionary<int, RunwayNote> held = new Dictionary<int, RunwayNote>();
        readonly List<(int midi, float until)> demoHeld = new List<(int, float)>();
        readonly List<float> offsets = new List<float>();
        int perfect, good, ok, miss;

        public bool Demo { get; private set; }
        public bool Paused { get; private set; }
        public bool Waiting { get; private set; }
        public bool Running { get; private set; }
        public int Score { get; private set; }
        public int Combo { get; private set; }
        public int MaxCombo { get; private set; }
        public int Wrong { get; private set; }
        public int ShortHolds { get; private set; }

        public TimedEngine(Lesson lesson, PianoStage stage, EngineCallbacks callbacks)
        {
            this.lesson = lesson;
            this.stage = stage;
            this.callbacks = callbacks;
            anyKey = lesson.mode == LessonMode.Rhythm;
            spb = 60f / lesson.bpm;
            meter = lesson.meter;
            var parsed = Lessons.ParseSong(lesson, out float totalBeats);
            endTime = (totalBeats + 1) * spb;
            foreach (var n in parsed)
            {
                notes.Add(new RunwayNote
                {
                    midi = n.midi,
                    beats = n.beats,
                    time = n.beat * spb,
                    dur = n.beats * spb,
                    label = anyKey ? (BeatLabels.TryGetValue(n.beats, out var l) ? l : n.beats.ToString()) : Notes.PitchName(n.midi),
                });
            }
            Prepare(false);
        }

        public void Prepare(bool demo)
        {
            Demo = demo;
            Running = Paused = Waiting = false;
            t = -meter * spb;
            nextBeat = -meter;
            cursor = points = waitTries = 0;
            Score = Combo = MaxCombo = Wrong = ShortHolds = 0;
            perfect = good = ok = miss = 0;
            offsets.Clear();
            held.Clear();
            demoHeld.Clear();
            foreach (var n in notes) n.state = "pending";
            stage.Keyboard.ReleaseAll();
            stage.Keyboard.HintMidi = null;
            stage.Highway.SetNotes(notes, spb, meter);
            stage.Highway.Update(t);
        }

        public void Start() => Running = true;

        /// <summary>Freeze the song (the scene keeps rendering). Returns the new state.</summary>
        public bool TogglePause(bool? force = null)
        {
            bool next = force ?? !Paused;
            if (!Running && !Paused) return false;
            Paused = next;
            Running = !next;
            if (next) StopDemoNotes();
            return Paused;
        }

        public float Progress => Mathf.Clamp01(t / endTime);

        /// <summary>Song time in seconds (negative during the count-in).</summary>
        public float SongTime => t;

        /// <summary>The next note that still has to be played, or null.</summary>
        public RunwayNote Next
        {
            get
            {
                while (cursor < notes.Count && notes[cursor].state != "pending") cursor++;
                return cursor < notes.Count ? notes[cursor] : null;
            }
        }

        public void Update(float dt)
        {
            if (!Running) return;
            float speed = Demo ? 1f : Settings.Data.speed;
            float nt = t + dt * speed;

            // Wait mode: time stops at the next note until the learner plays it.
            var pending = Next;
            Waiting = false;
            if (!Demo && Settings.Data.wait && !anyKey && pending != null && nt >= pending.time)
            {
                nt = pending.time;
                Waiting = true;
            }

            while (nextBeat * spb <= nt)
            {
                bool counting = nextBeat < 0;
                if (counting || Settings.Data.metronome) Synth.Instance.Click(((nextBeat % meter) + meter) % meter == 0);
                if (nextBeat <= 0) callbacks.OnCountIn?.Invoke(counting ? nextBeat + meter + 1 : 0);
                nextBeat++;
            }

            if (Demo) PlayDemo(nt);
            else
            {
                float late = OkWindow * speed;
                for (int i = cursor; i < notes.Count && notes[i].time < nt - late; i++)
                    if (notes[i].state == "pending") Miss(notes[i]);
            }

            t = nt;
            var upcoming = Next;
            stage.Keyboard.HintMidi = !Demo && Settings.Data.hints && upcoming != null ? upcoming.midi : (int?)null;
            stage.Highway.Update(t);
            if (t >= endTime) Finish();
        }

        void PlayDemo(float time)
        {
            for (int i = cursor; i < notes.Count && notes[i].time <= time; i++)
            {
                var note = notes[i];
                if (note.state != "pending") continue;
                note.state = "hit";
                Synth.Instance.NoteOn(note.midi, 0.7f);
                stage.Keyboard.Press(note.midi, true);
                demoHeld.Add((note.midi, note.time + note.dur * 0.85f));
            }
            for (int i = demoHeld.Count - 1; i >= 0; i--)
            {
                if (demoHeld[i].until > time) continue;
                Synth.Instance.NoteOff(demoHeld[i].midi);
                stage.Keyboard.Press(demoHeld[i].midi, false);
                demoHeld.RemoveAt(i);
            }
        }

        void StopDemoNotes()
        {
            foreach (var h in demoHeld)
            {
                Synth.Instance.NoteOff(h.midi);
                stage.Keyboard.Press(h.midi, false);
            }
            demoHeld.Clear();
        }

        public void OnNoteOn(int midi)
        {
            if (!Running || Demo || Paused) return;
            float speed = Settings.Data.speed;

            if (Waiting)
            {
                var note = Next;
                if (note != null && note.midi == midi)
                {
                    Hit(note, midi, waitTries == 0 ? "perfect" : "good", 0);
                    waitTries = 0;
                }
                else
                {
                    waitTries++;
                    WrongPress(midi, "wrong");
                }
                return;
            }

            RunwayNote best = null;
            float bestOffset = 0;
            bool near = false;
            for (int i = cursor; i < notes.Count; i++)
            {
                var note = notes[i];
                float offset = (t - note.time) / speed; // + late, - early
                if (offset < -OkWindow) break;
                if (note.state != "pending" || offset > OkWindow) continue;
                near = true;
                if ((anyKey || note.midi == midi) && (best == null || Mathf.Abs(offset) < Mathf.Abs(bestOffset)))
                {
                    best = note;
                    bestOffset = offset;
                }
            }
            if (best == null)
            {
                WrongPress(midi, near ? "wrong" : "extra");
                return;
            }
            float abs = Mathf.Abs(bestOffset);
            Hit(best, midi, abs <= Perfect ? "perfect" : abs <= GoodWindow ? "good" : "ok", bestOffset);
        }

        public void OnNoteOff(int midi)
        {
            if (!held.TryGetValue(midi, out var note)) return;
            held.Remove(midi);
            // Long notes must actually be held.
            if (note.beats >= 2 && t - note.time < note.dur * 0.55f && Running)
            {
                ShortHolds++;
                callbacks.OnJudge?.Invoke(new Judgement { kind = "short" });
            }
        }

        void Hit(RunwayNote note, int midi, string kind, float offset)
        {
            note.state = "hit";
            held[midi] = note;
            if (kind == "perfect") perfect++;
            else if (kind == "good") good++;
            else ok++;
            int value = kind == "perfect" ? 100 : kind == "good" ? 70 : 40;
            points += value;
            Combo++;
            MaxCombo = Mathf.Max(MaxCombo, Combo);
            Score += value + Mathf.Min(Combo, 20) * 2;
            offsets.Add(offset);
            stage.Keyboard.Flash(midi, true);
            callbacks.OnJudge?.Invoke(new Judgement { kind = kind, offset = offset });
        }

        void Miss(RunwayNote note)
        {
            note.state = "miss";
            miss++;
            Combo = 0;
            callbacks.OnJudge?.Invoke(new Judgement { kind = "miss" });
        }

        void WrongPress(int midi, string kind)
        {
            Wrong++;
            Combo = 0;
            stage.Keyboard.Flash(midi, false);
            callbacks.OnJudge?.Invoke(new Judgement { kind = kind, played = midi });
        }

        void Finish()
        {
            Running = Paused = false;
            stage.Keyboard.HintMidi = null;
            StopDemoNotes();
            if (Demo)
            {
                callbacks.OnFinish?.Invoke(new LessonResult { demo = true });
                return;
            }
            float max = notes.Count * 100f;
            float penalty = Wrong * 5 + ShortHolds * 15;
            float mean = 0;
            foreach (var o in offsets) mean += o;
            if (offsets.Count > 0) mean /= offsets.Count;
            callbacks.OnFinish?.Invoke(new LessonResult
            {
                accuracy = Mathf.Max(0, points - penalty) / max,
                score = Score,
                maxCombo = MaxCombo,
                perfect = perfect,
                good = good,
                ok = ok,
                miss = miss,
                wrong = Wrong,
                shortHolds = ShortHolds,
                total = notes.Count,
                meanOffset = mean,
            });
        }

        public void Dispose()
        {
            Running = false;
            StopDemoNotes();
            stage.Keyboard.HintMidi = null;
            stage.Keyboard.ReleaseAll();
            stage.Highway.ClearNotes();
        }
    }

    /// <summary>Runs "find" lessons: show a note, wait for the matching key. No timing (port of FindEngine.js).</summary>
    public class FindEngine : ILessonEngine
    {
        const float NextDelay = 0.48f; // seconds the correct answer stays on screen
        const int RevealAfter = 2; // wrong tries before the key lights up

        readonly Lesson lesson;
        readonly PianoStage stage;
        readonly EngineCallbacks callbacks;
        List<(int midi, Hint hint)> prompts;
        int index, tries, firstTry, wrong;
        bool locked;
        float advanceIn = -1;

        public bool Running { get; private set; }
        public int Score { get; private set; }
        public int Combo { get; private set; }
        public int MaxCombo { get; private set; }

        public FindEngine(Lesson lesson, PianoStage stage, EngineCallbacks callbacks)
        {
            this.lesson = lesson;
            this.stage = stage;
            this.callbacks = callbacks;
            Prepare(false);
        }

        public void Prepare(bool demo)
        {
            prompts = Lessons.BuildPrompts(lesson);
            index = tries = firstTry = wrong = 0;
            Score = Combo = MaxCombo = 0;
            Running = locked = false;
            advanceIn = -1;
            stage.Keyboard.HintMidi = null;
        }

        public void Start()
        {
            Running = true;
            Present();
        }

        public float Progress => index / (float)prompts.Count;

        void Present()
        {
            var (midi, hint) = prompts[index];
            tries = 0;
            locked = false;
            stage.Keyboard.HintMidi = hint == Hint.Key ? midi : (int?)null;
            callbacks.OnPrompt?.Invoke(midi, hint, index, prompts.Count, false);
        }

        public void Update(float dt)
        {
            if (advanceIn < 0) return;
            advanceIn -= dt;
            if (advanceIn >= 0) return;
            advanceIn = -1;
            index++;
            if (index < prompts.Count)
            {
                Present();
                return;
            }
            Running = false;
            callbacks.OnFinish?.Invoke(new LessonResult
            {
                accuracy = firstTry / (float)prompts.Count,
                score = Score,
                maxCombo = MaxCombo,
                firstTry = firstTry,
                wrong = wrong,
                total = prompts.Count,
            });
        }

        public void OnNoteOn(int midi)
        {
            if (!Running || locked) return;
            var (target, hint) = prompts[index];

            if (midi != target)
            {
                tries++;
                wrong++;
                Combo = 0;
                stage.Keyboard.Flash(midi, false);
                callbacks.OnJudge?.Invoke(new Judgement { kind = "wrong", played = midi });
                if (tries >= RevealAfter) stage.Keyboard.HintMidi = target;
                return;
            }

            locked = true;
            if (tries == 0) firstTry++;
            Combo = tries == 0 ? Combo + 1 : 0;
            MaxCombo = Mathf.Max(MaxCombo, Combo);
            Score += (tries == 0 ? 100 : tries == 1 ? 50 : 20) + Mathf.Min(Combo, 20) * 2;
            stage.Keyboard.HintMidi = null;
            stage.Keyboard.Flash(midi, true);
            callbacks.OnJudge?.Invoke(new Judgement { kind = tries == 0 ? "correct" : "found" });
            callbacks.OnPrompt?.Invoke(target, hint, index, prompts.Count, true);
            advanceIn = NextDelay;
        }

        public void OnNoteOff(int midi)
        {
        }

        public void Dispose()
        {
            Running = false;
            advanceIn = -1;
            stage.Keyboard.HintMidi = null;
        }
    }
}
