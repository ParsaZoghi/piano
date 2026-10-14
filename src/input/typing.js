import { input } from '../core/input.js';
import { PIANO_MAX, PIANO_MIN } from '../core/notes.js';
import { synth } from '../audio/synth.js';

// Home row = white keys, the row above = black keys (the usual "piano" layout).
// Mapped by physical key (event.code) so it works on any keyboard language.
const LAYOUT = [
    ['KeyA', 'A', 0],
    ['KeyW', 'W', 1],
    ['KeyS', 'S', 2],
    ['KeyE', 'E', 3],
    ['KeyD', 'D', 4],
    ['KeyF', 'F', 5],
    ['KeyT', 'T', 6],
    ['KeyG', 'G', 7],
    ['KeyY', 'Y', 8],
    ['KeyH', 'H', 9],
    ['KeyU', 'U', 10],
    ['KeyJ', 'J', 11],
    ['KeyK', 'K', 12],
    ['KeyO', 'O', 13],
    ['KeyL', 'L', 14],
    ['KeyP', 'P', 15],
    ['Semicolon', ';', 16],
    ['Quote', "'", 17],
];
const OFFSET = new Map(LAYOUT.map(([code, , semis]) => [code, semis]));

const MIN_SHIFT = -4; // A0 reachable
const MAX_SHIFT = 3; // C8 reachable

let shift = 0;
let canShift = false;
let canSustain = false;
const down = new Map(); // code -> midi that was started
const changeListeners = new Set();

const base = () => 60 + shift * 12;
const notify = () => changeListeners.forEach((fn) => fn());

function isTypingTarget(target) {
    return target instanceof HTMLElement && (target.isContentEditable || /^(INPUT|TEXTAREA|SELECT)$/.test(target.tagName));
}

function onKeyDown(e) {
    if (e.ctrlKey || e.metaKey || e.altKey || isTypingTarget(e.target)) return;
    if (document.querySelector('dialog[open]')) return;

    if (OFFSET.has(e.code)) {
        e.preventDefault();
        if (e.repeat || down.has(e.code)) return;
        const midi = base() + OFFSET.get(e.code);
        down.set(e.code, midi);
        input.noteOn(midi, 0.8, 'typing');
    } else if (canShift && (e.code === 'KeyZ' || e.code === 'KeyX') && !e.repeat) {
        typing.setShift(shift + (e.code === 'KeyX' ? 1 : -1));
    } else if (canSustain && e.code === 'Space') {
        e.preventDefault();
        if (!e.repeat) synth.setSustain(true);
    }
}

function onKeyUp(e) {
    if (down.has(e.code)) {
        input.noteOff(down.get(e.code), 'typing');
        down.delete(e.code);
    } else if (canSustain && e.code === 'Space') {
        synth.setSustain(false);
    }
}

function releaseAll() {
    down.clear();
    input.releaseAll('typing');
    synth.setSustain(false);
}

export const typing = {
    init() {
        window.addEventListener('keydown', onKeyDown);
        window.addEventListener('keyup', onKeyUp);
        window.addEventListener('blur', releaseAll);
    },

    /** Free play allows octave shifting and the sustain pedal; lessons do not. */
    configure({ octaveShift = false, sustain = false } = {}) {
        canShift = octaveShift;
        canSustain = sustain;
        releaseAll();
        this.setShift(0);
    },

    get shift() {
        return shift;
    },

    get baseOctave() {
        return 4 + shift;
    },

    setShift(value) {
        const next = Math.max(MIN_SHIFT, Math.min(MAX_SHIFT, value));
        if (next === shift) return notify();
        releaseAll();
        shift = next;
        notify();
    },

    onChange(fn) {
        changeListeners.add(fn);
        return () => changeListeners.delete(fn);
    },

    /** midi -> the character printed on the typing key that plays it. */
    labelMap() {
        const map = new Map();
        for (const [, char, semis] of LAYOUT) {
            const midi = base() + semis;
            if (midi >= PIANO_MIN && midi <= PIANO_MAX) map.set(midi, char);
        }
        return map;
    },
};
