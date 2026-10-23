using System;
using System.Collections.Generic;

namespace Keystage.Core
{
    /// <summary>
    /// Every input device (typing keys, pointer, MIDI, phone) funnels into this bus.
    /// A note counts as held while at least one source holds it (port of core/input.js).
    /// </summary>
    public static class InputBus
    {
        public static event Action<int, float, string> NoteOn; // midi, velocity, source
        public static event Action<int, string> NoteOff; // midi, source

        static readonly Dictionary<int, HashSet<string>> Held = new Dictionary<int, HashSet<string>>();

        public static void Press(int midi, float velocity = 0.8f, string source = "keys")
        {
            if (midi < Notes.PianoMin || midi > Notes.PianoMax) return;
            if (!Held.TryGetValue(midi, out var sources)) Held[midi] = sources = new HashSet<string>();
            if (!sources.Add(source)) return;
            if (sources.Count == 1) NoteOn?.Invoke(midi, velocity, source);
        }

        public static void Release(int midi, string source = "keys")
        {
            if (!Held.TryGetValue(midi, out var sources) || !sources.Remove(source)) return;
            if (sources.Count == 0)
            {
                Held.Remove(midi);
                NoteOff?.Invoke(midi, source);
            }
        }

        /// <summary>Release everything, or everything held by sources starting with prefix.</summary>
        public static void ReleaseAll(string prefix = "")
        {
            foreach (var entry in new List<KeyValuePair<int, HashSet<string>>>(Held))
                foreach (var source in new List<string>(entry.Value))
                    if (source.StartsWith(prefix, StringComparison.Ordinal)) Release(entry.Key, source);
        }

        /// <summary>Forget all listeners and held notes (domain reload safety in the editor).</summary>
        public static void Reset()
        {
            NoteOn = null;
            NoteOff = null;
            Held.Clear();
        }
    }
}
