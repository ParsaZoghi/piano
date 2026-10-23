using Keystage.Core;
using Keystage.Input;
using Keystage.Remote;
using UnityEngine;
using UnityEngine.UIElements;
using static Keystage.Core.I18n;

namespace Keystage.Ui
{
    /// <summary>"Ways to play": typing keyboard, MIDI keyboard and phone pairing (port of ui/devices.js).</summary>
    public class DevicesDialog
    {
        readonly VisualElement layer;
        VisualElement midiSection, phoneSection;
        Texture2D qrTexture;
        string qrFor;

        public bool IsOpen => layer.childCount > 0;

        public DevicesDialog(VisualElement layer)
        {
            this.layer = layer;
            MidiInput.Changed += RenderSections;
            RemoteServer.Changed += RenderSections;
        }

        public void Dispose()
        {
            MidiInput.Changed -= RenderSections;
            RemoteServer.Changed -= RenderSections;
            TypingInput.Blocked = false;
        }

        public void Open()
        {
            Build();
            TypingInput.Blocked = true;
        }

        public void Close()
        {
            layer.Clear();
            layer.style.display = DisplayStyle.None;
            TypingInput.Blocked = false;
        }

        void Build()
        {
            layer.Clear();
            layer.style.display = DisplayStyle.Flex;
            var scrim = UiKit.Div("overlay", layer, true);
            // Clicking the dim area outside the dialog closes it.
            scrim.RegisterCallback<PointerDownEvent>(e =>
            {
                if (e.target == scrim) Close();
            });
            var dialog = UiKit.Div("dialog col", scrim, true);

            var head = UiKit.Div("dialog__head row", dialog);
            UiKit.Text(T("devices.title"), "dialog__title", W.Display, head);
            var close = UiKit.IconBtn("close", T("common.close"), Close);
            head.Add(close);
            // The sections scroll when the window is too short for all of them.
            var scroll = new ScrollView(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            scroll.AddToClassList("dialog__body");
            dialog.Add(scroll);
            var body = scroll.contentContainer;

            var typing = Section(body, "keyboard", T("devices.typing"));
            UiKit.Text(T("devices.typingText", ("z", "<b>Z</b>"), ("x", "<b>X</b>"), ("space", "<b>Space</b>")), "muted text-start", W.Regular, typing);
            var keymap = UiKit.Div("col", typing);
            var top = UiKit.Div("keymap__row keymap__row--top", keymap);
            foreach (var k in new[] { "W", "E", "", "T", "Y", "U", "", "O", "P" })
                UiKit.Text(k, "keymap__cell" + (k == "" ? " keymap__cell--blank" : " keymap__cell--dark"), W.SemiBold, top);
            var home = UiKit.Div("keymap__row", keymap);
            foreach (var k in new[] { "A", "S", "D", "F", "G", "H", "J", "K", "L", ";" })
                UiKit.Text(k, "keymap__cell", W.SemiBold, home);
            var names = UiKit.Div("keymap__row", keymap);
            foreach (var n in new[] { "C", "D", "E", "F", "G", "A", "B", "C", "D", "E" })
                UiKit.Text(Notes.PitchClassName(n), "keymap__cell keymap__cell--name", W.SemiBold, names);
            if (IsRtl) keymap.style.alignSelf = Align.FlexEnd;

            midiSection = UiKit.Div("col", Section(body, "plug", T("devices.midi")));
            phoneSection = UiKit.Div("col", Section(body, "phone", T("devices.phone")));
            RenderSections();
            close.Focus();
        }

        static VisualElement Section(VisualElement dialog, string icon, string title)
        {
            var section = UiKit.Div("device col", dialog);
            var head = UiKit.Div("device__title row", section);
            head.Add(Icons.Create(icon));
            UiKit.Text(title, null, W.SemiBold, head);
            return section;
        }

        void RenderSections()
        {
            if (!IsOpen || midiSection == null) return;
            RenderMidi();
            RenderPhone();
        }

        void RenderMidi()
        {
            midiSection.Clear();
            switch (MidiInput.Status)
            {
                case MidiInput.State.Unsupported:
                    UiKit.Text(T("devices.midiUnsupported"), "muted text-start", W.Regular, midiSection);
                    return;
                case MidiInput.State.Denied:
                    UiKit.Text(T("devices.midiDenied"), "muted text-start", W.Regular, midiSection);
                    return;
                case MidiInput.State.Ready:
                    if (MidiInput.Devices.Count == 0)
                    {
                        Status(midiSection, T("devices.midiNone"), false);
                        return;
                    }
                    foreach (var name in MidiInput.Devices) Status(midiSection, name, true);
                    return;
                default:
                    UiKit.Text(T("devices.midiPlug"), "muted text-start", W.Regular, midiSection);
                    var b = UiKit.Btn("", MidiInput.Enable, null, MidiInput.Status == MidiInput.State.Pending ? T("devices.midiPending") : T("devices.midiEnable"));
                    b.style.alignSelf = IsRtl ? Align.FlexEnd : Align.FlexStart;
                    midiSection.Add(b);
                    return;
            }
        }

        void RenderPhone()
        {
            phoneSection.Clear();
            switch (RemoteServer.Status)
            {
                case RemoteServer.State.Off:
                    UiKit.Text(T("devices.phoneIntro"), "muted text-start", W.Regular, phoneSection);
                    var b = UiKit.Btn("", RemoteServer.Start, null, T("devices.phoneShow"));
                    b.style.alignSelf = IsRtl ? Align.FlexEnd : Align.FlexStart;
                    phoneSection.Add(b);
                    return;
                case RemoteServer.State.Unavailable:
                    UiKit.Text(T("devices.phoneUnavailable", ("port", RemoteServer.FirstPort.ToString())), "muted text-start", W.Regular, phoneSection);
                    return;
            }

            RemoteServer.RefreshUrl();
            var pair = UiKit.Div("pair", phoneSection);
            var qr = UiKit.Div("pair__qr", pair);
            qr.style.backgroundImage = new StyleBackground(QrTexture(RemoteServer.Url));
            var info = UiKit.Div("pair__info col", pair);
            UiKit.Text(T("devices.phoneScan"), "text-start", W.Regular, info);
            var url = UiKit.Text(RemoteServer.Url, "pair__url", W.SemiBold, info);
            url.style.unityFontDefinition = new StyleFontDefinition(Fonts.Get(W.SemiBold, latin: true));
            url.selection.isSelectable = true;
            Status(info, RemoteServer.Peers > 0 ? T("devices.phoneConnected", ("n", RemoteServer.Peers)) : T("devices.phoneWaiting"), RemoteServer.Peers > 0);
            UiKit.Text(T("devices.firewall"), "muted text-start", W.Regular, info).style.fontSize = 13;
        }

        static void Status(VisualElement parent, string text, bool on)
        {
            var row = UiKit.Div("status row", parent);
            UiKit.Div("dot" + (on ? " dot--on" : ""), row);
            UiKit.Text(text, "text-start", W.Regular, row);
        }

        Texture2D QrTexture(string text)
        {
            if (qrTexture != null && qrFor == text) return qrTexture;
            var code = QrCode.Encode(text);
            const int margin = 2;
            int size = code.Size + margin * 2;
            qrTexture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var dark = new Color32(15, 15, 35, 255);
            var light = new Color32(255, 255, 255, 255);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    int qx = x - margin, qy = y - margin;
                    bool on = qx >= 0 && qy >= 0 && qx < code.Size && qy < code.Size && code[qx, qy];
                    // Texture rows go bottom-up; QR rows go top-down.
                    qrTexture.SetPixel(x, size - 1 - y, on ? dark : light);
                }
            qrTexture.Apply();
            qrFor = text;
            return qrTexture;
        }
    }
}
