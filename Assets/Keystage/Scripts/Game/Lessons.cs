using System;
using System.Collections.Generic;
using Keystage.Core;
using Random = UnityEngine.Random;

namespace Keystage.Game
{
    public enum LessonMode { Find, Rhythm, Play }

    public enum Hint { Key, Name, Staff }

    public class Lesson
    {
        public string id;
        public string unit;
        public LessonMode mode;
        public string[] range; // null for songs: worked out from the notes
        public string[] pool;
        public string[] focus;
        public (Hint hint, int count)[] rounds;
        public string lane;
        public int bpm;
        public int meter = 4; // beats per bar
        public string song;

        public string ModeKey => mode == LessonMode.Find ? "find" : mode == LessonMode.Rhythm ? "rhythm" : "play";
    }

    public struct SongNote
    {
        public int midi;
        public float beat;
        public float beats;
    }

    /// <summary>
    /// The curriculum: notes first, rhythm second, then both together in songs
    /// (port of game/lessons.js). Titles, goals and tips live in I18nData under
    /// lessons.&lt;id&gt;. Songs are "NOTE:beats" tokens: "R" is a rest, "X" a rhythm
    /// tap, "|" only marks bars. All songs are public-domain melodies.
    /// </summary>
    public static class Lessons
    {
        public static readonly string[] Units = { "notes", "rhythm", "songs" };

        static readonly string[] White5 = { "C4", "D4", "E4", "F4", "G4" };
        static readonly string[] White8 = { "C4", "D4", "E4", "F4", "G4", "A4", "B4", "C5" };

        static Lesson Find(string id, string lo, string hi, string[] pool, string[] focus, params (Hint, int)[] rounds) =>
            new Lesson { id = id, unit = "notes", mode = LessonMode.Find, range = new[] { lo, hi }, pool = pool, focus = focus, rounds = rounds };

        static Lesson Rhythm(string id, int bpm, string song) =>
            new Lesson { id = id, unit = "rhythm", mode = LessonMode.Rhythm, range = new[] { "C4", "E4" }, lane = "D4", bpm = bpm, song = song };

        static Lesson Song(string id, int bpm, string song, int meter = 4) =>
            new Lesson { id = id, unit = "songs", mode = LessonMode.Play, bpm = bpm, song = song, meter = meter };

        public static readonly Lesson[] All =
        {
            Find("n1", "C4", "E4", new[] { "C4", "D4", "E4" }, null, (Hint.Key, 6), (Hint.Name, 6), (Hint.Staff, 6)),
            Find("n2", "C4", "G4", White5, new[] { "F4", "G4" }, (Hint.Key, 6), (Hint.Name, 8), (Hint.Staff, 8)),
            Find("n3", "C4", "G4", White5, null, (Hint.Name, 8), (Hint.Staff, 10)),
            Find("n4", "C4", "C5", White8, new[] { "A4", "B4", "C5" }, (Hint.Key, 6), (Hint.Name, 8), (Hint.Staff, 10)),
            Find("n5", "C4", "C5", new[] { "C#4", "D#4", "F#4", "G#4", "A#4" }, null, (Hint.Key, 5), (Hint.Name, 10), (Hint.Staff, 6)),
            Find("n6", "C4", "E5", new[] { "C4", "D4", "E4", "F4", "G4", "A4", "B4", "C5", "D5", "E5" }, null, (Hint.Name, 8), (Hint.Staff, 14)),

            Rhythm("r1", 76, "X X X X | X X X X | X X X X | X X X X"),
            Rhythm("r2", 80, "X:2 X:2 | X X X:2 | X:2 X X | X:4 | X:2 X:2 | X X X X | X:2 X:2 | X:4"),
            Rhythm("r3", 72, "X X X:.5 X:.5 X | X:.5 X:.5 X:.5 X:.5 X X | X X:.5 X:.5 X X:.5 X:.5 | X:.5 X:.5 X X:2 | X:.5 X:.5 X:.5 X:.5 X:.5 X:.5 X | X:2 X:2"),
            Rhythm("r4", 80, "X R X R | X X R X | R X R X | X:2 R:2 | X R X X | R X X R | X X R X | X:2 R:2"),

            Song("s1", 84, "E4 D4 C4:2 | E4 D4 C4:2 | C4:.5 C4:.5 C4:.5 C4:.5 D4:.5 D4:.5 D4:.5 D4:.5 | E4 D4 C4:2"),
            Song("s7", 88, "G4 E4 G4:.5 G4:.5 E4 | G4:.5 G4:.5 E4:.5 A4:.5 G4:.5 G4:.5 E4 | G4 E4 G4:.5 G4:.5 E4 | G4:.5 G4:.5 E4:.5 A4:.5 G4:.5 G4:.5 E4"),
            Song("s8", 96, "C4 C4 C4 D4 | E4:2 D4:2 | C4 E4 D4 D4 | C4:4 | C4 C4 C4 D4 | E4:2 D4:2 | C4 E4 D4 D4 | C4:4"),
            Song("s2", 92, "E4 D4 C4 D4 | E4 E4 E4:2 | D4 D4 D4:2 | E4 G4 G4:2 | E4 D4 C4 D4 | E4 E4 E4 E4 | D4 D4 E4 D4 | C4:4"),
            Song("s9", 96, "E4:2 E4 D4 | C4:2 C4:2 | D4:2 D4 F4 | E4 D4 C4:2 | G4:2 G4 F4 | E4:2 E4:2 | C4 D4 E4 D4 | C4:4"),
            Song("s10", 100, "G4 E4 E4:2 | F4 D4 D4:2 | C4 D4 E4 F4 | G4 G4 G4:2 | G4 E4 E4:2 | F4 D4 D4:2 | C4 E4 G4 G4 | E4:4 | D4 D4 D4 D4 | D4 E4 F4:2 | E4 E4 E4 E4 | E4 F4 G4:2 | G4 E4 E4:2 | F4 D4 D4:2 | C4 E4 G4 G4 | C4:4"),
            Song("s3", 96, "E4 E4 F4 G4 | G4 F4 E4 D4 | C4 C4 D4 E4 | E4:1.5 D4:.5 D4:2 | E4 E4 F4 G4 | G4 F4 E4 D4 | C4 C4 D4 E4 | D4:1.5 C4:.5 C4:2"),
            Song("s11", 100, "C4:2 C4 D4 | E4:2 E4 F4 | G4:2 A4 G4 | E4:4 | G4:2 F4 E4 | D4:4 | F4:2 E4 D4 | C4:4"),
            Song("s12", 92, "C4 F4 E4 F4 | G4 D4 G4:2 | F4 E4 D4 E4 | F4:4 | C4 F4 E4 F4 | G4 D4 G4:2 | F4 E4 D4 E4 | F4:4"),
            Song("s4", 96, "C4 C4 G4 G4 | A4 A4 G4:2 | F4 F4 E4 E4 | D4 D4 C4:2 | G4 G4 F4 F4 | E4 E4 D4:2 | G4 G4 F4 F4 | E4 E4 D4:2 | C4 C4 G4 G4 | A4 A4 G4:2 | F4 F4 E4 E4 | D4 D4 C4:2"),
            Song("s13", 104, "G4:1.5 A4:.5 G4 F4 | E4 F4 G4:2 | D4 E4 F4:2 | E4 F4 G4:2 | G4:1.5 A4:.5 G4 F4 | E4 F4 G4:2 | D4:2 G4:2 | E4 C4:3"),
            Song("s14", 76, "E4:1.5 G4:.5 G4:2 | E4:1.5 D4:.5 C4:2 | D4:1.5 E4:.5 G4:1.5 E4:.5 | D4:4 | E4:1.5 G4:.5 G4:2 | E4:1.5 D4:.5 C4:2 | D4 E4 D4:1.5 C4:.5 | C4:4"),
            Song("s5", 100, "E4 E4 E4:2 | E4 E4 E4:2 | E4 G4 C4:1.5 D4:.5 | E4:4 | F4 F4 F4:1.5 F4:.5 | F4 E4 E4 E4:.5 E4:.5 | E4 D4 D4 E4 | D4:2 G4:2"),
            Song("s15", 108, "R C4 E4 F4 | G4:4 | R C4 E4 F4 | G4:4 | R C4 E4 F4 | G4:2 E4:2 | C4:2 E4:2 | D4:4 | R E4 E4 D4 | C4:3 C4 | E4:2 G4:2 | G4 G4 F4:2 | R:2 E4 F4 | G4:2 E4:2 | C4:2 D4:2 | C4:4"),
            Song("s6", 88, "C4 D4 E4 F4 | G4 A4 B4 C5 | C5 B4 A4 G4 | F4 E4 D4 C4 | C4:.5 D4:.5 E4:.5 F4:.5 G4:.5 A4:.5 B4:.5 C5:.5 | C5:.5 B4:.5 A4:.5 G4:.5 F4:.5 E4:.5 D4:.5 C4:.5 | C4:4"),
            Song("s16", 96, "G4 E4 G4:2 | G4 E4 G4:2 | A4 G4 F4 E4 | D4 E4 F4:2 | E4:.5 F4:.5 G4 C4:.5 C4:.5 C4:.5 C4:.5 | C4:.5 D4:.5 E4:.5 F4:.5 G4:2 | G4 D4 D4 F4 | E4 D4 C4:2"),
            Song("s17", 100, "R:3 C4:.5 D4:.5 | E4 G4 G4:1.5 A4:.5 | G4 E4 C4:1.5 D4:.5 | E4 E4 D4 C4 | D4:3 C4:.5 D4:.5 | E4 G4 G4:1.5 A4:.5 | G4 E4 C4:1.5 D4:.5 | E4 E4 D4 D4 | C4:4"),
            Song("s18", 104, "G4 G4 G4 D4 | E4 E4 D4:2 | B4 B4 A4 A4 | G4:3 D4 | G4 G4 G4 D4 | E4 E4 D4:2 | B4 B4 A4 A4 | G4:4"),
            Song("s19", 104, "G4 G4 G4 A4 | G4 G4 D4:2 | E4 D4 E4 F#4 | G4:2 G4:2 | G4 G4 G4 A4 | G4 G4 D4:2 | E4 D4 E4 F#4 | G4:2 G4:2 | D5 C5 B4 A4 | B4 A4 G4:2 | E4 D4 E4 F#4 | G4:2 G4:2"),
            Song("s20", 108, "G4 G4 A4 B4 | G4 B4 A4 D4 | G4 G4 A4 B4 | G4:2 F#4:2 | G4 G4 A4 B4 | C5 B4 A4 G4 | F#4 D4 E4 F#4 | G4:2 G4:2"),
            Song("s21", 104, "F4 G4 A4 F4 | F4 G4 A4 F4 | A4 A#4 C5:2 | A4 A#4 C5:2 | C5:.5 D5:.5 C5:.5 A#4:.5 A4 F4 | C5:.5 D5:.5 C5:.5 A#4:.5 A4 F4 | F4 C4 F4:2 | F4 C4 F4:2"),
            Song("s22", 100, "F4:1.5 G4:.5 A4 A4 | G4:.5 F4:.5 G4:.5 A4:.5 F4 C4 | F4:1.5 G4:.5 A4 A4 | G4:.5 F4:.5 G4:.5 A4:.5 F4:2"),
            Song("s23", 112, "C4:2 D4:.5 F4:.5 E4:.5 D4:.5 | G4 G4 G4:.5 A4:.5 E4:.5 F4:.5 | D4 D4 D4:.5 F4:.5 E4:.5 D4:.5 | C4:.5 C5:.5 B4:.5 A4:.5 G4:.5 F4:.5 E4:.5 D4:.5 | C4:2 D4:.5 F4:.5 E4:.5 D4:.5 | G4 G4 G4:.5 A4:.5 E4:.5 F4:.5 | D4 D4 D4:.5 F4:.5 E4:.5 D4:.5 | C4:.5 G4:.5 D4:.5 E4:.5 C4:2"),
            Song("s24", 96, "R:2 C4:.5 C4:.5 | D4 C4 F4 | E4:2 C4:.5 C4:.5 | D4 C4 G4 | F4:2 C4:.5 C4:.5 | C5 A4 F4 | E4 D4 A#4:.5 A#4:.5 | A4 F4 G4 | F4:3", 3),
            Song("s25", 88, "R:2 D4 | G4:2 B4:.5 G4:.5 | B4:2 A4 | G4:2 E4 | D4:2 D4 | G4:2 B4:.5 G4:.5 | B4:2 A4 | D5:3 | R:2 B4 | D5:1.5 B4:.5 D5:.5 B4:.5 | G4:2 D4 | E4:1.5 G4:.5 G4:.5 E4:.5 | D4:2 D4 | G4:2 B4:.5 G4:.5 | B4:2 A4 | G4:3", 3),
            Song("s26", 108, "R:2 D4 | G4 G4:.5 A4:.5 G4:.5 F#4:.5 | E4 E4 E4 | A4 A4:.5 B4:.5 A4:.5 G4:.5 | F#4 D4 D4 | B4 B4:.5 C5:.5 B4:.5 A4:.5 | G4 E4 D4:.5 D4:.5 | E4 A4 F#4 | G4:3", 3),
            Song("s27", 84, "G4:1.5 A4:.5 G4 | E4:3 | G4:1.5 A4:.5 G4 | E4:3 | D5:2 D5 | B4:3 | C5:2 C5 | G4:3 | A4:2 A4 | C5:1.5 B4:.5 A4 | G4:1.5 A4:.5 G4 | E4:3 | A4:2 A4 | C5:1.5 B4:.5 A4 | G4:1.5 A4:.5 G4 | E4:3 | D5:2 D5 | F5:1.5 D5:.5 B4 | C5:3 | E5:3 | C5 G4 E4 | G4:1.5 F4:.5 D4 | C4:3", 3),
            Song("s28", 108, "E5 B4:.5 C5:.5 D5 C5:.5 B4:.5 | A4 A4:.5 C5:.5 E5 D5:.5 C5:.5 | B4:1.5 C5:.5 D5 E5 | C5 A4 A4:2 | E5 B4:.5 C5:.5 D5 C5:.5 B4:.5 | A4 A4:.5 C5:.5 E5 D5:.5 C5:.5 | B4:1.5 C5:.5 D5 E5 | C5 A4 A4:2"),
        };

        public static int IndexOf(string id) => Array.FindIndex(All, l => l.id == id);
        public static string Title(Lesson l) => I18n.T($"lessons.{l.id}.title");
        public static string Goal(Lesson l) => I18n.T($"lessons.{l.id}.goal");

        public static string[] Tips(Lesson l) =>
            I18n.Has($"lessons.{l.id}.tips") ? I18n.List($"lessons.{l.id}.tips") : I18n.List("lessons.songTips");

        /// <summary>Expand a song string into notes (rests are dropped).</summary>
        public static List<SongNote> ParseSong(Lesson lesson, out float totalBeats)
        {
            int lane = lesson.lane != null ? Notes.Parse(lesson.lane) : 0;
            var notes = new List<SongNote>();
            float beat = 0;
            foreach (var token in lesson.song.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (token == "|") continue;
                var parts = token.Split(':');
                float beats = parts.Length > 1 ? float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture) : 1f;
                if (parts[0] != "R") notes.Add(new SongNote { midi = parts[0] == "X" ? lane : Notes.Parse(parts[0]), beat = beat, beats = beats });
                beat += beats;
            }
            totalBeats = beat;
            return notes;
        }

        /// <summary>Keys to show: the lesson's own range, or a song's lowest to highest note.</summary>
        public static (int lo, int hi) Range(Lesson lesson)
        {
            if (lesson.range != null) return (Notes.Parse(lesson.range[0]), Notes.Parse(lesson.range[1]));
            int lo = int.MaxValue, hi = int.MinValue;
            foreach (var n in ParseSong(lesson, out _))
            {
                lo = Math.Min(lo, n.midi);
                hi = Math.Max(hi, n.midi);
            }
            // At least five white keys so the hand has room; end on white keys.
            hi = Math.Max(hi, lo + 7);
            while (Notes.IsBlack(lo)) lo--;
            while (Notes.IsBlack(hi)) hi++;
            return (lo, hi);
        }

        /// <summary>Build the prompt list for a "find" lesson.</summary>
        public static List<(int midi, Hint hint)> BuildPrompts(Lesson lesson)
        {
            var pool = lesson.pool.ConvertAll(Notes.Parse);
            var focus = (lesson.focus ?? lesson.pool).ConvertAll(Notes.Parse);
            var prompts = new List<(int, Hint)>();
            int last = -1;
            foreach (var (hint, count) in lesson.rounds)
            {
                for (int i = 0; i < count; i++)
                {
                    int midi;
                    if (hint == Hint.Key)
                    {
                        // Guided round: walk through the new notes in order.
                        midi = focus[i % focus.Length];
                    }
                    else
                    {
                        var from = lesson.focus != null && Random.value < 0.55f ? focus : pool;
                        do midi = from[Random.Range(0, from.Length)];
                        while (midi == last && pool.Length > 1);
                    }
                    last = midi;
                    prompts.Add((midi, hint));
                }
            }
            return prompts;
        }
    }

    static class ArrayExtensions
    {
        public static TOut[] ConvertAll<TIn, TOut>(this TIn[] array, Func<TIn, TOut> map) => Array.ConvertAll(array, x => map(x));
    }
}
