using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Keystage.Core
{
    /// <summary>Translations and number formatting (port of core/i18n.js).</summary>
    public static class I18n
    {
        public static readonly (string id, string label)[] Languages = { ("en", "English"), ("fa", "فارسی") };

        static readonly Regex NoteToken = new Regex(@"\{n:([A-G]#?)\}");
        static readonly Regex VarToken = new Regex(@"\{(\w+)\}");

        public static string Language { get; private set; } = "en";
        public static bool IsRtl => Language == "fa";

        public static void SetLanguage(string value) => Language = value == "fa" ? "fa" : "en";

        static Dictionary<string, string> Text => Language == "fa" ? I18nData.FaText : I18nData.EnText;
        static Dictionary<string, string[]> Lists => Language == "fa" ? I18nData.FaLists : I18nData.EnLists;

        /// <summary>Numbers in the digits of the current language (۱۲۳ in Persian).</summary>
        public static string Num(double value)
        {
            string text = value.ToString(CultureInfo.InvariantCulture);
            if (Language != "fa") return text;
            var chars = text.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                if (chars[i] >= '0' && chars[i] <= '9') chars[i] = (char)('۰' + (chars[i] - '0'));
            return new string(chars);
        }

        /// <summary>
        /// Translate a dotted key, falling back to English and then to the key.
        /// {name} is filled from vars (numbers use the language's digits); {n:C}
        /// becomes a note name in the learner's chosen naming (C or Do).
        /// </summary>
        public static string T(string key, params (string name, object value)[] vars)
        {
            if (!Text.TryGetValue(key, out var text) && !I18nData.EnText.TryGetValue(key, out text)) return key;
            return Fill(text, vars);
        }

        public static string[] List(string key, params (string name, object value)[] vars)
        {
            if (!Lists.TryGetValue(key, out var list) && !I18nData.EnLists.TryGetValue(key, out list)) return new string[0];
            var result = new string[list.Length];
            for (int i = 0; i < list.Length; i++) result[i] = Fill(list[i], vars);
            return result;
        }

        public static bool Has(string key) => I18nData.EnText.ContainsKey(key) || I18nData.EnLists.ContainsKey(key);

        static string Fill(string text, (string name, object value)[] vars)
        {
            text = NoteToken.Replace(text, m => Notes.PitchClassName(m.Groups[1].Value));
            if (vars == null || vars.Length == 0) return text;
            return VarToken.Replace(text, m =>
            {
                foreach (var (name, value) in vars)
                {
                    if (name != m.Groups[1].Value) continue;
                    if (value is int i) return Num(i);
                    if (value is float f) return Num(f);
                    if (value is double d) return Num(d);
                    return value?.ToString() ?? "";
                }
                return m.Value;
            });
        }
    }
}
