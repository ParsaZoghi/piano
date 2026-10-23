using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Keystage.Core;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using TextElement = UnityEngine.UIElements.TextElement;

namespace Keystage.Ui
{
    public enum W { Regular, Medium, SemiBold, Bold, Display }

    /// <summary>
    /// Fonts: Poppins and Righteous for English, Vazirmatn for Persian (the web
    /// version's fonts). UI Toolkit font assets are created at startup from the
    /// TTF files in Resources/Fonts, each falling back to the others.
    /// </summary>
    public static class Fonts
    {
        static FontAsset[] latin, persian;
        static FontAsset symbol;

        public static void Init()
        {
            // Font assets made in an earlier editor Play session may have been destroyed.
            if (latin != null && latin[0] != null && persian[0] != null) return;
            latin = new[] { Make("Poppins-Regular"), Make("Poppins-Medium"), Make("Poppins-SemiBold"), Make("Poppins-Bold"), Make("Righteous-Regular") };
            var vazirRegular = Make("Vazirmatn-Regular");
            var vazirMedium = Make("Vazirmatn-Medium");
            var vazirBold = Make("Vazirmatn-Bold");
            persian = new[] { vazirRegular, vazirMedium, vazirMedium, vazirBold, vazirBold };

            try
            {
                // Music symbols (treble clef, sharp) come from the system font on Windows.
                symbol = FontAsset.CreateFontAsset("Segoe UI Symbol", "Regular", 90);
            }
            catch (Exception)
            {
                symbol = null;
            }

            for (int i = 0; i < latin.Length; i++)
            {
                Fallback(latin[i], persian[i], symbol);
                Fallback(persian[i], latin[i], symbol);
            }
        }

        static FontAsset Make(string file)
        {
            var font = Resources.Load<Font>("Fonts/" + file);
            var asset = FontAsset.CreateFontAsset(font);
            asset.name = file;
            return asset;
        }

        static void Fallback(FontAsset asset, params FontAsset[] others)
        {
            if (asset == null) return;
            asset.fallbackFontAssetTable ??= new List<FontAsset>();
            foreach (var o in others)
                if (o != null && !asset.fallbackFontAssetTable.Contains(o)) asset.fallbackFontAssetTable.Add(o);
        }

        /// <summary>The font for a weight in the current language (latin: always the English font).</summary>
        public static FontDefinition Get(W weight, bool latin = false) =>
            FontDefinition.FromSDFFont((I18n.IsRtl && !latin ? persian : Fonts.latin)[(int)weight]);

        /// <summary>The music-symbol font (falls back to the regular UI font).</summary>
        public static StyleFontDefinition SymbolDefinition => new StyleFontDefinition(FontDefinition.FromSDFFont(symbol ?? latin[0]));

        public static void Apply(VisualElement e, W weight) => e.style.unityFontDefinition = new StyleFontDefinition(Get(weight));
    }

    /// <summary>Follows the Windows "Show animations" setting (the web version used prefers-reduced-motion).</summary>
    public static class Motion
    {
        static bool? reduced;

        public static bool Reduced
        {
            get
            {
                if (reduced.HasValue) return reduced.Value;
                reduced = false;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                try
                {
                    bool animations = true;
                    if (SystemParametersInfo(0x1042, 0, ref animations, 0)) reduced = !animations; // SPI_GETCLIENTAREAANIMATION
                }
                catch (Exception)
                {
                    // Not available: keep animations on.
                }
#endif
                return reduced.Value;
            }
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("user32.dll")]
        static extern bool SystemParametersInfo(uint action, uint param, ref bool value, uint winIni);
#endif
    }

    /// <summary>Small helpers for building UI Toolkit trees in code (the web version's ui/dom.js).</summary>
    public static class UiKit
    {
        static Texture2D fadeDown, fadeUp, vignette;

        public static T Classes<T>(this T e, string classes) where T : VisualElement
        {
            if (string.IsNullOrEmpty(classes)) return e;
            foreach (var c in classes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)) e.AddToClassList(c);
            return e;
        }

        public static T In<T>(this T e, VisualElement parent) where T : VisualElement
        {
            parent?.Add(e);
            return e;
        }

        public static VisualElement Div(string classes = null, VisualElement parent = null, bool pick = false)
        {
            var e = new VisualElement { pickingMode = pick ? PickingMode.Position : PickingMode.Ignore }.Classes(classes);
            parent?.Add(e);
            return e;
        }

        public static Label Text(string text, string classes = null, W weight = W.Regular, VisualElement parent = null)
        {
            var label = new Label(text) { pickingMode = PickingMode.Ignore, enableRichText = true }.Classes(classes);
            Fonts.Apply(label, weight);
            parent?.Add(label);
            return label;
        }

        static Button BareButton(string classes, Action click)
        {
            var b = new Button(click);
            b.RemoveFromClassList(Button.ussClassName);
            b.RemoveFromClassList(TextElement.ussClassName);
            b.Classes(classes);
            b.text = "";
            return b;
        }

        /// <summary>A button with an optional icon and text (icon first in reading order).</summary>
        public static Button Btn(string classes, Action click, string icon = null, string text = null, string tooltip = null, bool iconAfter = false)
        {
            var b = BareButton("btn " + classes, click);
            if (icon != null && !iconAfter) b.Add(Icons.Create(icon));
            if (!string.IsNullOrEmpty(text))
            {
                var label = Text(text, null, W.SemiBold, b);
                if (icon != null)
                {
                    float m = 8;
                    if (I18n.IsRtl ^ iconAfter) label.style.marginRight = m;
                    else label.style.marginLeft = m;
                }
            }
            if (icon != null && iconAfter) b.Add(Icons.Create(icon));
            if (tooltip != null) b.tooltip = tooltip;
            return b;
        }

        public static Button IconBtn(string icon, string label, Action click, string classes = null)
        {
            var b = BareButton("btn icon-btn " + classes, click);
            b.Add(Icons.Create(icon));
            b.tooltip = label;
            return b;
        }

        /// <summary>An on/off pill whose dot says the state as well as the colour does.</summary>
        public static Button Chip(string text, bool on, Action click)
        {
            var b = BareButton("chip", click);
            Div("chip__dot", b);
            Text(text, null, W.Medium, b);
            SetOn(b, on);
            return b;
        }

        public static void SetOn(VisualElement e, bool on) => e.EnableInClassList("on", on);

        /// <summary>A row of mutually exclusive options.</summary>
        public static VisualElement Segmented<T>(IList<(T value, string text)> options, T current, Action<T> select, bool forceLtr = false)
        {
            var group = Div("segmented");
            if (forceLtr)
            {
                group.AddToClassList("row--ltr");
                group.languageDirection = LanguageDirection.LTR;
            }
            var buttons = new List<Button>();
            foreach (var (value, text) in options)
            {
                var v = value;
                Button b = null;
                b = BareButton("segmented__item", () =>
                {
                    foreach (var x in buttons) SetOn(x, x == b);
                    select(v);
                });
                Text(text, null, W.SemiBold, b);
                SetOn(b, EqualityComparer<T>.Default.Equals(v, current));
                buttons.Add(b);
                group.Add(b);
            }
            if (I18n.IsRtl && !forceLtr) group.style.flexDirection = FlexDirection.RowReverse;
            return group;
        }

        /// <summary>Set start-side margins between children (USS has no gap property).</summary>
        public static void Gap(VisualElement row, float px)
        {
            int n = row.childCount;
            for (int i = 0; i < n; i++)
            {
                var c = row[i];
                bool last = i == n - 1;
                c.style.marginRight = !I18n.IsRtl && !last ? px : StyleKeyword.Null;
                c.style.marginLeft = I18n.IsRtl && !last ? px : StyleKeyword.Null;
            }
        }

        /// <summary>Texture-based fades (white, tinted with the theme background in USS).</summary>
        public static void FadeDown(VisualElement e) => SetFade(e, fadeDown ??= Gradient(true));
        public static void FadeUp(VisualElement e) => SetFade(e, fadeUp ??= Gradient(false));

        public static void Vignette(VisualElement e)
        {
            if (vignette == null)
            {
                const int size = 128;
                vignette = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        // An ellipse centred near the top: opaque in the middle, thinner outside.
                        float dx = (x - size / 2f) / (size * 0.45f), dy = (y - size * 0.72f) / (size * 0.35f);
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        float a = Mathf.Lerp(0.9f, 0.25f, Mathf.Clamp01((d - 0.3f) / 0.7f));
                        vignette.SetPixel(x, y, new Color(1, 1, 1, a));
                    }
                vignette.Apply();
            }
            e.style.backgroundImage = new StyleBackground(vignette);
            e.AddToClassList("vignette");
        }

        static void SetFade(VisualElement e, Texture2D t)
        {
            e.style.backgroundImage = new StyleBackground(t);
            e.AddToClassList("fade");
        }

        static Texture2D Gradient(bool opaqueAtTop)
        {
            const int h = 64;
            var t = new Texture2D(1, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < h; y++)
            {
                // Texture rows go bottom-up; the web fades were solid for the first ~half.
                float fromTop = 1f - y / (float)(h - 1);
                float along = opaqueAtTop ? fromTop : 1f - fromTop;
                float a = along < 0.5f ? 0.94f : Mathf.Lerp(0.94f, 0f, (along - 0.5f) / 0.5f);
                t.SetPixel(0, y, new Color(1, 1, 1, a));
            }
            t.Apply();
            return t;
        }
    }
}
