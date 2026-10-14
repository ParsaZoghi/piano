import { parseNote } from '../core/notes.js';

// The curriculum: notes first, rhythm second, then both together in songs.
// Three lesson modes:
//   find   - untimed: press the key for the note shown (hints fade per round)
//   rhythm - timed: tap any key in time; only the rhythm is scored
//   play   - timed: right key at the right time
//
// Songs are written as "NOTE:beats" tokens ("R" is a rest, "X" a rhythm tap,
// "|" is ignored and only marks bars for readability).

export const UNITS = [
    { id: 'notes', title: 'Meet the notes', blurb: 'Where each note lives on the keyboard and on the staff.' },
    { id: 'rhythm', title: 'Feel the rhythm', blurb: 'Steady beats, long notes, short notes and silence.' },
    { id: 'songs', title: 'Play real songs', blurb: 'Put notes and rhythm together.' },
];

const WHITE_5 = ['C4', 'D4', 'E4', 'F4', 'G4'];
const WHITE_8 = [...WHITE_5, 'A4', 'B4', 'C5'];

export const LESSONS = [
    {
        id: 'n1',
        unit: 'notes',
        mode: 'find',
        title: 'C, D and E',
        goal: 'Find your first three notes.',
        range: ['C4', 'E4'],
        pool: ['C4', 'D4', 'E4'],
        rounds: [['key', 6], ['name', 6], ['staff', 6]],
        tips: ['C is the white key just left of the two black keys.', 'On your keyboard: A plays C, S plays D, D plays E.', 'The glowing key helps at first, then the hint fades.'],
    },
    {
        id: 'n2',
        unit: 'notes',
        mode: 'find',
        title: 'F and G',
        goal: 'Add two more notes to make a five-note hand position.',
        range: ['C4', 'G4'],
        pool: WHITE_5,
        focus: ['F4', 'G4'],
        rounds: [['key', 6], ['name', 8], ['staff', 8]],
        tips: ['F is the white key just left of the three black keys.', 'F and G are on the F and G typing keys.', 'Notes climb the staff one step at a time: line, space, line, space.'],
    },
    {
        id: 'n3',
        unit: 'notes',
        mode: 'find',
        title: 'Five-note review',
        goal: 'C D E F G with no glowing keys.',
        range: ['C4', 'G4'],
        pool: WHITE_5,
        rounds: [['name', 8], ['staff', 10]],
        tips: ['Rest five fingers on A S D F G and try not to look down.', 'Miss twice and the key lights up, so you are never stuck.'],
    },
    {
        id: 'n4',
        unit: 'notes',
        mode: 'find',
        title: 'A, B and high C',
        goal: 'Finish the whole octave.',
        range: ['C4', 'C5'],
        pool: WHITE_8,
        focus: ['A4', 'B4', 'C5'],
        rounds: [['key', 6], ['name', 8], ['staff', 10]],
        tips: ['After G the musical alphabet starts again: A, B, then C.', 'Every C looks the same: left of a pair of black keys.'],
    },
    {
        id: 'n5',
        unit: 'notes',
        mode: 'find',
        title: 'The black keys',
        goal: 'Meet the sharps.',
        range: ['C4', 'C5'],
        pool: ['C#4', 'D#4', 'F#4', 'G#4', 'A#4'],
        rounds: [['key', 5], ['name', 10], ['staff', 6]],
        tips: ['A sharp (♯) is the black key just to the right of a white key.', 'Black keys sit on the row above: W E T Y U.', 'They come in a group of two, then a group of three.'],
    },
    {
        id: 'n6',
        unit: 'notes',
        mode: 'find',
        title: 'Staff reading challenge',
        goal: 'Read ten white-key notes straight from the staff.',
        range: ['C4', 'E5'],
        pool: [...WHITE_8, 'D5', 'E5'],
        rounds: [['name', 8], ['staff', 14]],
        tips: ['Lines from the bottom: E G B D F. Spaces: F A C E.', 'Middle C sits on its own short line below the staff.'],
    },
    {
        id: 'r1',
        unit: 'rhythm',
        mode: 'rhythm',
        title: 'The steady beat',
        goal: 'Quarter notes: one tap per beat.',
        range: ['C4', 'E4'],
        lane: 'D4',
        bpm: 76,
        values: [1],
        song: 'X X X X | X X X X | X X X X | X X X X',
        tips: ['Press any key when a block reaches the green line.', 'Listen to the click and count 1, 2, 3, 4.', 'Four clicks count you in before the notes arrive.'],
    },
    {
        id: 'r2',
        unit: 'rhythm',
        mode: 'rhythm',
        title: 'Long notes',
        goal: 'Half notes last two beats, whole notes four.',
        range: ['C4', 'E4'],
        lane: 'D4',
        bpm: 80,
        values: [1, 2, 4],
        song: 'X:2 X:2 | X X X:2 | X:2 X X | X:4 | X:2 X:2 | X X X X | X:2 X:2 | X:4',
        tips: ['Hold the key down for the whole length of a long block.', 'Keep counting in your head while you hold.'],
    },
    {
        id: 'r3',
        unit: 'rhythm',
        mode: 'rhythm',
        title: 'Eighth notes',
        goal: 'Two quick notes fit in one beat.',
        range: ['C4', 'E4'],
        lane: 'D4',
        bpm: 72,
        values: [0.5, 1, 2],
        song: 'X X X:.5 X:.5 X | X:.5 X:.5 X:.5 X:.5 X X | X X:.5 X:.5 X X:.5 X:.5 | X:.5 X:.5 X X:2 | X:.5 X:.5 X:.5 X:.5 X:.5 X:.5 X | X:2 X:2',
        tips: ['Count "1 and 2 and 3 and 4 and".', 'Eighth notes land on the numbers and on the "and"s.'],
    },
    {
        id: 'r4',
        unit: 'rhythm',
        mode: 'rhythm',
        title: 'Rests',
        goal: 'Silence has a length too.',
        range: ['C4', 'E4'],
        lane: 'D4',
        bpm: 80,
        values: [1, 2],
        song: 'X R X R | X X R X | R X R X | X:2 R:2 | X R X X | R X X R | X X R X | X:2 R:2',
        tips: ['A gap in the blocks is a rest: lift your finger and wait.', 'Keep counting through the silence.'],
    },
    {
        id: 's1',
        unit: 'songs',
        mode: 'play',
        title: 'Hot Cross Buns',
        goal: 'Three notes, one real tune.',
        range: ['C4', 'E4'],
        bpm: 84,
        song: 'E4 D4 C4:2 | E4 D4 C4:2 | C4:.5 C4:.5 C4:.5 C4:.5 D4:.5 D4:.5 D4:.5 D4:.5 | E4 D4 C4:2',
        tips: ['Tap "Listen first" to hear the tune.', 'Turn on Wait mode and the notes will wait for you.'],
    },
    {
        id: 's2',
        unit: 'songs',
        mode: 'play',
        title: 'Mary Had a Little Lamb',
        goal: 'Stepping between C and G.',
        range: ['C4', 'G4'],
        bpm: 92,
        song: 'E4 D4 C4 D4 | E4 E4 E4:2 | D4 D4 D4:2 | E4 G4 G4:2 | E4 D4 C4 D4 | E4 E4 E4 E4 | D4 D4 E4 D4 | C4:4',
        tips: ['Too fast? Lower the speed in the toolbar.', 'Watch the letter on each block as it comes toward you.'],
    },
    {
        id: 's3',
        unit: 'songs',
        mode: 'play',
        title: 'Ode to Joy',
        goal: 'A famous melody with a dotted rhythm.',
        range: ['C4', 'G4'],
        bpm: 96,
        song: 'E4 E4 F4 G4 | G4 F4 E4 D4 | C4 C4 D4 E4 | E4:1.5 D4:.5 D4:2 | E4 E4 F4 G4 | G4 F4 E4 D4 | C4 C4 D4 E4 | D4:1.5 C4:.5 C4:2',
        tips: ['The slightly longer block is held for a beat and a half.'],
    },
    {
        id: 's4',
        unit: 'songs',
        mode: 'play',
        title: 'Twinkle, Twinkle',
        goal: 'Jumping from C up to G, and reaching A.',
        range: ['C4', 'A4'],
        bpm: 96,
        song: 'C4 C4 G4 G4 | A4 A4 G4:2 | F4 F4 E4 E4 | D4 D4 C4:2 | G4 G4 F4 F4 | E4 E4 D4:2 | G4 G4 F4 F4 | E4 E4 D4:2 | C4 C4 G4 G4 | A4 A4 G4:2 | F4 F4 E4 E4 | D4 D4 C4:2',
        tips: ['C to G is a jump: A key to G key.', 'A is one step above G, on the H key.'],
    },
    {
        id: 's5',
        unit: 'songs',
        mode: 'play',
        title: 'Jingle Bells',
        goal: 'Quarter, half, dotted and eighth notes in one song.',
        range: ['C4', 'G4'],
        bpm: 100,
        song: 'E4 E4 E4:2 | E4 E4 E4:2 | E4 G4 C4:1.5 D4:.5 | E4:4 | F4 F4 F4:1.5 F4:.5 | F4 E4 E4 E4:.5 E4:.5 | E4 D4 D4 E4 | D4:2 G4:2',
        tips: ['Everything you have learned so far is in this one.'],
    },
    {
        id: 's6',
        unit: 'songs',
        mode: 'play',
        title: 'C major scale',
        goal: 'All eight notes, up and back down.',
        range: ['C4', 'C5'],
        bpm: 88,
        song: 'C4 D4 E4 F4 | G4 A4 B4 C5 | C5 B4 A4 G4 | F4 E4 D4 C4 | C4:.5 D4:.5 E4:.5 F4:.5 G4:.5 A4:.5 B4:.5 C5:.5 | C5:.5 B4:.5 A4:.5 G4:.5 F4:.5 E4:.5 D4:.5 C4:.5 | C4:4',
        tips: ['The second time is twice as fast: eighth notes.'],
    },
];

export const lessonIndex = (id) => LESSONS.findIndex((lesson) => lesson.id === id);
export const lessonRange = (lesson) => lesson.range.map(parseNote);

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
