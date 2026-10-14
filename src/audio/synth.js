import { midiToFreq } from '../core/notes.js';

// A small Web Audio piano-like synth: one harmonic-rich oscillator per note,
// a low-pass that closes as the note decays, and a pitch-dependent decay time.
let ctx = null;
let master = null;
let wave = null;
let volume = 0.8;
let sustain = false;
const voices = new Map(); // midi -> voice
const sustained = new Set(); // notes released while the pedal is down

function ensure() {
    if (!ctx) {
        ctx = new AudioContext({ latencyHint: 'interactive' });
        master = ctx.createGain();
        master.gain.value = volume;
        const comp = ctx.createDynamicsCompressor();
        comp.threshold.value = -16;
        comp.ratio.value = 4;
        master.connect(comp).connect(ctx.destination);
        const real = new Float32Array([0, 1, 0.46, 0.24, 0.15, 0.08, 0.05, 0.03, 0.015]);
        wave = ctx.createPeriodicWave(real, new Float32Array(real.length));
    }
    if (ctx.state === 'suspended') ctx.resume();
    return ctx;
}

function hold(param, t) {
    if (param.cancelAndHoldAtTime) param.cancelAndHoldAtTime(t);
    else {
        const v = param.value;
        param.cancelScheduledValues(t);
        param.setValueAtTime(v, t);
    }
}

function release(midi, time = 0.09) {
    const voice = voices.get(midi);
    if (!voice) return;
    voices.delete(midi);
    const t = ctx.currentTime;
    hold(voice.gain.gain, t);
    voice.gain.gain.setTargetAtTime(0, t, time);
    voice.osc.stop(t + time * 8);
}

export const synth = {
    /** Call from a user gesture so the browser allows audio. */
    unlock: ensure,

    noteOn(midi, velocity = 0.8) {
        ensure();
        release(midi, 0.02);
        sustained.delete(midi);
        const t = ctx.currentTime;
        const freq = midiToFreq(midi);

        const osc = ctx.createOscillator();
        osc.setPeriodicWave(wave);
        osc.frequency.value = freq;

        const filter = ctx.createBiquadFilter();
        filter.type = 'lowpass';
        filter.frequency.setValueAtTime(Math.min(14000, freq * 7 + 1200), t);
        filter.frequency.setTargetAtTime(freq * 2 + 300, t, 0.5);

        const gain = ctx.createGain();
        const peak = 0.12 + 0.34 * velocity;
        const decay = Math.min(3.5, Math.max(0.35, 1.5 * 2 ** ((60 - midi) / 24)));
        gain.gain.setValueAtTime(0, t);
        gain.gain.linearRampToValueAtTime(peak, t + 0.005);
        gain.gain.setTargetAtTime(0, t + 0.005, decay);

        osc.connect(filter).connect(gain).connect(master);
        osc.start(t);
        osc.onended = () => gain.disconnect();
        voices.set(midi, { osc, gain });
    },

    noteOff(midi) {
        if (!ctx) return;
        if (sustain) sustained.add(midi);
        else release(midi);
    },

    setSustain(on) {
        sustain = on;
        if (!on && ctx) {
            for (const midi of sustained) release(midi);
            sustained.clear();
        }
    },

    allOff() {
        if (!ctx) return;
        sustained.clear();
        for (const midi of [...voices.keys()]) release(midi, 0.03);
    },

    setVolume(value) {
        volume = value;
        if (master) master.gain.setTargetAtTime(value, ctx.currentTime, 0.02);
    },

    /** Metronome click; the accented one marks the first beat of a bar. */
    click(accent = false) {
        ensure();
        const t = ctx.currentTime;
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.frequency.value = accent ? 1568 : 1046;
        gain.gain.setValueAtTime(accent ? 0.28 : 0.18, t);
        gain.gain.exponentialRampToValueAtTime(0.0001, t + 0.06);
        osc.connect(gain).connect(master);
        osc.start(t);
        osc.stop(t + 0.08);
    },

    /** Short rising arpeggio for a finished lesson. */
    fanfare(stars = 3) {
        ensure();
        const notes = [72, 76, 79, 84].slice(0, stars + 1);
        notes.forEach((midi, i) => {
            setTimeout(() => {
                this.noteOn(midi, 0.6);
                setTimeout(() => this.noteOff(midi), 260);
            }, i * 120);
        });
    },
};
