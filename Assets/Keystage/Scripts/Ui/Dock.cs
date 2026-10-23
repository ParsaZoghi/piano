using System.Collections.Generic;
using Keystage.Core;
using Keystage.Scene;
using UnityEngine;
using UnityEngine.UIElements;
using static Keystage.Core.I18n;

namespace Keystage.Ui
{
    /// <summary>
    /// The controls shown on every page: theme toggle, camera controls and a
    /// settings menu (port of ui/dock.js). Lives outside the screens so it
    /// survives navigation.
    /// </summary>
    public class Dock
    {
        readonly PianoStage stage;
        readonly System.Action openDevices;
        public readonly VisualElement Element;
        string open; // id of the open popover, or null

        public Dock(PianoStage stage, System.Action openDevices)
        {
            this.stage = stage;
            this.openDevices = openDevices;
            Element = UiKit.Div("dock", null, true);
            Render();
        }

        public bool IsOpen => open != null;

        public void Close()
        {
            open = null;
            Render();
        }

        public void Render()
        {
            Element.Clear();
            bool dark = Settings.Data.theme == "dark";
            string themeLabel = dark ? T("dock.toLight") : T("dock.toDark");
            Element.Add(UiKit.IconBtn(dark ? "sun" : "moon", themeLabel, () => Settings.Update(d => d.theme = d.theme == "dark" ? "light" : "dark", "theme")));

            if (stage.Interactive)
            {
                var camWrap = UiKit.Div("popwrap", Element, true);
                var camButton = UiKit.IconBtn("move", T("dock.camera"), () => Toggle("camera"), open == "camera" ? "open" : null);
                camWrap.Add(camButton);
                if (open == "camera") camWrap.Add(CameraPanel());
            }

            var setWrap = UiKit.Div("popwrap", Element, true);
            setWrap.Add(UiKit.IconBtn("gear", T("dock.settings"), () => Toggle("settings"), open == "settings" ? "open" : null));
            if (open == "settings") setWrap.Add(SettingsPanel());
            UiKit.Gap(Element, 8);
        }

        void Toggle(string id)
        {
            open = open == id ? null : id;
            Render();
        }

        VisualElement CameraPanel()
        {
            var panel = UiKit.Div("popover popover--camera col", null, true);
            UiKit.Text(T("camera.title"), "popover__title text-start", W.SemiBold, panel);
            var pad = UiKit.Div("dpad", panel);
            void Place(string icon, string label, System.Action act, float x, float y)
            {
                var b = UiKit.IconBtn(icon, label, act);
                b.style.left = x;
                b.style.top = y;
                pad.Add(b);
            }
            Place("up", T("camera.up"), () => stage.Step(0, 1), 50, 0);
            Place("left", T("camera.left"), () => stage.Step(-1, 0), 0, 50);
            Place("home", T("camera.reset") + " (0)", stage.ResetCamera, 50, 50);
            Place("right", T("camera.right"), () => stage.Step(1, 0), 100, 50);
            Place("down", T("camera.down"), () => stage.Step(0, -1), 50, 100);

            var zoom = UiKit.Div("row row--ltr", panel);
            zoom.style.marginTop = 12;
            zoom.Add(UiKit.Btn("btn--ghost", () => stage.ZoomBy(1.2f), "zoomOut", null, T("camera.zoomOut")));
            zoom.Add(UiKit.Btn("btn--ghost", () => stage.ZoomBy(1 / 1.2f), "zoomIn", null, T("camera.zoomIn")));
            zoom[0].style.marginRight = 8;
            UiKit.Text(T("camera.hint"), "popover__hint text-start", W.Regular, panel);
            return panel;
        }

        VisualElement SettingsPanel()
        {
            var panel = UiKit.Div("popover col", null, true);
            UiKit.Text(T("settings.title"), "popover__title text-start", W.SemiBold, panel);

            Field(panel, T("settings.language"), UiKit.Segmented(new List<(string, string)> { ("en", "English"), ("fa", "فارسی") }, Settings.Data.lang,
                v => Settings.Update(d => d.lang = v, "lang")));
            Field(panel, T("settings.noteNames"), UiKit.Segmented(new List<(string, string)> { ("letters", "C D E"), ("solfege", "Do Re Mi") }, Settings.Data.naming,
                v => Settings.Update(d => d.naming = v, "naming")));
            Field(panel, T("settings.theme"), UiKit.Segmented(new List<(string, string)> { ("dark", T("settings.dark")), ("light", T("settings.light")) }, Settings.Data.theme,
                v => Settings.Update(d => d.theme = v, "theme")));

            var labelsRow = UiKit.Div("row field field--row", panel);
            UiKit.Text(T("settings.labels"), "field__label", W.SemiBold, labelsRow);
            Button sw = null;
            sw = new Button(() =>
            {
                Settings.Update(d => d.labels = !d.labels, "labels");
                UiKit.SetOn(sw, Settings.Data.labels);
            });
            sw.RemoveFromClassList(Button.ussClassName);
            sw.text = "";
            sw.AddToClassList("switch");
            sw.tooltip = T("settings.labels");
            var knob = UiKit.Div("switch__knob", sw);
            if (IsRtl)
            {
                knob.style.left = StyleKeyword.Auto;
                knob.style.right = 3;
            }
            UiKit.SetOn(sw, Settings.Data.labels);
            labelsRow.Add(sw);

            var volumeField = UiKit.Div("field col", panel);
            UiKit.Text(T("settings.volume"), "field__label text-start", W.SemiBold, volumeField);
            var slider = new Slider(0f, 1f) { value = Settings.Data.volume };
            slider.style.flexGrow = 1;
            slider.RegisterValueChangedCallback(e => Settings.Update(d => d.volume = e.newValue, "volume"));
            var volumeRow = UiKit.Div("row", volumeField);
            volumeRow.Add(Icons.Create("volume"));
            volumeRow.Add(slider);
            UiKit.Gap(volumeRow, 8);

            panel.Add(UiKit.Btn("btn--ghost", () =>
            {
                Close();
                openDevices();
            }, "plug", T("settings.devices")));
            return panel;
        }

        static void Field(VisualElement panel, string label, VisualElement control)
        {
            var field = UiKit.Div("field col", panel);
            UiKit.Text(label, "field__label text-start", W.SemiBold, field);
            field.Add(control);
        }
    }
}
