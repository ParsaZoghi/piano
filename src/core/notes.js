export const PIANO_MIN = 21; // A0
export const PIANO_MAX = 108; // C8

const NAMES = ['C', 'C♯', 'D', 'D♯', 'E', 'F', 'F♯', 'G', 'G♯', 'A', 'A♯', 'B'];
const BLACK = new Set([1, 3, 6, 8, 10]);
// Letter index (C=0 … B=6) each pitch class is written on when using sharps.
const LETTER_INDEX = [0, 0, 1, 1, 2, 3, 3, 4, 4, 5, 5, 6];

export const isBlack = (midi) => BLACK.has(midi % 12);
export const octaveOf = (midi) => Math.floor(midi / 12) - 1;
export const pitchName = (midi) => NAMES[midi % 12];
export const noteName = (midi) => pitchName(midi) + octaveOf(midi);
export const midiToFreq = (midi) => 440 * 2 ** ((midi - 69) / 12);

/** "C4", "F#4" or "F♯4" -> MIDI number. */
export function parseNote(text) {
    const m = /^([A-Ga-g])([#♯]?)(-?\d)$/.exec(text.trim());
    if (!m) throw new Error(`Bad note: ${text}`);
    const pc = NAMES.indexOf(m[1].toUpperCase() + (m[2] ? '♯' : ''));
    return pc + (Number(m[3]) + 1) * 12;
}

/** Diatonic steps above E4 (the bottom line of the treble staff). */
export function staffStep(midi) {
    return octaveOf(midi) * 7 + LETTER_INDEX[midi % 12] - (4 * 7 + 2);
}
