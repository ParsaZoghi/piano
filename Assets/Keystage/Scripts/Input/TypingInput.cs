using System;
using System.Collections.Generic;
using Keystage.Audio;
using Keystage.Core;
using UnityEngine.InputSystem;

namespace Keystage.Input
{
    /// <summary>
    /// The computer keyboard as a piano (port of input/typing.js). Home row =
    /// white keys, the row above = black keys. Keys are matched by physical
    /// position (Input System Key values), so any keyboard language works.
    /// </summary>
    public static class TypingInput
    {
        static readonly (Key key, string label, int semis)[] Layout =
        {
            (Key.A, "A", 0), (Key.W, "W", 1), (Key.S, "S", 2), (Key.E, "E", 3), (Key.D, "D", 4),
            (Key.F, "F", 5), (Key.T, "T", 6), (Key.G, "G", 7), (Key.Y, "Y", 8), (Key.H, "H", 9),
            (Key.U, "U", 10), (Key.J, "J", 11), (Key.K, "K", 12), (Key.O, "O", 13), (Key.L, "L", 14),
            (Key.P, "P", 15), (Key.Semicolon, ";", 16), (Key.Quote, "'", 17),
        };

        const int MinShift = -4; // A0 reachable
        const int MaxShift = 3; // C8 reachable

        static readonly Dictionary<Key, int> Down = new Dictionary<Key, int>();
        static bool canShift, canSustain, sustainDown;

        public static int Shift { get; private set; }
        public static int BaseOctave => 4 + Shift;
        public static event Action Changed;

        /// <summary>Set by the UI while a dialog is open so typing does not play notes.</summary>
        public static bool Blocked { get; set; }

        /// <summary>Free play allows octave shifting and the sustain pedal; lessons do not.</summary>
        public static void Configure(bool octaveShift = false, bool sustain = false)
        {
            canShift = octaveShift;
            canSustain = sustain;
            ReleaseAll();
            SetShift(0);
        }

        public static void SetShift(int value)
        {
            int next = Math.Max(MinShift, Math.Min(MaxShift, value));
            if (next != Shift)
            {
                ReleaseAll();
                Shift = next;
            }
            Changed?.Invoke();
        }

        /// <summary>midi to the character printed on the typing key that plays it.</summary>
        public static Dictionary<int, string> LabelMap()
        {
            var map = new Dictionary<int, string>();
            foreach (var (_, label, semis) in Layout)
            {
                int midi = 60 + Shift * 12 + semis;
                if (midi >= Notes.PianoMin && midi <= Notes.PianoMax) map[midi] = label;
            }
            return map;
        }

        public static void ReleaseAll()
        {
            Down.Clear();
            InputBus.ReleaseAll("typing");
            if (sustainDown) Synth.Instance?.SetSustain(false);
            sustainDown = false;
        }

        /// <summary>Poll the keyboard once per frame.</summary>
        public static void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            bool modifier = keyboard.ctrlKey.isPressed || keyboard.altKey.isPressed || keyboard.leftMetaKey.isPressed || keyboard.rightMetaKey.isPressed;

            foreach (var (key, _, semis) in Layout)
            {
                var control = keyboard[key];
                if (control.wasPressedThisFrame && !Blocked && !modifier && !Down.ContainsKey(key))
                {
                    int midi = 60 + Shift * 12 + semis;
                    Down[key] = midi;
                    InputBus.Press(midi, 0.8f, "typing");
                }
                else if (!control.isPressed && Down.TryGetValue(key, out int held))
                {
                    Down.Remove(key);
                    InputBus.Release(held, "typing");
                }
            }

            if (Blocked || modifier) return;
            if (canShift && keyboard.zKey.wasPressedThisFrame) SetShift(Shift - 1);
            if (canShift && keyboard.xKey.wasPressedThisFrame) SetShift(Shift + 1);
            if (canSustain)
            {
                bool space = keyboard.spaceKey.isPressed;
                if (space != sustainDown)
                {
                    sustainDown = space;
                    Synth.Instance?.SetSustain(space);
                }
            }
        }
    }
}
