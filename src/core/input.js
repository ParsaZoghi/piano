import { PIANO_MAX, PIANO_MIN } from './notes.js';

// Every input device (typing keys, pointer, MIDI, phone) funnels into this bus.
// A note counts as held while at least one source holds it.
const listeners = { noteon: new Set(), noteoff: new Set() };
const held = new Map(); // midi -> Set<source>

function emit(type, event) {
    for (const fn of [...listeners[type]]) fn(event);
}

export const input = {
    on(type, fn) {
        listeners[type].add(fn);
        return () => listeners[type].delete(fn);
    },

    noteOn(midi, velocity = 0.8, source = 'keys') {
        if (midi < PIANO_MIN || midi > PIANO_MAX) return;
        let sources = held.get(midi);
        if (!sources) held.set(midi, (sources = new Set()));
        if (sources.has(source)) return;
        sources.add(source);
        if (sources.size === 1) emit('noteon', { midi, velocity, source });
    },

    noteOff(midi, source = 'keys') {
        const sources = held.get(midi);
        if (!sources || !sources.delete(source)) return;
        if (sources.size === 0) {
            held.delete(midi);
            emit('noteoff', { midi, source });
        }
    },

    /** Release everything, or everything held by sources matching `prefix`. */
    releaseAll(prefix = '') {
        for (const [midi, sources] of [...held]) {
            for (const source of [...sources]) {
                if (source.startsWith(prefix)) this.noteOff(midi, source);
            }
        }
    },
};
