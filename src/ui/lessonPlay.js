import { synth } from '../audio/synth.js';
import { input } from '../core/input.js';
import { noteName, pitchName } from '../core/notes.js';
import { saveSettings, settings } from '../core/settings.js';
import { typing } from '../input/typing.js';
import { FindEngine } from '../game/FindEngine.js';
import { LESSONS, lessonIndex, lessonRange } from '../game/lessons.js';
import { progress, starsFor } from '../game/progress.js';
import { TimedEngine } from '../game/TimedEngine.js';
import { el, escapeHtml, icon, stars } from './dom.js';
import { noteValue, staffSvg } from './staff.js';

const MODE_LABEL = { find: 'Notes', rhythm: 'Rhythm', play: 'Song' };
const SPEEDS = [0.6, 0.8, 1];
const JUDGE_TEXT = {
    perfect: 'Perfect',
    good: 'Good',
    ok: 'OK',
    miss: 'Missed',
    wrong: 'Wrong key',
    extra: 'Not yet',
    short: 'Hold it longer',
    correct: 'Correct',
    found: 'Got it',
};
const FIND_MESSAGE = {
    key: 'Press the glowing key.',
    name: 'Find this note on the keyboard.',
    staff: 'Which note is this? Press its key.',
};

function timedTip(r) {
    if (r.counts.miss / r.total > 0.3) return 'Lots of notes slipped past. Try a slower speed, or turn on Wait mode so the notes wait for you.';
    if (r.shortHolds > 1) return 'Keep long notes held down until the end of the block.';
    if (r.wrong > 3) return 'A few wrong keys crept in. Read the letter on each block before it arrives.';
    if (r.meanOffset > 0.05) return 'You tend to press a little late. Aim for the moment the block touches the green line.';
    if (r.meanOffset < -0.05) return 'You tend to press a little early. Let the click lead you rather than racing it.';
    return 'Clean and steady. You are ready for the next one.';
}

function findTip(r) {
    if (r.accuracy >= 0.9) return 'You know these notes. On to the next ones.';
    if (r.wrong > r.total / 2) return 'Take your time: there is no clock here. Look for the groups of two and three black keys to find your place.';
    return 'Nearly there. One more round will make these notes stick.';
}

export function mountLesson({ scene, ui, navigate, openDevices }, id) {
    const index = lessonIndex(id);
    if (index < 0 || !progress.isUnlocked(index)) {
        navigate('/lessons');
        return () => {};
    }
    const lesson = LESSONS[index];
    const nextLesson = LESSONS[index + 1];
    const timed = lesson.mode !== 'find';
    const [lo, hi] = lessonRange(lesson);

    scene.configure({ lo, hi, view: timed ? 'lesson' : 'find', highway: timed });
    typing.configure();
    const applyLabels = () => scene.keyboard.setLabels({ names: settings.labels, typingMap: settings.labels ? typing.labelMap() : null });
    applyLabels();

    const toggle = (key, label) => `<button class="chip" data-toggle="${key}" aria-pressed="${settings[key]}">${label}</button>`;
    const view = el(`
        <div class="hud">
            <header class="topbar">
                <a class="icon-btn" href="#/lessons" aria-label="Back to lessons">${icon('back')}</a>
                <div class="topbar__title"><span class="eyebrow">Lesson ${index + 1} · ${MODE_LABEL[lesson.mode]}</span><h1 tabindex="-1">${escapeHtml(lesson.title)}</h1></div>
                <div class="progress" role="progressbar" aria-label="Lesson progress" aria-valuemin="0" aria-valuemax="100" aria-valuenow="0"><div class="progress__bar"></div></div>
                <div class="stat"><span class="stat__label">Score</span><span class="stat__value" data-score>0</span></div>
                <div class="stat"><span class="stat__label">Streak</span><span class="stat__value" data-combo>0</span></div>
            </header>

            <section class="prompt ${timed ? 'prompt--side' : ''}" data-prompt hidden>
                <span class="eyebrow" data-step></span>
                <div class="prompt__row">
                    <div data-staff></div>
                    <div class="prompt__name" data-name></div>
                    <div class="prompt__values" data-values></div>
                </div>
                <p class="prompt__msg" data-msg aria-live="polite"></p>
            </section>

            <div class="judge" data-judge aria-live="polite"></div>
            <div class="countin" data-countin aria-hidden="true"></div>

            <footer class="toolbar" data-toolbar hidden>
                ${timed ? `<button class="btn btn--ghost" data-action="listen">${icon('listen')}<span>Listen</span></button>` : ''}
                <button class="btn btn--ghost" data-action="retry">${icon('restart')}<span>Restart</span></button>
                ${
                    timed
                        ? `<div class="segmented" role="group" aria-label="Practice speed">${SPEEDS.map(
                              (s) => `<button data-speed="${s}" aria-pressed="${settings.speed === s}">${Math.round(s * 100)}%</button>`,
                          ).join('')}</div>`
                        : ''
                }
                ${lesson.mode === 'play' ? toggle('wait', 'Wait mode') : ''}
                ${timed ? toggle('metronome', 'Click') + toggle('hints', 'Key hints') : ''}
                ${toggle('labels', 'Key labels')}
                <button class="btn btn--ghost" data-devices>${icon('plug')}<span>Devices</span></button>
            </footer>

            <div class="overlay" data-overlay></div>
        </div>`);
    ui.append(view);

    const $ = (selector) => view.querySelector(selector);
    const overlay = $('[data-overlay]');
    const promptBox = $('[data-prompt]');
    const judgeBox = $('[data-judge]');
    const countBox = $('[data-countin]');
    const progressBox = $('.progress');
    const shown = { score: -1, combo: -1, progress: -1, next: undefined, waiting: null };

    function showJudge({ kind, offset }) {
        let text = JUDGE_TEXT[kind];
        if ((kind === 'good' || kind === 'ok') && Math.abs(offset) > 0.09) text += offset > 0 ? ' · late' : ' · early';
        judgeBox.textContent = text;
        judgeBox.className = `judge judge--${['miss', 'wrong', 'extra', 'short'].includes(kind) ? 'bad' : kind === 'ok' ? 'ok' : 'good'}`;
        void judgeBox.offsetWidth; // restart the pop animation
        judgeBox.classList.add('judge--show');
    }

    function showFindPrompt({ midi, hint, index: i, total, solved }) {
        promptBox.hidden = false;
        $('[data-step]').textContent = `Note ${i + 1} of ${total} · ${hint === 'key' ? 'guided' : hint === 'name' ? 'by name' : 'from the staff'}`;
        $('[data-staff]').innerHTML = staffSvg(midi);
        $('[data-name]').textContent = hint !== 'staff' || solved ? pitchName(midi) : '?';
        $('[data-msg]').textContent = solved ? `Yes, that is ${noteName(midi)}.` : FIND_MESSAGE[hint];
    }

    function showTimedPrompt(note, waiting) {
        promptBox.hidden = false;
        if (lesson.mode === 'rhythm') {
            $('[data-step]').textContent = `${lesson.bpm} beats per minute`;
            $('[data-values]').innerHTML = lesson.values.map(noteValue).join('');
            $('[data-msg]').textContent = 'Press any key as each block reaches the green line.';
            return;
        }
        $('[data-step]').textContent = note ? 'Next note' : 'Well played';
        $('[data-staff]').innerHTML = staffSvg(note ? note.midi : null);
        $('[data-name]').textContent = note ? pitchName(note.midi) : '';
        $('[data-msg]').textContent = waiting ? 'Waiting for you. Take your time.' : settings.wait ? 'Wait mode is on: the notes will wait for you.' : 'Press each key as its block reaches the green line.';
    }

    const callbacks = {
        onJudge(judgement) {
            showJudge(judgement);
            if (!timed && judgement.kind === 'wrong') $('[data-msg]').textContent = `That was ${pitchName(judgement.played)}. Try again.`;
        },
        onPrompt: showFindPrompt,
        onCountIn(n) {
            countBox.textContent = n || '';
            countBox.classList.toggle('countin--show', n > 0);
        },
        onFinish(result) {
            if (result.demo) return showIntro('Your turn. Play it the way you just heard it.');
            const earned = progress.record(lesson.id, result.accuracy);
            synth.fanfare(earned);
            showResults(result, earned);
        },
    };
    const engine = timed ? new TimedEngine(lesson, scene, callbacks) : new FindEngine(lesson, scene, callbacks);

    function openOverlay(html) {
        overlay.innerHTML = html;
        overlay.classList.add('overlay--open');
        $('[data-toolbar]').hidden = true;
        overlay.querySelector('.btn--primary')?.focus();
    }

    function showIntro(note = '') {
        promptBox.hidden = true;
        openOverlay(`
            <div class="card" role="dialog" aria-labelledby="intro-title">
                <span class="eyebrow">Lesson ${index + 1} · ${MODE_LABEL[lesson.mode]}</span>
                <h2 id="intro-title">${escapeHtml(lesson.title)}</h2>
                <p class="card__lead">${escapeHtml(note || lesson.goal)}</p>
                <ul class="tips">${lesson.tips.map((tip) => `<li>${icon('check')}<span>${escapeHtml(tip)}</span></li>`).join('')}</ul>
                <div class="card__actions">
                    <button class="btn btn--primary" data-action="start">${icon('play')}Start</button>
                    ${timed ? `<button class="btn" data-action="listen">${icon('listen')}Listen first</button>` : ''}
                </div>
            </div>`);
    }

    function showResults(result, earned) {
        promptBox.hidden = true;
        const percent = Math.round(result.accuracy * 100);
        const rows = timed
            ? [
                  ['Perfect', result.counts.perfect],
                  ['Good', result.counts.good],
                  ['OK', result.counts.ok],
                  ['Missed', result.counts.miss],
                  ['Wrong keys', result.wrong],
                  result.shortHolds ? ['Released early', result.shortHolds] : ['Best streak', result.maxCombo],
              ]
            : [
                  ['First try', `${result.firstTry} / ${result.total}`],
                  ['Wrong keys', result.wrong],
                  ['Best streak', result.maxCombo],
              ];
        const passed = earned > 0;
        openOverlay(`
            <div class="card card--result" role="dialog" aria-labelledby="result-title">
                <span class="eyebrow">${escapeHtml(lesson.title)}</span>
                <h2 id="result-title">${earned === 3 ? 'Outstanding' : earned === 2 ? 'Well done' : earned === 1 ? 'Lesson passed' : 'Not quite yet'}</h2>
                <div class="result__stars">${stars(earned)}</div>
                <p class="result__score"><strong>${percent}%</strong> accuracy · ${result.score} points</p>
                <dl class="result__grid">${rows.map(([k, v]) => `<div><dt>${k}</dt><dd>${v}</dd></div>`).join('')}</dl>
                <p class="card__lead">${timed ? timedTip(result) : findTip(result)}</p>
                ${passed ? '' : '<p class="muted">You need 50% for the first star and to unlock the next lesson.</p>'}
                <div class="card__actions">
                    ${passed && nextLesson ? `<a class="btn btn--primary" href="#/lesson/${nextLesson.id}">Next lesson${icon('next')}</a>` : ''}
                    <button class="btn ${passed && nextLesson ? '' : 'btn--primary'}" data-action="retry">${icon('restart')}Try again</button>
                    <a class="btn btn--ghost" href="#/lessons">All lessons</a>
                </div>
            </div>`);
    }

    function begin(demo) {
        synth.unlock();
        synth.allOff();
        overlay.classList.remove('overlay--open');
        overlay.innerHTML = '';
        $('[data-toolbar]').hidden = false;
        judgeBox.className = 'judge';
        callbacks.onCountIn(0);
        engine.prepare(demo);
        shown.next = undefined;
        if (timed) showTimedPrompt(engine.next, false);
        if (demo) $('[data-msg]').textContent = 'Listen and watch the keys.';
        engine.start();
        view.querySelector('h1').focus({ preventScroll: true });
    }

    view.addEventListener('click', (e) => {
        const action = e.target.closest('[data-action]')?.dataset.action;
        const speed = e.target.closest('[data-speed]');
        const toggled = e.target.closest('[data-toggle]');
        if (action === 'start' || action === 'retry') begin(false);
        else if (action === 'listen') begin(true);
        else if (speed) {
            saveSettings({ speed: Number(speed.dataset.speed) });
            view.querySelectorAll('[data-speed]').forEach((b) => b.setAttribute('aria-pressed', b === speed));
        } else if (toggled) {
            const key = toggled.dataset.toggle;
            saveSettings({ [key]: !settings[key] });
            toggled.setAttribute('aria-pressed', settings[key]);
            if (key === 'labels') applyLabels();
            shown.waiting = null; // refresh the prompt text
        } else if (e.target.closest('[data-devices]')) openDevices();
    });

    const offNoteOn = input.on('noteon', (e) => engine.onNoteOn(e.midi));
    const offNoteOff = input.on('noteoff', (e) => engine.onNoteOff(e.midi));
    const offFrame = scene.onFrame((dt) => {
        engine.update(dt);
        if (engine.score !== shown.score) $('[data-score]').textContent = shown.score = engine.score;
        if (engine.combo !== shown.combo) $('[data-combo]').textContent = shown.combo = engine.combo;
        const percent = Math.round(engine.progress * 100);
        if (percent !== shown.progress) {
            shown.progress = percent;
            progressBox.setAttribute('aria-valuenow', percent);
            progressBox.firstElementChild.style.transform = `scaleX(${percent / 100})`;
        }
        if (timed && engine.running && !engine.demo && (engine.next !== shown.next || engine.waiting !== shown.waiting)) {
            shown.next = engine.next;
            shown.waiting = engine.waiting;
            showTimedPrompt(engine.next, engine.waiting);
        }
    });

    showIntro();

    return () => {
        offNoteOn();
        offNoteOff();
        offFrame();
        engine.dispose();
        view.remove();
    };
}
