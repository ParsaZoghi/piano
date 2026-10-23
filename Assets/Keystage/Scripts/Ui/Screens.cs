using System;
using System.Collections.Generic;
using Keystage.Core;
using Keystage.Game;
using Keystage.Input;
using Keystage.Scene;
using UnityEngine;
using UnityEngine.UIElements;
using static Keystage.Core.I18n;

namespace Keystage.Ui
{
    /// <summary>What every screen gets from the app.</summary>
    public class ScreenContext
    {
        public PianoStage Stage;
        public VisualElement Layer; // where screens put their UI
        public Action<string> Navigate; // "menu", "free", "lessons", "lesson/n1"
        public Action OpenDevices;
    }

    /// <summary>The first page: a free play button and a lessons button (port of ui/menu.js).</summary>
    public static class MenuScreen
    {
        public static Action Mount(ScreenContext ctx)
        {
            ctx.Stage.Configure(view: View.Menu);
            ctx.Stage.Keyboard.SetLabels(false, null);
            TypingInput.Configure();

            int earned = Progress.TotalStars();
            var view = UiKit.Div("layer menu col", ctx.Layer, true);
            UiKit.Vignette(view);

            var logo = UiKit.Div("menu__logo", view);
            logo.Add(Icons.Create("music"));
            var title = UiKit.Text("Keystage", "menu__title", W.Display, view);
            title.style.unityFontDefinition = new StyleFontDefinition(Fonts.Get(W.Display, latin: true));
            UiKit.Text(T("menu.tagline"), "menu__tagline", W.Regular, view);

            Choice(view, "play", T("menu.free"), T("menu.freeSub"), false, () => ctx.Navigate("free"));
            string sub = T("menu.learnSub", ("count", Lessons.All.Length));
            if (earned > 0) sub += " " + T("menu.stars", ("earned", earned), ("total", Lessons.All.Length * 3));
            Choice(view, "book", T("menu.learn"), sub, true, () => ctx.Navigate("lessons"));

            var connect = UiKit.Btn("btn--ghost", ctx.OpenDevices, "plug", T("menu.connect"));
            connect.style.marginTop = 14;
            view.Add(connect);
            return () => view.RemoveFromHierarchy();
        }

        static void Choice(VisualElement parent, string icon, string title, string sub, bool primary, Action click)
        {
            var b = new Button(click);
            b.RemoveFromClassList(Button.ussClassName);
            b.RemoveFromClassList(TextElement.ussClassName);
            b.text = "";
            b.Classes("choice" + (primary ? " choice--primary" : ""));
            var iconBox = UiKit.Div("choice__icon", b);
            iconBox.Add(Icons.Create(icon));
            var text = UiKit.Div("grow col", b);
            UiKit.Text(title, "choice__title text-start", W.SemiBold, text);
            UiKit.Text(sub, "choice__sub text-start", W.Regular, text);
            b.Add(Icons.Create("next", "choice__arrow"));
            parent.Add(b);
        }
    }

    /// <summary>The full 88-key piano with no goals (port of ui/freePlay.js).</summary>
    public static class FreePlayScreen
    {
        public static Action Mount(ScreenContext ctx)
        {
            ctx.Stage.Configure(Notes.PianoMin, Notes.PianoMax, View.Free);
            TypingInput.Configure(true, true);

            var view = UiKit.Div("layer", ctx.Layer);
            var topbar = UiKit.Div("topbar", view, true);
            UiKit.FadeDown(topbar);
            topbar.Add(UiKit.IconBtn("back", T("common.backMenu"), () => ctx.Navigate("menu")));
            var titleBox = UiKit.Div("topbar__title col", topbar);
            UiKit.Text(T("free.sub"), "eyebrow text-start", W.SemiBold, titleBox);
            UiKit.Text(T("free.title"), "topbar__title-text text-start", W.SemiBold, titleBox);

            var tools = UiKit.Div("row", topbar);
            var stepper = UiKit.Div("row row--ltr", tools);
            stepper.Add(UiKit.IconBtn("minus", T("free.octaveDown"), () => TypingInput.SetShift(TypingInput.Shift - 1)));
            var octave = UiKit.Text("", "stepper__value", W.SemiBold, stepper);
            stepper.Add(UiKit.IconBtn("plus", T("free.octaveUp"), () => TypingInput.SetShift(TypingInput.Shift + 1)));
            UiKit.Gap(stepper, 6);
            tools.Add(UiKit.Btn("btn--ghost", ctx.OpenDevices, "plug", T("common.devices")));
            UiKit.Gap(tools, 10);

            var hint = UiKit.Div("hint-bar", view, true);
            void Keys(params string[] keys)
            {
                foreach (var k in keys) UiKit.Text(k, "kbd", W.SemiBold, hint);
            }
            void Words(string text) => UiKit.Text(text, null, W.Regular, hint);
            Keys("A");
            Words("–");
            Keys(";");
            Words(T("free.white") + " ·");
            Keys("W", "E", "T", "Y", "U");
            Words(T("free.black") + " ·");
            Keys("Z", "X");
            Words(T("free.octaveKeys") + " ·");
            Keys("Space");
            Words(T("free.sustain") + " · " + T("free.other"));
            if (IsRtl) hint.style.flexDirection = FlexDirection.RowReverse;

            void Refresh()
            {
                octave.text = Notes.PitchClassName("C") + TypingInput.BaseOctave;
                ctx.Stage.Keyboard.SetLabels(Settings.Data.labels, Settings.Data.labels ? TypingInput.LabelMap() : null);
            }
            void OnSettings(string[] fields)
            {
                if (Settings.Has(fields, "labels")) Refresh();
            }
            TypingInput.Changed += Refresh;
            Settings.Changed += OnSettings;
            Refresh();

            return () =>
            {
                TypingInput.Changed -= Refresh;
                Settings.Changed -= OnSettings;
                view.RemoveFromHierarchy();
            };
        }
    }

    /// <summary>All lessons grouped by unit, with stars and locks (port of ui/lessonList.js).</summary>
    public static class LessonListScreen
    {
        const float CardMin = 304f;
        const float CardGap = 12f;

        public static Action Mount(ScreenContext ctx)
        {
            ctx.Stage.Configure(view: View.Menu);
            ctx.Stage.Keyboard.SetLabels(false, null);
            TypingInput.Configure();

            var view = UiKit.Div("layer page col", ctx.Layer, true);
            var topbar = UiKit.Div("topbar topbar--page", view, true);
            topbar.Add(UiKit.IconBtn("back", T("common.backMenu"), () => ctx.Navigate("menu")));
            var titleBox = UiKit.Div("topbar__title col", topbar);
            UiKit.Text(T("list.eyebrow"), "eyebrow text-start", W.SemiBold, titleBox);
            UiKit.Text(T("list.title"), "topbar__title-text text-start", W.SemiBold, titleBox);
            var stat = UiKit.Div("stat col", topbar);
            UiKit.Text(T("list.stars"), "stat__label", W.SemiBold, stat);
            UiKit.Text($"{Num(Progress.TotalStars())} / {Num(Lessons.All.Length * 3)}", "stat__value", W.Bold, stat);

            var scroll = new ScrollView(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            scroll.AddToClassList("page__body");
            view.Add(scroll);
            var inner = UiKit.Div("page__inner col", scroll.contentContainer);

            var cards = new List<VisualElement>();
            Button firstOpen = null;
            for (int u = 0; u < Lessons.Units.Length; u++)
            {
                string unit = Lessons.Units[u];
                var section = UiKit.Div("unit col", inner);
                UiKit.Text(T("list.unit", ("n", u + 1)), "eyebrow text-start", W.SemiBold, section);
                UiKit.Text(T($"units.{unit}.title"), "unit__title text-start", W.Display, section);
                UiKit.Text(T($"units.{unit}.blurb"), "unit__blurb text-start", W.Regular, section);
                var list = UiKit.Div("unit__list", section);
                for (int i = 0; i < Lessons.All.Length; i++)
                {
                    var lesson = Lessons.All[i];
                    if (lesson.unit != unit) continue;
                    var card = Card(ctx, lesson, i);
                    if (firstOpen == null && card is Button b && Progress.IsUnlocked(i) && Progress.Stars(lesson.id) == 0) firstOpen = b;
                    list.Add(card);
                    cards.Add(card);
                }
            }

            // Cards flow into as many columns as fit (the web version's auto-fill grid).
            void Columns()
            {
                float width = inner.contentRect.width;
                if (float.IsNaN(width) || width <= 0) return;
                int cols = Mathf.Max(1, Mathf.FloorToInt((width + CardGap) / (CardMin + CardGap)));
                // Every card carries a CardGap margin on its trailing side (USS has no gap).
                float cardWidth = (width - CardGap * cols) / cols - 0.5f;
                for (int i = 0; i < cards.Count; i++) cards[i].style.width = cardWidth;
            }
            inner.RegisterCallback<GeometryChangedEvent>(_ => Columns());
            return () => view.RemoveFromHierarchy();
        }

        static VisualElement Card(ScreenContext ctx, Lesson lesson, int index)
        {
            bool unlocked = Progress.IsUnlocked(index);
            VisualElement card;
            if (unlocked)
            {
                var b = new Button(() => ctx.Navigate("lesson/" + lesson.id));
                b.RemoveFromClassList(Button.ussClassName);
                b.RemoveFromClassList(TextElement.ussClassName);
                b.text = "";
                card = b;
            }
            else
            {
                card = UiKit.Div(null, null, true);
                card.tooltip = T("list.lockedTitle", ("n", index));
            }
            card.Classes("lesson" + (unlocked ? "" : " lesson--locked"));
            UiKit.Text(Num(index + 1), "lesson__num", W.Bold, card);
            var text = UiKit.Div("lesson__text col", card);
            UiKit.Text(T("mode." + lesson.ModeKey), "eyebrow text-start", W.SemiBold, text);
            UiKit.Text(Lessons.Title(lesson), "text-start", W.SemiBold, text);
            UiKit.Text(Lessons.Goal(lesson), "lesson__goal text-start", W.Regular, text);
            var state = UiKit.Div("lesson__state", card);
            if (unlocked)
            {
                int stars = Progress.Stars(lesson.id);
                for (int s = 0; s < 3; s++) state.Add(Icons.Star(s < stars));
            }
            else state.Add(Icons.Create("lock"));
            return card;
        }
    }
}
