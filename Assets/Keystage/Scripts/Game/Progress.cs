using System;
using System.Collections.Generic;
using UnityEngine;

namespace Keystage.Game
{
    /// <summary>Stars and best accuracy per lesson, saved in PlayerPrefs (port of game/progress.js).</summary>
    public static class Progress
    {
        const string Key = "keystage.progress";

        [Serializable]
        class Entry
        {
            public string id;
            public int stars;
            public float best;
        }

        [Serializable]
        class Store
        {
            public List<Entry> entries = new List<Entry>();
        }

        static Store data;

        static Store Data
        {
            get
            {
                if (data != null) return data;
                try
                {
                    data = JsonUtility.FromJson<Store>(PlayerPrefs.GetString(Key, "")) ?? new Store();
                }
                catch (Exception)
                {
                    data = new Store();
                }
                return data;
            }
        }

        static Entry Find(string id) => Data.entries.Find(e => e.id == id);

        public static int StarsFor(float accuracy) => accuracy >= 0.9f ? 3 : accuracy >= 0.75f ? 2 : accuracy >= 0.5f ? 1 : 0;

        public static int Stars(string id) => Find(id)?.stars ?? 0;

        /// <summary>A lesson opens once the one before it has at least one star.</summary>
        public static bool IsUnlocked(int index) => index <= 0 || Stars(Lessons.All[index - 1].id) > 0;

        public static int TotalStars()
        {
            int sum = 0;
            foreach (var lesson in Lessons.All) sum += Stars(lesson.id);
            return sum;
        }

        public static int Record(string id, float accuracy)
        {
            int stars = StarsFor(accuracy);
            var entry = Find(id);
            if (entry == null) Data.entries.Add(entry = new Entry { id = id });
            entry.stars = Math.Max(entry.stars, stars);
            entry.best = Math.Max(entry.best, accuracy);
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
            return stars;
        }

        /// <summary>Test helper: give every lesson one star so all of them open.</summary>
        public static void UnlockAllForTesting()
        {
            foreach (var lesson in Lessons.All)
                if (Stars(lesson.id) == 0) Record(lesson.id, 0.5f);
        }

        /// <summary>Forget the cached copy (used after PlayerPrefs are cleared).</summary>
        public static void Reload() => data = null;
    }
}
