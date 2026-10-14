import { has, t, tList } from '../core/i18n.js';
import { isBlack, parseNote } from '../core/notes.js';

// The curriculum: notes first, rhythm second, then both together in songs.
// Titles, goals and tips live in src/i18n (key: lessons.<id>).
// Three lesson modes:
//   find   - untimed: press the key for the note shown (hints fade per round)
//   rhythm - timed: tap any key in time; only the rhythm is scored
//   play   - timed: right key at the right time
//
// Songs are written as "NOTE:beats" tokens ("R" is a rest, "X" a rhythm tap,
// "|" is ignored and only marks bars for readability). `meter` is beats per
// bar (4 unless given). Songs are public-domain melodies.

export const UNITS = ['notes', 'rhythm', 'songs'];

const WHITE_5 = ['C4', 'D4', 'E4', 'F4', 'G4'];
const WHITE_8 = [...WHITE_5, 'A4', 'B4', 'C5'];

const song = (id, bpm, notes, extra = {}) => ({ id, unit: 'songs', mode: 'play', bpm, song: notes, ...extra });

export const LESSONS = [
    { id: 'n1', unit: 'notes', mode: 'find', range: ['C4', 'E4'], pool: ['C4', 'D4', 'E4'], rounds: [['key', 6], ['name', 6], ['staff', 6]] },
    { id: 'n2', unit: 'notes', mode: 'find', range: ['C4', 'G4'], pool: WHITE_5, focus: ['F4', 'G4'], rounds: [['key', 6], ['name', 8], ['staff', 8]] },
    { id: 'n3', unit: 'notes', mode: 'find', range: ['C4', 'G4'], pool: WHITE_5, rounds: [['name', 8], ['staff', 10]] },
    { id: 'n4', unit: 'notes', mode: 'find', range: ['C4', 'C5'], pool: WHITE_8, focus: ['A4', 'B4', 'C5'], rounds: [['key', 6], ['name', 8], ['staff', 10]] },
    { id: 'n5', unit: 'notes', mode: 'find', range: ['C4', 'C5'], pool: ['C#4', 'D#4', 'F#4', 'G#4', 'A#4'], rounds: [['key', 5], ['name', 10], ['staff', 6]] },
    { id: 'n6', unit: 'notes', mode: 'find', range: ['C4', 'E5'], pool: [...WHITE_8, 'D5', 'E5'], rounds: [['name', 8], ['staff', 14]] },

    { id: 'r1', unit: 'rhythm', mode: 'rhythm', range: ['C4', 'E4'], lane: 'D4', bpm: 76, song: 'X X X X | X X X X | X X X X | X X X X' },
    {
        id: 'r2',
        unit: 'rhythm',
        mode: 'rhythm',
        range: ['C4', 'E4'],
        lane: 'D4',
        bpm: 80,
        song: 'X:2 X:2 | X X X:2 | X:2 X X | X:4 | X:2 X:2 | X X X X | X:2 X:2 | X:4',
    },
    {
        id: 'r3',
        unit: 'rhythm',
        mode: 'rhythm',
        range: ['C4', 'E4'],
        lane: 'D4',
        bpm: 72,
        song: 'X X X:.5 X:.5 X | X:.5 X:.5 X:.5 X:.5 X X | X X:.5 X:.5 X X:.5 X:.5 | X:.5 X:.5 X X:2 | X:.5 X:.5 X:.5 X:.5 X:.5 X:.5 X | X:2 X:2',
    },
    {
        id: 'r4',
        unit: 'rhythm',
        mode: 'rhythm',
        range: ['C4', 'E4'],
        lane: 'D4',
        bpm: 80,
        song: 'X R X R | X X R X | R X R X | X:2 R:2 | X R X X | R X X R | X X R X | X:2 R:2',
    },

    song('s1', 84, 'E4 D4 C4:2 | E4 D4 C4:2 | C4:.5 C4:.5 C4:.5 C4:.5 D4:.5 D4:.5 D4:.5 D4:.5 | E4 D4 C4:2'),
    song('s7', 88, 'G4 E4 G4:.5 G4:.5 E4 | G4:.5 G4:.5 E4:.5 A4:.5 G4:.5 G4:.5 E4 | G4 E4 G4:.5 G4:.5 E4 | G4:.5 G4:.5 E4:.5 A4:.5 G4:.5 G4:.5 E4'),
    song('s8', 96, 'C4 C4 C4 D4 | E4:2 D4:2 | C4 E4 D4 D4 | C4:4 | C4 C4 C4 D4 | E4:2 D4:2 | C4 E4 D4 D4 | C4:4'),
    song('s2', 92, 'E4 D4 C4 D4 | E4 E4 E4:2 | D4 D4 D4:2 | E4 G4 G4:2 | E4 D4 C4 D4 | E4 E4 E4 E4 | D4 D4 E4 D4 | C4:4'),
    song('s9', 96, 'E4:2 E4 D4 | C4:2 C4:2 | D4:2 D4 F4 | E4 D4 C4:2 | G4:2 G4 F4 | E4:2 E4:2 | C4 D4 E4 D4 | C4:4'),
    song(
        's10',
        100,
        'G4 E4 E4:2 | F4 D4 D4:2 | C4 D4 E4 F4 | G4 G4 G4:2 | G4 E4 E4:2 | F4 D4 D4:2 | C4 E4 G4 G4 | E4:4 | D4 D4 D4 D4 | D4 E4 F4:2 | E4 E4 E4 E4 | E4 F4 G4:2 | G4 E4 E4:2 | F4 D4 D4:2 | C4 E4 G4 G4 | C4:4',
    ),
    song('s3', 96, 'E4 E4 F4 G4 | G4 F4 E4 D4 | C4 C4 D4 E4 | E4:1.5 D4:.5 D4:2 | E4 E4 F4 G4 | G4 F4 E4 D4 | C4 C4 D4 E4 | D4:1.5 C4:.5 C4:2'),
    song('s11', 100, 'C4:2 C4 D4 | E4:2 E4 F4 | G4:2 A4 G4 | E4:4 | G4:2 F4 E4 | D4:4 | F4:2 E4 D4 | C4:4'),
    song('s12', 92, 'C4 F4 E4 F4 | G4 D4 G4:2 | F4 E4 D4 E4 | F4:4 | C4 F4 E4 F4 | G4 D4 G4:2 | F4 E4 D4 E4 | F4:4'),
    song(
        's4',
        96,
        'C4 C4 G4 G4 | A4 A4 G4:2 | F4 F4 E4 E4 | D4 D4 C4:2 | G4 G4 F4 F4 | E4 E4 D4:2 | G4 G4 F4 F4 | E4 E4 D4:2 | C4 C4 G4 G4 | A4 A4 G4:2 | F4 F4 E4 E4 | D4 D4 C4:2',
    ),
    song('s13', 104, 'G4:1.5 A4:.5 G4 F4 | E4 F4 G4:2 | D4 E4 F4:2 | E4 F4 G4:2 | G4:1.5 A4:.5 G4 F4 | E4 F4 G4:2 | D4:2 G4:2 | E4 C4:3'),
    song('s14', 76, 'E4:1.5 G4:.5 G4:2 | E4:1.5 D4:.5 C4:2 | D4:1.5 E4:.5 G4:1.5 E4:.5 | D4:4 | E4:1.5 G4:.5 G4:2 | E4:1.5 D4:.5 C4:2 | D4 E4 D4:1.5 C4:.5 | C4:4'),
    song('s5', 100, 'E4 E4 E4:2 | E4 E4 E4:2 | E4 G4 C4:1.5 D4:.5 | E4:4 | F4 F4 F4:1.5 F4:.5 | F4 E4 E4 E4:.5 E4:.5 | E4 D4 D4 E4 | D4:2 G4:2'),
    song(
        's15',
        108,
        'R C4 E4 F4 | G4:4 | R C4 E4 F4 | G4:4 | R C4 E4 F4 | G4:2 E4:2 | C4:2 E4:2 | D4:4 | R E4 E4 D4 | C4:3 C4 | E4:2 G4:2 | G4 G4 F4:2 | R:2 E4 F4 | G4:2 E4:2 | C4:2 D4:2 | C4:4',
    ),
    song(
        's6',
        88,
        'C4 D4 E4 F4 | G4 A4 B4 C5 | C5 B4 A4 G4 | F4 E4 D4 C4 | C4:.5 D4:.5 E4:.5 F4:.5 G4:.5 A4:.5 B4:.5 C5:.5 | C5:.5 B4:.5 A4:.5 G4:.5 F4:.5 E4:.5 D4:.5 C4:.5 | C4:4',
    ),
    song('s16', 96, 'G4 E4 G4:2 | G4 E4 G4:2 | A4 G4 F4 E4 | D4 E4 F4:2 | E4:.5 F4:.5 G4 C4:.5 C4:.5 C4:.5 C4:.5 | C4:.5 D4:.5 E4:.5 F4:.5 G4:2 | G4 D4 D4 F4 | E4 D4 C4:2'),
    song(
        's17',
        100,
        'R:3 C4:.5 D4:.5 | E4 G4 G4:1.5 A4:.5 | G4 E4 C4:1.5 D4:.5 | E4 E4 D4 C4 | D4:3 C4:.5 D4:.5 | E4 G4 G4:1.5 A4:.5 | G4 E4 C4:1.5 D4:.5 | E4 E4 D4 D4 | C4:4',
    ),
    song('s18', 104, 'G4 G4 G4 D4 | E4 E4 D4:2 | B4 B4 A4 A4 | G4:3 D4 | G4 G4 G4 D4 | E4 E4 D4:2 | B4 B4 A4 A4 | G4:4'),
    song(
        's19',
        104,
        'G4 G4 G4 A4 | G4 G4 D4:2 | E4 D4 E4 F#4 | G4:2 G4:2 | G4 G4 G4 A4 | G4 G4 D4:2 | E4 D4 E4 F#4 | G4:2 G4:2 | D5 C5 B4 A4 | B4 A4 G4:2 | E4 D4 E4 F#4 | G4:2 G4:2',
    ),
    song('s20', 108, 'G4 G4 A4 B4 | G4 B4 A4 D4 | G4 G4 A4 B4 | G4:2 F#4:2 | G4 G4 A4 B4 | C5 B4 A4 G4 | F#4 D4 E4 F#4 | G4:2 G4:2'),
    song(
        's21',
        104,
        'F4 G4 A4 F4 | F4 G4 A4 F4 | A4 A#4 C5:2 | A4 A#4 C5:2 | C5:.5 D5:.5 C5:.5 A#4:.5 A4 F4 | C5:.5 D5:.5 C5:.5 A#4:.5 A4 F4 | F4 C4 F4:2 | F4 C4 F4:2',
    ),
    song('s22', 100, 'F4:1.5 G4:.5 A4 A4 | G4:.5 F4:.5 G4:.5 A4:.5 F4 C4 | F4:1.5 G4:.5 A4 A4 | G4:.5 F4:.5 G4:.5 A4:.5 F4:2'),
    song(
        's23',
        112,
        'C4:2 D4:.5 F4:.5 E4:.5 D4:.5 | G4 G4 G4:.5 A4:.5 E4:.5 F4:.5 | D4 D4 D4:.5 F4:.5 E4:.5 D4:.5 | C4:.5 C5:.5 B4:.5 A4:.5 G4:.5 F4:.5 E4:.5 D4:.5 | C4:2 D4:.5 F4:.5 E4:.5 D4:.5 | G4 G4 G4:.5 A4:.5 E4:.5 F4:.5 | D4 D4 D4:.5 F4:.5 E4:.5 D4:.5 | C4:.5 G4:.5 D4:.5 E4:.5 C4:2',
    ),
    song('s24', 96, 'R:2 C4:.5 C4:.5 | D4 C4 F4 | E4:2 C4:.5 C4:.5 | D4 C4 G4 | F4:2 C4:.5 C4:.5 | C5 A4 F4 | E4 D4 A#4:.5 A#4:.5 | A4 F4 G4 | F4:3', { meter: 3 }),
    song(
        's25',
        88,
        'R:2 D4 | G4:2 B4:.5 G4:.5 | B4:2 A4 | G4:2 E4 | D4:2 D4 | G4:2 B4:.5 G4:.5 | B4:2 A4 | D5:3 | R:2 B4 | D5:1.5 B4:.5 D5:.5 B4:.5 | G4:2 D4 | E4:1.5 G4:.5 G4:.5 E4:.5 | D4:2 D4 | G4:2 B4:.5 G4:.5 | B4:2 A4 | G4:3',
        { meter: 3 },
    ),
    song(
        's26',
        108,
        'R:2 D4 | G4 G4:.5 A4:.5 G4:.5 F#4:.5 | E4 E4 E4 | A4 A4:.5 B4:.5 A4:.5 G4:.5 | F#4 D4 D4 | B4 B4:.5 C5:.5 B4:.5 A4:.5 | G4 E4 D4:.5 D4:.5 | E4 A4 F#4 | G4:3',
        { meter: 3 },
    ),
    song(
        's27',
        84,
        'G4:1.5 A4:.5 G4 | E4:3 | G4:1.5 A4:.5 G4 | E4:3 | D5:2 D5 | B4:3 | C5:2 C5 | G4:3 | A4:2 A4 | C5:1.5 B4:.5 A4 | G4:1.5 A4:.5 G4 | E4:3 | A4:2 A4 | C5:1.5 B4:.5 A4 | G4:1.5 A4:.5 G4 | E4:3 | D5:2 D5 | F5:1.5 D5:.5 B4 | C5:3 | E5:3 | C5 G4 E4 | G4:1.5 F4:.5 D4 | C4:3',
        { meter: 3 },
    ),
    song(
        's28',
        108,
        'E5 B4:.5 C5:.5 D5 C5:.5 B4:.5 | A4 A4:.5 C5:.5 E5 D5:.5 C5:.5 | B4:1.5 C5:.5 D5 E5 | C5 A4 A4:2 | E5 B4:.5 C5:.5 D5 C5:.5 B4:.5 | A4 A4:.5 C5:.5 E5 D5:.5 C5:.5 | B4:1.5 C5:.5 D5 E5 | C5 A4 A4:2',
    ),
];

export const lessonIndex = (id) => LESSONS.findIndex((lesson) => lesson.id === id);
export const lessonTitle = (lesson) => t(`lessons.${lesson.id}.title`);
export const lessonGoal = (lesson) => t(`lessons.${lesson.id}.goal`);
export const lessonTips = (lesson) => (has(`lessons.${lesson.id}.tips`) ? tList(`lessons.${lesson.id}.tips`) : tList('lessons.songTips'));

/** Expand a song string into [{ midi, beat, beats }] (rests are dropped). */
export function parseSong(lesson) {
    const lane = lesson.lane ? parseNote(lesson.lane) : null;
    const notes = [];
    let beat = 0;
    for (const token of lesson.song.split(/\s+/)) {
        if (!token || token === '|') continue;
        const [name, length] = token.split(':');
        const beats = length ? Number(length) : 1;
        if (name !== 'R') notes.push({ midi: name === 'X' ? lane : parseNote(name), beat, beats });
        beat += beats;
    }
    return { notes, totalBeats: beat };
}

/** Keys to show: the lesson's own range, or a song's lowest to highest note. */
export function lessonRange(lesson) {
    if (lesson.range) return lesson.range.map(parseNote);
    const midis = parseSong(lesson).notes.map((n) => n.midi);
    let lo = Math.min(...midis);
    let hi = Math.max(...midis);
    // At least five white keys so the hand has room; end on white keys.
    hi = Math.max(hi, lo + 7);
    while (isBlack(lo)) lo--;
    while (isBlack(hi)) hi++;
    return [lo, hi];
}

/** Build the prompt list for a "find" lesson: [{ midi, hint }]. */
export function buildPrompts(lesson) {
    const pool = lesson.pool.map(parseNote);
    const focus = (lesson.focus || lesson.pool).map(parseNote);
    const prompts = [];
    let last = null;
    for (const [hint, count] of lesson.rounds) {
        for (let i = 0; i < count; i++) {
            let midi;
            if (hint === 'key') {
                // Guided round: walk through the new notes in order.
                midi = focus[i % focus.length];
            } else {
                const from = lesson.focus && Math.random() < 0.55 ? focus : pool;
                do midi = from[Math.floor(Math.random() * from.length)];
                while (midi === last && pool.length > 1);
            }
            last = midi;
            prompts.push({ midi, hint });
        }
    }
    return prompts;
}
