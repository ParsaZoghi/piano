using System;
using UnityEngine;

namespace Keystage.Core
{
    /// <summary>The learner's preferences, saved in PlayerPrefs as JSON (port of core/settings.js).</summary>
    [Serializable]
    public class SettingsData
    {
        public float volume = 0.8f;
        public bool labels = true;
        public bool hints = true;
        public bool metronome = true;
        public bool wait = false;
        public float speed = 1f;
        public string theme = "dark"; // dark | light
        public string lang = "en"; // en | fa
        public string naming = "letters"; // letters (C D E) | solfege (Do Re Mi)
    }

    public static class Settings
    {
        const string Key = "keystage.settings";

        public static SettingsData Data { get; private set; } = Load();

        /// <summary>Runs after every change with the names of the fields that changed.</summary>
        public static event Action<string[]> Changed;

        static SettingsData Load()
        {
            try
            {
                var json = PlayerPrefs.GetString(Key, "");
                if (!string.IsNullOrEmpty(json)) return JsonUtility.FromJson<SettingsData>(json) ?? new SettingsData();
            }
            catch (Exception)
            {
                // A broken saved value just means starting from the defaults.
            }
            return new SettingsData();
        }

        /// <summary>Apply a change, save it and tell listeners which fields changed.</summary>
        public static void Update(Action<SettingsData> change, params string[] fields)
        {
            change(Data);
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
            Changed?.Invoke(fields);
        }

        public static bool Has(string[] fields, string name) => Array.IndexOf(fields, name) >= 0;
    }
}
