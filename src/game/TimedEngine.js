import { synth } from '../audio/synth.js';
import { pitchName } from '../core/notes.js';
import { settings } from '../core/settings.js';
import { parseSong } from './lessons.js';

// Timing windows in real seconds either side of the beat.
const WINDOWS = { perfect: 0.09, good: 0.17, ok: 0.28 };
const POINTS = { perfect: 100, good: 70, ok: 40 };
const COUNT_IN_BEATS = 4;
const BEAT_LABELS = { 0.5: '½', 1: '1', 1.5: '1½', 2: '2', 3: '3', 4: '4' };

/** Runs "rhythm" and "play" lessons: moves the highway and judges each press. */
export class TimedEngine {
    constructor(lesson, scene, callbacks) {
        this.lesson = lesson;
        this.scene = scene;
        this.callbacks = callbacks;
        this.anyKey = lesson.mode === 'rhythm';
        this.spb = 60 / lesson.bpm;

        const { notes, totalBeats } = parseSong(lesson);
        this.notes = notes.map((n) => ({
            midi: n.midi,
            beats: n.beats,
            time: n.beat * this.spb,
            dur: n.beats * this.spb,
            label: this.anyKey ? BEAT_LABELS[n.beats] || String(n.beats) : pitchName(n.midi),
            state: 'pending',
        }));
        this.endTime = (totalBeats + 1) * this.spb;
        this.running = false;
        this.prepare(false);
    }

    prepare(demo) {
        this.demo = demo;
        this.running = false;
        this.finished = false;
        this.waiting = false;
        this.t = -COUNT_IN_BEATS * this.spb;
        this.nextBeat = -COUNT_IN_BEATS;
        this.cursor = 0;
        this.score = 0;
        this.combo = 0;
        this.maxCombo = 0;
        this.points = 0;
        this.wrong = 0;
        this.shortHolds = 0;
        this.counts = { perfect: 0, good: 0, ok: 0, miss: 0 };
        this.offsets = [];
        this.held = new Map(); // midi -> note currently being held
        this.demoHeld = [];
        this.waitTries = 0;
        for (const note of this.notes) note.state = 'pending';
        this.scene.keyboard.releaseAll();
        this.scene.keyboard.setHint(null);
        this.scene.highway.setNotes(this.notes, { secondsPerBeat: this.spb });
        this.scene.highway.update(this.t);
    }

    start() {
        this.running = true;
    }

    get progress() {
        return Math.min(1, Math.max(0, this.t / this.endTime));
    }

    /** The next note that still has to be played, or null. */
    get next() {
        while (this.cursor < this.notes.length && this.notes[this.cursor].state !== 'pending') this.cursor++;
        return this.notes[this.cursor] || null;
    }

    update(dt) {
        if (!this.running) return;
        const speed = this.demo ? 1 : settings.speed;
        let t = this.t + dt * speed;

        // Wait mode: time stops at the next note until the learner plays it.
        const pending = this.next;
        this.waiting = false;
        if (!this.demo && settings.wait && !this.anyKey && pending && t >= pending.time) {
            t = pending.time;
            this.waiting = true;
        }

        while (this.nextBeat * this.spb <= t) {
            const counting = this.nextBeat < 0;
            if (counting || settings.metronome) synth.click(this.nextBeat % 4 === 0);
            if (this.nextBeat <= 0) this.callbacks.onCountIn?.(counting ? this.nextBeat + COUNT_IN_BEATS + 1 : 0);
            this.nextBeat++;
        }

        if (this.demo) this.playDemo(t);
        else {
            const late = WINDOWS.ok * speed;
            for (let i = this.cursor; i < this.notes.length && this.notes[i].time < t - late; i++) {
                if (this.notes[i].state === 'pending') this.miss(this.notes[i]);
            }
        }

        this.t = t;
        const upcoming = this.next;
        this.scene.keyboard.setHint(!this.demo && settings.hints && upcoming ? upcoming.midi : null);
        this.scene.highway.update(t);
        if (t >= this.endTime) this.finish();
    }

    playDemo(t) {
        for (let i = this.cursor; i < this.notes.length && this.notes[i].time <= t; i++) {
            const note = this.notes[i];
            if (note.state !== 'pending') continue;
            note.state = 'hit';
            synth.noteOn(note.midi, 0.7);
            this.scene.keyboard.press(note.midi, true);
            this.demoHeld.push({ midi: note.midi, until: note.time + note.dur * 0.85 });
        }
        this.demoHeld = this.demoHeld.filter((h) => {
            if (h.until > t) return true;
            synth.noteOff(h.midi);
            this.scene.keyboard.press(h.midi, false);
            return false;
        });
    }

    onNoteOn(midi) {
        if (!this.running || this.demo) return;
        const speed = settings.speed;

        if (this.waiting) {
            const note = this.next;
            if (note && note.midi === midi) {
                this.hit(note, midi, this.waitTries === 0 ? 'perfect' : 'good', 0);
                this.waitTries = 0;
            } else {
                this.waitTries++;
                this.wrongPress(midi);
            }
            return;
        }

        let best = null;
        let near = false;
        for (let i = this.cursor; i < this.notes.length; i++) {
            const note = this.notes[i];
            const offset = (this.t - note.time) / speed; // + late, - early
            if (offset < -WINDOWS.ok) break;
            if (note.state !== 'pending' || offset > WINDOWS.ok) continue;
            near = true;
            if ((this.anyKey || note.midi === midi) && (!best || Math.abs(offset) < Math.abs(best.offset))) best = { note, offset };
        }
        if (!best) return this.wrongPress(midi, near ? 'wrong' : 'extra');

        const abs = Math.abs(best.offset);
        const kind = abs <= WINDOWS.perfect ? 'perfect' : abs <= WINDOWS.good ? 'good' : 'ok';
        this.hit(best.note, midi, kind, best.offset);
    }

    onNoteOff(midi) {
        const note = this.held.get(midi);
        if (!note) return;
        this.held.delete(midi);
        // Long notes must actually be held.
        if (note.beats >= 2 && this.t - note.time < note.dur * 0.55 && this.running) {
            this.shortHolds++;
            this.callbacks.onJudge?.({ kind: 'short' });
        }
    }

    hit(note, midi, kind, offset) {
        note.state = 'hit';
        this.held.set(midi, note);
        this.counts[kind]++;
        this.points += POINTS[kind];
        this.combo++;
        this.maxCombo = Math.max(this.maxCombo, this.combo);
        this.score += POINTS[kind] + Math.min(this.combo, 20) * 2;
        this.offsets.push(offset);
        this.scene.keyboard.flash(midi, 'good');
        this.callbacks.onJudge?.({ kind, offset });
    }

    miss(note) {
        note.state = 'miss';
        this.counts.miss++;
        this.combo = 0;
        this.callbacks.onJudge?.({ kind: 'miss' });
    }

    wrongPress(midi, kind = 'wrong') {
        this.wrong++;
        this.combo = 0;
        this.scene.keyboard.flash(midi, 'bad');
        this.callbacks.onJudge?.({ kind });
    }

    finish() {
        this.running = false;
        this.finished = true;
        this.scene.keyboard.setHint(null);
        for (const h of this.demoHeld) {
            synth.noteOff(h.midi);
            this.scene.keyboard.press(h.midi, false);
        }
        this.demoHeld = [];
        if (this.demo) return this.callbacks.onFinish?.({ demo: true });

        const max = this.notes.length * POINTS.perfect;
        const penalty = this.wrong * 5 + this.shortHolds * 15;
        const accuracy = Math.max(0, this.points - penalty) / max;
        const mean = this.offsets.length ? this.offsets.reduce((a, b) => a + b, 0) / this.offsets.length : 0;
        this.callbacks.onFinish?.({
            accuracy,
            score: this.score,
            maxCombo: this.maxCombo,
            counts: this.counts,
            wrong: this.wrong,
            shortHolds: this.shortHolds,
            total: this.notes.length,
            meanOffset: mean,
        });
    }

    dispose() {
        this.running = false;
        for (const h of this.demoHeld) synth.noteOff(h.midi);
        this.scene.keyboard.setHint(null);
        this.scene.keyboard.releaseAll();
        this.scene.highway.setNotes([], { secondsPerBeat: this.spb });
    }
}
