using System;
using System.Text.RegularExpressions;

namespace Keystage.Core
{
    /// <summary>Note names, MIDI numbers and staff positions (port of core/notes.js).</summary>
    public static class Notes
    {
        public const int PianoMin = 21; // A0
        public const int PianoMax = 108; // C8

        static readonly string[] ParseNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        static readonly bool[] BlackKeys = { false, true, false, true, false, false, true, false, true, false, true, false };
        // Letter index (C=0 … B=6) each pitch class is written on when using sharps.
        static readonly int[] LetterIndex = { 0, 0, 1, 1, 2, 3, 3, 4, 4, 5, 5, 6 };
        static readonly string[] Letters = { "C", "D", "E", "F", "G", "A", "B" };
        static readonly string[] Solfege = { "Do", "Re", "Mi", "Fa", "Sol", "La", "Si" };
        static readonly Regex NotePattern = new Regex(@"^([A-Ga-g])([#♯]?)(-?\d)$");

        /// <summary>"letters" (C D E) or "solfege" (Do Re Mi).</summary>
        public static string Naming { get; private set; } = "letters";

        public static void SetNaming(string value) => Naming = value == "solfege" ? "solfege" : "letters";

        public static bool IsBlack(int midi) => BlackKeys[Mod(midi, 12)];
        public static int OctaveOf(int midi) => (int)Math.Floor(midi / 12.0) - 1;

        public static string PitchName(int midi)
        {
            var names = Naming == "solfege" ? Solfege : Letters;
            return names[LetterIndex[Mod(midi, 12)]] + (IsBlack(midi) ? "♯" : "");
        }

        public static string NoteName(int midi) => PitchName(midi) + OctaveOf(midi);
        public static float MidiToFreq(int midi) => 440f * (float)Math.Pow(2, (midi - 69) / 12.0);

        /// <summary>"C" or "F#" (no octave) to the name in the current naming, e.g. "Do", "Fa♯".</summary>
        public static string PitchClassName(string token) => PitchName(60 + Array.IndexOf(ParseNames, token.ToUpperInvariant()));

        /// <summary>"C4", "F#4" or "F♯4" to a MIDI number.</summary>
        public static int Parse(string text)
        {
            var m = NotePattern.Match(text.Trim());
            if (!m.Success) throw new ArgumentException("Bad note: " + text);
            int pc = Array.IndexOf(ParseNames, m.Groups[1].Value.ToUpperInvariant() + (m.Groups[2].Value.Length > 0 ? "#" : ""));
            return pc + (int.Parse(m.Groups[3].Value) + 1) * 12;
        }

        /// <summary>Diatonic steps above E4 (the bottom line of the treble staff).</summary>
        public static int StaffStep(int midi) => OctaveOf(midi) * 7 + LetterIndex[Mod(midi, 12)] - (4 * 7 + 2);

        static int Mod(int a, int n) => ((a % n) + n) % n;
    }
}
