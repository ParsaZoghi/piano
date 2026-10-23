using System;
using System.Collections.Generic;
using Keystage.Audio;
using Keystage.Core;
using Keystage.Game;
using Keystage.Input;
using Keystage.Scene;
using UnityEngine;
using UnityEngine.UIElements;
using static Keystage.Core.I18n;

namespace Keystage.Ui
{
    /// <summary>One lesson: intro card, the lesson itself and the results (port of ui/lessonPlay.js).</summary>
    public class LessonScreen
    {
        static readonly float[] Speeds = { 0.6f, 0.8f, 1f };
        static readonly HashSet<string> BadKinds = new HashSet<string> { "miss", "wrong", "extra", "short" };
        // Rhythm lessons only score timing, so their notes are drawn on the middle line.
        static readonly int RhythmStaffNote = Notes.Parse("B4");
        const float RhythmKeyWidth = 1.5f;

        /// <summary>The screen that is showing, so the app can route Escape to it.</summary>
        public static LessonScreen Current { get; private set; }

        readonly ScreenContext ctx;
        readonly Lesson lesson;
        readonly Lesson nextLesson;
        readonly bool timed, rhythm;
        readonly string title, eyebrow;
        readonly ILessonEngine engine;
        readonly TimedEngine timedEngine;

        VisualElement view, overlay, prompt, toolbar;
        Label stepLabel, nameLabel, msgLabel, judgeLabel, countLabel, scoreLabel, comboLabel;
        VisualElement nameBox, progressBar;
        StaffView staff;
        Button pauseButton;
        float judgeAge = 99f;
        int shownScore = -1, shownCombo = -1, shownProgress = -1;
        RunwayNote shownNext;
        bool? shownWaiting;
        bool forceRefresh;

        public static Action Mount(ScreenContext ctx, string id)
        {
            int index = Lessons.IndexOf(id);
            if (index < 0 || !Progress.IsUnlocked(index))
            {
                ctx.Navigate("lessons");
                return () => { };
            }
            var screen = new LessonScreen(ctx, index);
            Current = screen;
            return () =>
            {
                if (Current == screen) Current = null;
                screen.Unmount();
            };
        }

        LessonScreen(ScreenContext ctx, int index)
        {
            this.ctx = ctx;
            lesson = Lessons.All[index];
            nextLesson = index + 1 < Lessons.All.Length ? Lessons.All[index + 1] : null;
            timed = lesson.mode != LessonMode.Find;
            rhythm = lesson.mode == LessonMode.Rhythm;
            title = Lessons.Title(lesson);
            eyebrow = T("lesson.eyebrow", ("n", index + 1), ("mode", T("mode." + lesson.ModeKey)));

            var (lo, hi) = Lessons.Range(lesson);
            ctx.Stage.Configure(lo, hi, timed ? View.Lesson : View.Find, timed, rhythm ? RhythmKeyWidth : 1f);
            TypingInput.Configure();
            ApplyLabels();

            Build();
            var callbacks = new EngineCallbacks
            {
                OnJudge = OnJudge,
                OnPrompt = ShowFindPrompt,
                OnCountIn = ShowCountIn,
                OnFinish = OnFinish,
            };
            if (timed) engine = timedEngine = new TimedEngine(lesson, ctx.Stage, callbacks);
            else engine = new FindEngine(lesson, ctx.Stage, callbacks);

            InputBus.NoteOn += OnNoteOn;
            InputBus.NoteOff += OnNoteOff;
            Settings.Changed += OnSettings;
            ctx.Stage.Frame += OnFrame;
            ShowIntro(null);
        }

        void Unmount()
        {
            InputBus.NoteOn -= OnNoteOn;
            InputBus.NoteOff -= OnNoteOff;
            Settings.Changed -= OnSettings;
            ctx.Stage.Frame -= OnFrame;
            engine.Dispose();
            view.RemoveFromHierarchy();
        }

        void OnNoteOn(int midi, float velocity, string source) => engine.OnNoteOn(midi);
        void OnNoteOff(int midi, string source) => engine.OnNoteOff(midi);

        void OnSettings(string[] fields)
        {
            if (Settings.Has(fields, "labels")) ApplyLabels();
        }

        void ApplyLabels() => ctx.Stage.Keyboard.SetLabels(Settings.Data.labels, Settings.Data.labels ? TypingInput.LabelMap() : null);

        // ---------- building the HUD ----------

        void Build()
        {
            view = UiKit.Div("layer", ctx.Layer);

            var topbar = UiKit.Div("topbar", view, true);
            UiKit.FadeDown(topbar);
            topbar.Add(UiKit.IconBtn("back", T("common.backLessons"), () => ctx.Navigate("lessons")));
            var titleBox = UiKit.Div("topbar__title col", topbar);
            UiKit.Text(eyebrow, "eyebrow text-start", W.SemiBold, titleBox);
            UiKit.Text(title, "topbar__title-text text-start", W.SemiBold, titleBox);
            var progress = UiKit.Div("progress", topbar);
            progress.tooltip = T("lesson.progress");
            progressBar = UiKit.Div("progress__bar", progress);
            scoreLabel = Stat(topbar, T("lesson.score"));
            comboLabel = Stat(topbar, T("lesson.streak"));

            prompt = UiKit.Div("prompt col" + (timed ? " prompt--side" : ""), view, true);
            prompt.style.display = DisplayStyle.None;
            stepLabel = UiKit.Text("", "eyebrow center-text", W.SemiBold, prompt);
            var row = UiKit.Div("prompt__row", prompt);
            staff = new StaffView();
            row.Add(staff);
            nameBox = UiKit.Div("col", row);
            nameLabel = UiKit.Text("", "prompt__name", W.Display, nameBox);
            nameLabel.style.unityFontDefinition = new StyleFontDefinition(Fonts.Get(W.Display, latin: true));
            msgLabel = UiKit.Text("", "prompt__msg", W.Regular, prompt);
            if (timed) judgeLabel = UiKit.Text("", "judge judge--inline", W.Display, prompt);
            else judgeLabel = UiKit.Text("", "judge judge--float", W.Display, view);

            countLabel = UiKit.Text("", "countin", W.Display, view);
            countLabel.style.display = DisplayStyle.None;

            toolbar = UiKit.Div("toolbar", view, true);
            UiKit.FadeUp(toolbar);
            toolbar.style.display = DisplayStyle.None;
            if (timed) toolbar.Add(UiKit.Btn("btn--ghost", () => Begin(true), "listen", T("lesson.listen")));
            if (timed)
            {
                pauseButton = UiKit.Btn("btn--ghost", () => TogglePause(), "pause", T("lesson.pause"));
                toolbar.Add(pauseButton);
            }
            toolbar.Add(UiKit.Btn("btn--ghost", () => Begin(false), "restart", T("lesson.restart")));
            if (timed)
            {
                var speeds = new List<(float, string)>();
                foreach (var s in Speeds) speeds.Add((s, Num(Mathf.RoundToInt(s * 100)) + "%"));
                var seg = UiKit.Segmented(speeds, Settings.Data.speed, v => Settings.Update(d => d.speed = v, "speed"), forceLtr: true);
                seg.tooltip = T("lesson.speed");
                toolbar.Add(seg);
            }
            if (lesson.mode == LessonMode.Play) toolbar.Add(Toggle("wait", T("lesson.wait")));
            if (timed)
            {
                toolbar.Add(Toggle("metronome", T("lesson.click")));
                toolbar.Add(Toggle("hints", T("lesson.hints")));
            }
            if (IsRtl) toolbar.style.flexDirection = FlexDirection.RowReverse;

            overlay = UiKit.Div("overlay", view, true);
        }

        static Label Stat(VisualElement parent, string label)
        {
            var box = UiKit.Div("stat col", parent);
            UiKit.Text(label, "stat__label", W.SemiBold, box);
            return UiKit.Text(Num(0), "stat__value", W.Bold, box);
        }

        Button Toggle(string key, string text)
        {
            bool Get() => key == "wait" ? Settings.Data.wait : key == "metronome" ? Settings.Data.metronome : Settings.Data.hints;
            Button chip = null;
            chip = UiKit.Chip(text, Get(), () =>
            {
                bool next = !Get();
                Settings.Update(d =>
                {
                    if (key == "wait") d.wait = next;
                    else if (key == "metronome") d.metronome = next;
                    else d.hints = next;
                }, key);
                UiKit.SetOn(chip, next);
                forceRefresh = true; // refresh the prompt text
            });
            return chip;
        }

        // ---------- prompts and feedback ----------

        void ShowFindPrompt(int midi, Hint hint, int index, int total, bool solved)
        {
            TestFindTarget = solved ? -1 : midi;
            prompt.style.display = DisplayStyle.Flex;
            string kind = T("lesson.kind." + HintKey(hint));
            stepLabel.text = T("lesson.findStep", ("i", index + 1), ("total", total), ("kind", kind));
            staff.Set(midi);
            SetName(hint != Hint.Staff || solved ? Notes.PitchName(midi) : "?");
            msgLabel.text = solved ? T("lesson.solved", ("note", Notes.NoteName(midi))) : T("lesson.find." + HintKey(hint));
        }

        static string HintKey(Hint h) => h == Hint.Key ? "key" : h == Hint.Name ? "name" : "staff";

        void SetName(string text)
        {
            nameBox.Clear();
            nameLabel.text = text;
            nameBox.Add(nameLabel);
            nameBox.style.display = string.IsNullOrEmpty(text) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        void ShowTimedPrompt(RunwayNote note, bool waiting)
        {
            prompt.style.display = DisplayStyle.Flex;
            if (rhythm)
            {
                stepLabel.text = T("lesson.bpm", ("n", lesson.bpm));
                staff.Set(note != null ? RhythmStaffNote : (int?)null, note?.beats ?? 1f);
                nameBox.Clear();
                if (note != null)
                {
                    string key = StaffKey(note.beats);
                    UiKit.Text(T("value." + key), "prompt__value", W.SemiBold, nameBox);
                    UiKit.Text(T("beats." + key), "prompt__value-beats", W.Regular, nameBox);
                }
                nameBox.style.display = note != null ? DisplayStyle.Flex : DisplayStyle.None;
                msgLabel.text = T("lesson.rhythmMsg");
                return;
            }
            stepLabel.text = note != null ? T("lesson.next") : T("lesson.done");
            staff.Set(note?.midi, note?.beats ?? 1f);
            SetName(note != null ? Notes.PitchName(note.midi) : "");
            msgLabel.text = waiting ? T("lesson.waiting") : Settings.Data.wait ? T("lesson.waitOn") : T("lesson.playMsg");
        }

        static string StaffKey(float beats) => beats.ToString(System.Globalization.CultureInfo.InvariantCulture).Replace('.', '_');

        void OnJudge(Judgement j)
        {
            string text = T("judge." + j.kind);
            if ((j.kind == "good" || j.kind == "ok") && Mathf.Abs(j.offset) > 0.09f) text += " · " + T(j.offset > 0 ? "judge.late" : "judge.early");
            judgeLabel.text = text;
            judgeLabel.EnableInClassList("judge--good", !BadKinds.Contains(j.kind) && j.kind != "ok");
            judgeLabel.EnableInClassList("judge--ok", j.kind == "ok");
            judgeLabel.EnableInClassList("judge--bad", BadKinds.Contains(j.kind));
            judgeAge = 0f;
            if (!timed && j.kind == "wrong") msgLabel.text = T("lesson.wrongWas", ("note", Notes.PitchName(j.played)));
        }

        void ShowCountIn(int n)
        {
            countLabel.RemoveFromClassList("countin--paused");
            countLabel.text = n > 0 ? Num(n) : "";
            countLabel.style.display = n > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void OnFinish(LessonResult result)
        {
            SetPaused(false);
            if (result.demo)
            {
                ShowIntro(T("lesson.yourTurn"));
                return;
            }
            int earned = Progress.Record(lesson.id, result.accuracy);
            Debug.Log($"[ks] result {lesson.id} accuracy {result.accuracy:0.00} stars {earned} perfect {result.perfect} good {result.good} ok {result.ok} miss {result.miss} wrong {result.wrong} firstTry {result.firstTry}/{result.total}");
            Synth.Instance.Fanfare(earned);
            ShowResults(result, earned);
        }

        // ---------- pause ----------

        void SetPaused(bool paused)
        {
            if (pauseButton == null) return;
            pauseButton.Clear();
            pauseButton.Add(Icons.Create(paused ? "play" : "pause"));
            var label = UiKit.Text(T(paused ? "lesson.resume" : "lesson.pause"), null, W.SemiBold, pauseButton);
            if (IsRtl) label.style.marginRight = 8;
            else label.style.marginLeft = 8;
            countLabel.text = paused ? T("lesson.paused") : "";
            countLabel.EnableInClassList("countin--paused", paused);
            countLabel.style.display = paused ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void TogglePause(bool? force = null)
        {
            if (timedEngine == null || overlay.style.display == DisplayStyle.Flex) return;
            bool paused = timedEngine.TogglePause(force);
            if (paused) Synth.Instance.AllOff();
            SetPaused(paused);
        }

        /// <summary>Test hook (AutoTest): the running timed engine, if any.</summary>
        public TimedEngine TestTimedEngine => timedEngine;

        /// <summary>Test hook (AutoTest): whether the lesson is still running.</summary>
        public bool TestEngineRunning => engine.Running;

        /// <summary>Test hook (AutoTest): the note the find lesson is asking for.</summary>
        public int TestFindTarget { get; private set; } = -1;

        /// <summary>Test hook (AutoTest): press Start or Listen without a mouse.</summary>
        public void TestBegin(bool demo) => Begin(demo);

        /// <summary>Escape pauses or resumes; returns true when it did something.</summary>
        public bool OnEscape()
        {
            if (timedEngine == null || overlay.style.display == DisplayStyle.Flex) return false;
            TogglePause();
            return true;
        }

        // ---------- overlay cards ----------

        void OpenOverlay(VisualElement card)
        {
            overlay.Clear();
            overlay.Add(card);
            overlay.style.display = DisplayStyle.Flex;
            toolbar.style.display = DisplayStyle.None;
            card.Q<Button>(className: "btn--primary")?.Focus();
        }

        void ShowIntro(string note)
        {
            prompt.style.display = DisplayStyle.None;
            var card = UiKit.Div("card col", null, true);
            UiKit.Text(eyebrow, "eyebrow text-start", W.SemiBold, card);
            UiKit.Text(title, "card__title text-start", W.Display, card);
            UiKit.Text(note ?? Lessons.Goal(lesson), "card__lead text-start", W.Regular, card);
            var tips = UiKit.Div("col", card);
            foreach (var tip in Lessons.Tips(lesson))
            {
                var row = UiKit.Div("tip", tips);
                row.Add(Icons.Create("check"));
                UiKit.Text(tip, "text-start", W.Regular, row);
            }
            var actions = UiKit.Div("card__actions", card);
            actions.Add(UiKit.Btn("btn--primary", () => Begin(false), "play", T("lesson.start")));
            if (timed) actions.Add(UiKit.Btn("", () => Begin(true), "listen", T("lesson.listenFirst")));
            OpenOverlay(card);
        }

        void ShowResults(LessonResult r, int earned)
        {
            prompt.style.display = DisplayStyle.None;
            var rows = new List<(string, string)>();
            if (timed)
            {
                rows.Add(("perfect", Num(r.perfect)));
                rows.Add(("good", Num(r.good)));
                rows.Add(("ok", Num(r.ok)));
                rows.Add(("missed", Num(r.miss)));
                rows.Add(("wrong", Num(r.wrong)));
                rows.Add(r.shortHolds > 0 ? ("early", Num(r.shortHolds)) : ("streak", Num(r.maxCombo)));
            }
            else
            {
                rows.Add(("firstTry", $"{Num(r.firstTry)} / {Num(r.total)}"));
                rows.Add(("wrong", Num(r.wrong)));
                rows.Add(("streak", Num(r.maxCombo)));
            }
            bool passed = earned > 0;

            var card = UiKit.Div("card card--result col", null, true);
            UiKit.Text(title, "eyebrow", W.SemiBold, card);
            UiKit.Text(T("result.title" + earned), "card__title", W.Display, card);
            var stars = UiKit.Div("result__stars row row--ltr", card);
            for (int i = 0; i < 3; i++) stars.Add(Icons.Star(i < earned));
            UiKit.Text(T("result.score", ("percent", Mathf.RoundToInt(r.accuracy * 100)), ("score", r.score)), null, W.Regular, card);
            var grid = UiKit.Div("result__grid", card);
            if (IsRtl) grid.style.flexDirection = FlexDirection.RowReverse;
            foreach (var (key, value) in rows)
            {
                var cell = UiKit.Div("result__cell col", grid);
                UiKit.Text(T("result." + key), "result__cell-label", W.Regular, cell);
                UiKit.Text(value, "result__cell-value", W.Bold, cell);
            }
            UiKit.Text(timed ? TimedTip(r) : FindTip(r), "card__lead", W.Regular, card);
            if (!passed) UiKit.Text(T("result.need"), "muted", W.Regular, card);
            var actions = UiKit.Div("card__actions", card);
            if (passed && nextLesson != null)
                actions.Add(UiKit.Btn("btn--primary", () => ctx.Navigate("lesson/" + nextLesson.id), "next", T("result.next"), iconAfter: true));
            actions.Add(UiKit.Btn(passed && nextLesson != null ? "" : "btn--primary", () => Begin(false), "restart", T("result.again")));
            actions.Add(UiKit.Btn("btn--ghost", () => ctx.Navigate("lessons"), null, T("result.all")));
            OpenOverlay(card);
        }

        static string TimedTip(LessonResult r)
        {
            if (r.miss / (float)r.total > 0.3f) return T("tip.missed");
            if (r.shortHolds > 1) return T("tip.hold");
            if (r.wrong > 3) return T("tip.wrong");
            if (r.meanOffset > 0.05f) return T("tip.late");
            if (r.meanOffset < -0.05f) return T("tip.early");
            return T("tip.clean");
        }

        static string FindTip(LessonResult r)
        {
            if (r.accuracy >= 0.9f) return T("tip.findGreat");
            if (r.wrong > r.total / 2f) return T("tip.findSlow");
            return T("tip.findClose");
        }

        // ---------- running ----------

        void Begin(bool demo)
        {
            Synth.Instance.AllOff();
            overlay.Clear();
            overlay.style.display = DisplayStyle.None;
            toolbar.style.display = DisplayStyle.Flex;
            judgeLabel.text = "";
            judgeAge = 99f;
            ShowCountIn(0);
            SetPaused(false);
            engine.Prepare(demo);
            shownNext = null;
            shownWaiting = null;
            if (timed) ShowTimedPrompt(timedEngine.Next, false);
            if (demo) msgLabel.text = T("lesson.demoMsg");
            engine.Start();
            view.focusController?.focusedElement?.Blur();
        }

        void OnFrame(float dt, float time)
        {
            engine.Update(dt);
            if (engine.Score != shownScore) scoreLabel.text = Num(shownScore = engine.Score);
            if (engine.Combo != shownCombo) comboLabel.text = Num(shownCombo = engine.Combo);
            int percent = Mathf.RoundToInt(engine.Progress * 100);
            if (percent != shownProgress)
            {
                shownProgress = percent;
                progressBar.style.width = Length.Percent(percent);
            }
            if (timedEngine != null && timedEngine.Running && !timedEngine.Demo &&
                (forceRefresh || timedEngine.Next != shownNext || timedEngine.Waiting != shownWaiting))
            {
                forceRefresh = false;
                shownNext = timedEngine.Next;
                shownWaiting = timedEngine.Waiting;
                ShowTimedPrompt(shownNext, timedEngine.Waiting);
            }
            AnimateJudge(dt);
        }

        /// <summary>The feedback pops in, holds, then fades (the web version's CSS keyframes).</summary>
        void AnimateJudge(float dt)
        {
            if (judgeAge > 5f) return;
            judgeAge += dt;
            float duration = timed ? 0.9f : 0.7f;
            float k = judgeAge / duration;
            float opacity, scale = 1f, rise = 0f;
            if (Motion.Reduced)
            {
                opacity = k < 1 ? 1 : 0;
            }
            else if (k < 0.16f)
            {
                float a = k / 0.16f;
                opacity = a;
                scale = Mathf.Lerp(0.85f, 1f, a);
            }
            else if (k < 0.72f) opacity = 1f;
            else
            {
                float a = Mathf.Clamp01((k - 0.72f) / 0.28f);
                opacity = 1f - a;
                if (!timed) rise = -20f * a;
            }
            judgeLabel.style.opacity = opacity;
            judgeLabel.style.scale = new Scale(new Vector3(scale, scale, 1));
            judgeLabel.style.translate = new Translate(0, rise);
        }
    }
}
