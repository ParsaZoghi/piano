using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Keystage.Audio;
using Keystage.Core;
using UnityEngine;

namespace Keystage.Input
{
    /// <summary>
    /// MIDI keyboards through the Windows multimedia API (winmm.dll), so no
    /// plugin is needed (port of input/midi.js). Messages arrive on a driver
    /// thread and are handed to the main thread through a queue. Devices
    /// plugged in later are picked up by re-scanning every two seconds.
    /// </summary>
    public static class MidiInput
    {
        public enum State { Idle, Pending, Ready, Denied, Unsupported }

        public static State Status { get; private set; } = State.Idle;
        public static readonly List<string> Devices = new List<string>();
        public static event Action Changed;

        static readonly ConcurrentQueue<uint> Messages = new ConcurrentQueue<uint>();
        static readonly List<IntPtr> Handles = new List<IntPtr>();
        static float nextScan;
        static int lastCount = -1;

        public static bool Supported => Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        const int CallbackFunction = 0x00030000;
        const uint MimData = 0x3C3;

        delegate void MidiInProc(IntPtr handle, uint msg, IntPtr instance, IntPtr param1, IntPtr param2);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct MidiInCaps
        {
            public ushort wMid;
            public ushort wPid;
            public uint vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;
            public uint dwSupport;
        }

        [DllImport("winmm.dll")] static extern uint midiInGetNumDevs();
        [DllImport("winmm.dll", CharSet = CharSet.Unicode)] static extern uint midiInGetDevCapsW(UIntPtr deviceId, ref MidiInCaps caps, uint size);
        [DllImport("winmm.dll")] static extern uint midiInOpen(out IntPtr handle, uint deviceId, MidiInProc callback, IntPtr instance, int flags);
        [DllImport("winmm.dll")] static extern uint midiInStart(IntPtr handle);
        [DllImport("winmm.dll")] static extern uint midiInStop(IntPtr handle);
        [DllImport("winmm.dll")] static extern uint midiInReset(IntPtr handle);
        [DllImport("winmm.dll")] static extern uint midiInClose(IntPtr handle);

        // Kept in a static field so the garbage collector never frees it while Windows holds it.
        static readonly MidiInProc Callback = OnMessage;

        [AOT.MonoPInvokeCallback(typeof(MidiInProc))]
        static void OnMessage(IntPtr handle, uint msg, IntPtr instance, IntPtr param1, IntPtr param2)
        {
            if (msg == MimData) Messages.Enqueue((uint)param1.ToInt64());
        }
#endif

        /// <summary>Turn MIDI on: open every input device that is plugged in.</summary>
        public static void Enable()
        {
            if (!Supported)
            {
                Status = State.Unsupported;
                Changed?.Invoke();
                return;
            }
            if (Status == State.Ready) return;
            Status = State.Pending;
            Changed?.Invoke();
            Open();
        }

        public static void Disable()
        {
            CloseAll();
            Status = State.Idle;
            Devices.Clear();
            Changed?.Invoke();
        }

        static void Open()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            CloseAll();
            Devices.Clear();
            uint count;
            try
            {
                count = midiInGetNumDevs();
            }
            catch (Exception)
            {
                Status = State.Unsupported;
                Changed?.Invoke();
                return;
            }
            bool anyFailed = false;
            for (uint id = 0; id < count; id++)
            {
                var caps = new MidiInCaps();
                midiInGetDevCapsW((UIntPtr)id, ref caps, (uint)Marshal.SizeOf(typeof(MidiInCaps)));
                if (midiInOpen(out var handle, id, Callback, IntPtr.Zero, CallbackFunction) == 0)
                {
                    midiInStart(handle);
                    Handles.Add(handle);
                    Devices.Add(string.IsNullOrEmpty(caps.szPname) ? "MIDI device" : caps.szPname);
                }
                else anyFailed = true;
            }
            lastCount = (int)count;
            Status = anyFailed && Devices.Count == 0 && count > 0 ? State.Denied : State.Ready;
            InputBus.ReleaseAll("midi");
            Changed?.Invoke();
#endif
        }

        static void CloseAll()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            foreach (var handle in Handles)
            {
                midiInStop(handle);
                midiInReset(handle);
                midiInClose(handle);
            }
#endif
            Handles.Clear();
        }

        /// <summary>Drain queued messages on the main thread and watch for new devices.</summary>
        public static void Update()
        {
            while (Messages.TryDequeue(out uint message))
            {
                int status = (int)(message & 0xFF);
                int d1 = (int)((message >> 8) & 0x7F);
                int d2 = (int)((message >> 16) & 0x7F);
                int command = status & 0xF0;
                if (command == 0x90 && d2 > 0) InputBus.Press(d1, Mathf.Max(0.25f, d2 / 127f), "midi");
                else if (command == 0x80 || (command == 0x90 && d2 == 0)) InputBus.Release(d1, "midi");
                else if (command == 0xB0 && d1 == 64) Synth.Instance?.SetSustain(d2 >= 64); // sustain pedal
            }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (Status != State.Ready || Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 2f;
            if ((int)midiInGetNumDevs() != lastCount) Open();
#endif
        }

        public static void Shutdown()
        {
            CloseAll();
            Status = State.Idle;
            Devices.Clear();
            lastCount = -1;
            while (Messages.TryDequeue(out _)) { }
        }
    }
}
