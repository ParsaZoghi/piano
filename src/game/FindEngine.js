import { buildPrompts } from './lessons.js';

const NEXT_DELAY = 480; // ms the correct answer stays on screen
const REVEAL_AFTER = 2; // wrong tries before the key lights up

/** Runs "find" lessons: show a note, wait for the matching key. No timing. */
export class FindEngine {
    constructor(lesson, scene, callbacks) {
        this.lesson = lesson;
        this.scene = scene;
        this.callbacks = callbacks;
        this.timer = 0;
        this.prepare();
    }

    prepare() {
        clearTimeout(this.timer);
        this.prompts = buildPrompts(this.lesson);
        this.index = 0;
        this.running = false;
        this.locked = false;
        this.score = 0;
        this.combo = 0;
        this.maxCombo = 0;
        this.firstTry = 0;
        this.wrong = 0;
        this.scene.keyboard.setHint(null);
    }

    start() {
        this.running = true;
        this.present();
    }

    get progress() {
        return this.index / this.prompts.length;
    }

    present() {
        const prompt = this.prompts[this.index];
        this.tries = 0;
        this.locked = false;
        this.scene.keyboard.setHint(prompt.hint === 'key' ? prompt.midi : null);
        this.callbacks.onPrompt?.({ ...prompt, index: this.index, total: this.prompts.length, solved: false });
    }

    update() {}

    onNoteOn(midi) {
        if (!this.running || this.locked) return;
        const prompt = this.prompts[this.index];

        if (midi !== prompt.midi) {
            this.tries++;
            this.wrong++;
            this.combo = 0;
            this.scene.keyboard.flash(midi, 'bad');
            this.callbacks.onJudge?.({ kind: 'wrong', played: midi });
            if (this.tries >= REVEAL_AFTER) this.scene.keyboard.setHint(prompt.midi);
            return;
        }

        this.locked = true;
        if (this.tries === 0) this.firstTry++;
        this.combo = this.tries === 0 ? this.combo + 1 : 0;
        this.maxCombo = Math.max(this.maxCombo, this.combo);
        this.score += (this.tries === 0 ? 100 : this.tries === 1 ? 50 : 20) + Math.min(this.combo, 20) * 2;
        this.scene.keyboard.setHint(null);
        this.scene.keyboard.flash(midi, 'good');
        this.callbacks.onJudge?.({ kind: this.tries === 0 ? 'correct' : 'found' });
        this.callbacks.onPrompt?.({ ...prompt, index: this.index, total: this.prompts.length, solved: true });

        this.timer = setTimeout(() => {
            this.index++;
            if (this.index < this.prompts.length) return this.present();
            this.running = false;
            this.callbacks.onFinish?.({
                accuracy: this.firstTry / this.prompts.length,
                score: this.score,
                maxCombo: this.maxCombo,
                firstTry: this.firstTry,
                wrong: this.wrong,
                total: this.prompts.length,
            });
        }, NEXT_DELAY);
    }

    onNoteOff() {}

    dispose() {
        clearTimeout(this.timer);
        this.running = false;
        this.scene.keyboard.setHint(null);
    }
}
