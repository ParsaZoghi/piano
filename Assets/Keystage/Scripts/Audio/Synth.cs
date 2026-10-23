using System.Collections;
using System.Collections.Generic;
using Keystage.Core;
using UnityEngine;

namespace Keystage.Audio
{
    /// <summary>
    /// A small piano-like synth (port of audio/synth.js). Each note is a
    /// harmonic-rich tone with a low-pass that closes as it decays and a
    /// pitch-dependent decay time. Tones are rendered once into AudioClips and
    /// played through a pool of AudioSources.
    /// </summary>
    public class Synth : MonoBehaviour
    {
        const int Voices = 24;
        const float ReleaseTime = 0.09f; // time constant of the key-up fade
        static readonly float[] Partials = { 1f, 0.46f, 0.24f, 0.15f, 0.08f, 0.05f, 0.03f, 0.015f };

        class Voice
        {
            public AudioSource source;
            public int midi = -1;
            public float gain;
            public bool releasing;
            public float releaseTime;
        }

        public static Synth Instance { get; private set; }

        readonly List<Voice> voices = new List<Voice>();
        readonly Dictionary<int, Voice> active = new Dictionary<int, Voice>();
        readonly HashSet<int> sustained = new HashSet<int>();
        readonly Dictionary<int, AudioClip> clips = new Dictionary<int, AudioClip>();
        AudioClip clickHigh, clickLow;
        AudioSource clickSource;
        bool sustain;
        float volume = 0.8f;
        int sampleRate;

        void Awake()
        {
            Instance = this;
            sampleRate = AudioSettings.outputSampleRate > 0 ? AudioSettings.outputSampleRate : 48000;
            for (int i = 0; i < Voices; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                voices.Add(new Voice { source = source });
            }
            clickSource = gameObject.AddComponent<AudioSource>();
            clickSource.playOnAwake = false;
            clickHigh = Click(1568f);
            clickLow = Click(1046f);
            StartCoroutine(Prewarm());
        }

        /// <summary>Render the middle of the keyboard ahead of time so the first press is instant.</summary>
        IEnumerator Prewarm()
        {
            for (int midi = 48; midi <= 84; midi++)
            {
                GetClip(midi);
                yield return null;
            }
        }

        public void SetVolume(float value)
        {
            volume = value;
            foreach (var v in voices)
                if (v.midi >= 0 && !v.releasing) v.source.volume = v.gain * volume;
        }

        public void NoteOn(int midi, float velocity = 0.8f)
        {
            if (active.TryGetValue(midi, out var existing)) Stop(existing);
            sustained.Remove(midi);
            var voice = FreeVoice();
            voice.midi = midi;
            voice.releasing = false;
            voice.gain = 0.35f + 0.65f * Mathf.Clamp01(velocity);
            voice.source.clip = GetClip(midi);
            voice.source.volume = voice.gain * volume;
            voice.source.Play();
            active[midi] = voice;
        }

        public void NoteOff(int midi)
        {
            if (sustain)
            {
                if (active.ContainsKey(midi)) sustained.Add(midi);
                return;
            }
            Release(midi);
        }

        public void SetSustain(bool on)
        {
            sustain = on;
            if (on) return;
            foreach (var midi in sustained) Release(midi);
            sustained.Clear();
        }

        public void AllOff()
        {
            sustained.Clear();
            foreach (var midi in new List<int>(active.Keys)) Release(midi);
        }

        /// <summary>Metronome click; the accented one marks the first beat of a bar.</summary>
        public void Click(bool accent) => clickSource.PlayOneShot(accent ? clickHigh : clickLow, (accent ? 0.9f : 0.6f) * volume);

        /// <summary>Short rising arpeggio for a finished lesson.</summary>
        public void Fanfare(int stars)
        {
            StartCoroutine(PlayFanfare(stars));
        }

        IEnumerator PlayFanfare(int stars)
        {
            int[] notes = { 72, 76, 79, 84 };
            for (int i = 0; i <= Mathf.Min(stars, 3); i++)
            {
                NoteOn(notes[i], 0.6f);
                StartCoroutine(ReleaseLater(notes[i], 0.26f));
                yield return new WaitForSeconds(0.12f);
            }
        }

        IEnumerator ReleaseLater(int midi, float delay)
        {
            yield return new WaitForSeconds(delay);
            NoteOff(midi);
        }

        void Release(int midi)
        {
            if (!active.TryGetValue(midi, out var voice)) return;
            active.Remove(midi);
            voice.releasing = true;
            voice.releaseTime = 0f;
        }

        void Stop(Voice voice)
        {
            voice.source.Stop();
            if (voice.midi >= 0 && active.TryGetValue(voice.midi, out var v) && v == voice) active.Remove(voice.midi);
            voice.midi = -1;
            voice.releasing = false;
        }

        Voice FreeVoice()
        {
            Voice best = null;
            foreach (var v in voices)
            {
                if (!v.source.isPlaying)
                {
                    Stop(v);
                    return v;
                }
                // Otherwise steal the quietest releasing voice, else the oldest.
                if (best == null || (v.releasing && !best.releasing) || v.source.time > best.source.time) best = v;
            }
            Stop(best);
            return best;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            foreach (var v in voices)
            {
                if (!v.releasing) continue;
                v.releaseTime += dt;
                float level = v.gain * volume * Mathf.Exp(-v.releaseTime / ReleaseTime);
                v.source.volume = level;
                if (level < 0.0005f) Stop(v);
            }
        }

        AudioClip GetClip(int midi)
        {
            if (clips.TryGetValue(midi, out var clip)) return clip;
            float freq = Notes.MidiToFreq(midi);
            float decay = Mathf.Clamp(1.5f * Mathf.Pow(2f, (60 - midi) / 24f), 0.35f, 3.5f);
            int length = Mathf.CeilToInt(Mathf.Min(8f, decay * 6f) * sampleRate);
            var data = new float[length];
            float startCutoff = Mathf.Min(14000f, freq * 7f + 1200f);
            float endCutoff = freq * 2f + 300f;
            float nyquist = sampleRate * 0.45f;
            var gains = new float[Partials.Length];
            const int block = 256;

            for (int start = 0; start < length; start += block)
            {
                float t0 = start / (float)sampleRate;
                float cutoff = endCutoff + (startCutoff - endCutoff) * Mathf.Exp(-t0 / 0.5f);
                for (int k = 0; k < Partials.Length; k++)
                {
                    float hf = freq * (k + 1);
                    float ratio = hf / cutoff;
                    gains[k] = hf >= nyquist ? 0f : Partials[k] / Mathf.Sqrt(1f + ratio * ratio * ratio * ratio);
                }
                int end = Mathf.Min(length, start + block);
                for (int i = start; i < end; i++)
                {
                    float t = i / (float)sampleRate;
                    float phase = 2f * Mathf.PI * freq * t;
                    float s = 0f;
                    for (int k = 0; k < gains.Length; k++)
                        if (gains[k] > 0f) s += gains[k] * Mathf.Cos(phase * (k + 1));
                    float envelope = (t < 0.005f ? t / 0.005f : 1f) * Mathf.Exp(-t / decay);
                    data[i] = s * envelope * 0.16f;
                }
            }
            // Fade the very end so the clip never ends with a click.
            int tail = Mathf.Min(length, sampleRate / 50);
            for (int i = 0; i < tail; i++) data[length - 1 - i] *= i / (float)tail;

            clip = AudioClip.Create($"note{midi}", length, 1, sampleRate, false);
            clip.SetData(data, 0);
            clips[midi] = clip;
            return clip;
        }

        AudioClip Click(float freq)
        {
            int length = (int)(0.08f * sampleRate);
            var data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)sampleRate;
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.3f * Mathf.Exp(-t / 0.012f);
            }
            var clip = AudioClip.Create("click" + freq, length, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
