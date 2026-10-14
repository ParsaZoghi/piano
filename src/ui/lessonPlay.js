import { synth } from '../audio/synth.js';
import { num, t } from '../core/i18n.js';
import { input } from '../core/input.js';
import { noteName, parseNote, pitchName } from '../core/notes.js';
import { onSettingsChange, saveSettings, settings } from '../core/settings.js';
import { typing } from '../input/typing.js';
import { FindEngine } from '../game/FindEngine.js';
import { LESSONS, lessonGoal, lessonIndex, lessonRange, lessonTips, lessonTitle } from '../game/lessons.js';
import { progress } from '../game/progress.js';
import { TimedEngine } from '../game/TimedEngine.js';
import { el, escapeHtml, icon, stars } from './dom.js';
import { staffSvg, valueKey } from './staff.js';

const SPEEDS = [0.6, 0.8, 1];
const BAD = new Set(['miss', 'wrong', 'extra', 'short']);
// Rhythm lessons only score timing, so their notes are drawn on the middle line.
const RHYTHM_STAFF_NOTE = parseNote('B4');
const RHYTHM_KEY_WIDTH = 1.5;

function timedTip(r) {
    if (r.counts.miss / r.total > 0.3) return t('tip.missed');
    if (r.shortHolds > 1) return t('tip.hold');
    if (r.wrong > 3) return t('tip.wrong');
    if (r.meanOffset > 0.05) return t('tip.late');
    if (r.meanOffset < -0.05) return t('tip.early');
    return t('tip.clean');
}

function findTip(r) {
    if (r.accuracy >= 0.9) return t('tip.findGreat');
    if (r.wrong > r.total / 2) return t('tip.findSlow');
    return t('tip.findClose');
}

export function mountLesson({ scene, ui, navigate }, id) {
    const index = lessonIndex(id);
    if (index < 0 || !progress.isUnlocked(index)) {
        navigate('/lessons');
        return () => {};
    }
    const lesson = LESSONS[index];
    const nextLesson = LESSONS[index + 1];
    const timed = lesson.mode !== 'find';
    const rhythm = lesson.mode === 'rhythm';
    const [lo, hi] = lessonRange(lesson);
    const title = lessonTitle(lesson);
    const eyebrow = t('lesson.eyebrow', { n: index + 1, mode: t(`mode.${lesson.mode}`) });

    scene.configure({ lo, hi, view: timed ? 'lesson' : 'find', highway: timed, unit: rhythm ? RHYTHM_KEY_WIDTH : 1 });
    typing.configure();
    const applyLabels = () => scene.keyboard.setLabels({ names: settings.labels, typingMap: settings.labels ? typing.labelMap() : null });
    applyLabels();

    const toggle = (key) => `<button class="chip" data-toggle="${key}" aria-pressed="${settings[key]}">${t(`lesson.${key === 'metronome' ? 'click' : key}`)}</button>`;
    const view = el(`
        <div class="hud">
            <header class="topbar">
                <a class="icon-btn" href="#/lessons" aria-label="${t('common.backLessons')}">${icon('back')}</a>
                <div class="topbar__title"><span class="eyebrow">${eyebrow}</span><h1 tabindex="-1">${escapeHtml(title)}</h1></div>
                <div class="progress" role="progressbar" aria-label="${t('lesson.progress')}" aria-valuemin="0" aria-valuemax="100" aria-valuenow="0"><div class="progress__bar"></div></div>
                <div class="stat"><span class="stat__label">${t('lesson.score')}</span><span class="stat__value" data-score>${num(0)}</span></div>
                <div class="stat"><span class="stat__label">${t('lesson.streak')}</span><span class="stat__value" data-combo>${num(0)}</span></div>
            </header>

            <section class="prompt ${timed ? 'prompt--side' : ''}" data-prompt hidden>
                <span class="eyebrow" data-step></span>
                <div class="prompt__row">
                    <div data-staff></div>
                    <div class="prompt__name" data-name></div>
                </div>
                <p class="prompt__msg" data-msg aria-live="polite"></p>
                ${timed ? '<div class="judge judge--inline" data-judge aria-live="polite"></div>' : ''}
            </section>

            ${timed ? '' : '<div class="judge judge--float" data-judge aria-live="polite"></div>'}
            <div class="countin" data-countin aria-live="polite"></div>

            <footer class="toolbar" data-toolbar hidden>
                ${timed ? `<button class="btn btn--ghost" data-action="listen">${icon('listen')}<span>${t('lesson.listen')}</span></button>` : ''}
                ${timed ? `<button class="btn btn--ghost" data-action="pause" aria-pressed="false">${icon('pause')}<span>${t('lesson.pause')}</span></button>` : ''}
                <button class="btn btn--ghost" data-action="retry">${icon('restart')}<span>${t('lesson.restart')}</span></button>
                ${
                    timed
                        ? `<div class="segmented" role="group" aria-label="${t('lesson.speed')}" dir="ltr">${SPEEDS.map(
                              (s) => `<button data-speed="${s}" aria-pressed="${settings.speed === s}">${num(Math.round(s * 100))}%</button>`,
                          ).join('')}</div>`
                        : ''
                }
                ${lesson.mode === 'play' ? toggle('wait') : ''}
                ${timed ? toggle('metronome') + toggle('hints') : ''}
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
    const pauseButton = $('[data-action="pause"]');
    const judgeClass = `judge ${timed ? 'judge--inline' : 'judge--float'}`;
    const shown = { score: -1, combo: -1, progress: -1, next: undefined, waiting: null };

    function showJudge({ kind, offset }) {
        let text = t(`judge.${kind}`);
        if ((kind === 'good' || kind === 'ok') && Math.abs(offset) > 0.09) text += ` · ${t(offset > 0 ? 'judge.late' : 'judge.early')}`;
        judgeBox.textContent = text;
        judgeBox.className = `${judgeClass} judge--${BAD.has(kind) ? 'bad' : kind === 'ok' ? 'ok' : 'good'}`;
        void judgeBox.offsetWidth; // restart the pop animation
        judgeBox.classList.add('judge--show');
    }

    function showFindPrompt({ midi, hint, index: i, total, solved }) {
        promptBox.hidden = false;
        $('[data-step]').textContent = t('lesson.findStep', { i: i + 1, total, kind: t(`lesson.kind.${hint}`) });
        $('[data-staff]').innerHTML = staffSvg(midi);
        $('[data-name]').textContent = hint !== 'staff' || solved ? pitchName(midi) : '?';
        $('[data-msg]').textContent = solved ? t('lesson.solved', { note: noteName(midi) }) : t(`lesson.find.${hint}`);
    }

    function showTimedPrompt(note, waiting) {
        promptBox.hidden = false;
        const name = $('[data-name]');
        if (rhythm) {
            $('[data-step]').textContent = t('lesson.bpm', { n: lesson.bpm });
            $('[data-staff]').innerHTML = staffSvg(note ? RHYTHM_STAFF_NOTE : null, note?.beats);
            name.innerHTML = note ? `<span class="prompt__value">${t(`value.${valueKey(note.beats)}`)}<small>${t(`beats.${valueKey(note.beats)}`)}</small></span>` : '';
            $('[data-msg]').textContent = t('lesson.rhythmMsg');
            return;
        }
        $('[data-step]').textContent = note ? t('lesson.next') : t('lesson.done');
        $('[data-staff]').innerHTML = staffSvg(note ? note.midi : null, note?.beats);
        name.textContent = note ? pitchName(note.midi) : '';
        $('[data-msg]').textContent = waiting ? t('lesson.waiting') : settings.wait ? t('lesson.waitOn') : t('lesson.playMsg');
    }

    const callbacks = {
        onJudge(judgement) {
            showJudge(judgement);
            if (!timed && judgement.kind === 'wrong') $('[data-msg]').textContent = t('lesson.wrongWas', { note: pitchName(judgement.played) });
        },
        onPrompt: showFindPrompt,
        onCountIn(n) {
            countBox.textContent = n ? num(n) : '';
            countBox.className = `countin${n > 0 ? ' countin--show' : ''}`;
        },
        onFinish(result) {
            setPaused(false);
            if (result.demo) return showIntro(t('lesson.yourTurn'));
            const earned = progress.record(lesson.id, result.accuracy);
            synth.fanfare(earned);
            showResults(result, earned);
        },
    };
    const engine = timed ? new TimedEngine(lesson, scene, callbacks) : new FindEngine(lesson, scene, callbacks);

    function setPaused(paused) {
        if (!pauseButton) return;
        pauseButton.setAttribute('aria-pressed', paused);
        pauseButton.innerHTML = `${icon(paused ? 'play' : 'pause')}<span>${t(paused ? 'lesson.resume' : 'lesson.pause')}</span>`;
        countBox.textContent = paused ? t('lesson.paused') : '';
        countBox.className = `countin${paused ? ' countin--show countin--paused' : ''}`;
    }

    function togglePause(force) {
        if (!timed) return;
        const paused = engine.togglePause(force);
        if (paused) synth.allOff();
        setPaused(paused);
    }

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
                <span class="eyebrow">${eyebrow}</span>
                <h2 id="intro-title">${escapeHtml(title)}</h2>
                <p class="card__lead">${escapeHtml(note || lessonGoal(lesson))}</p>
                <ul class="tips">${lessonTips(lesson)
                    .map((tip) => `<li>${icon('check')}<span>${escapeHtml(tip)}</span></li>`)
                    .join('')}</ul>
                <div class="card__actions">
                    <button class="btn btn--primary" data-action="start">${icon('play')}${t('lesson.start')}</button>
                    ${timed ? `<button class="btn" data-action="listen">${icon('listen')}${t('lesson.listenFirst')}</button>` : ''}
                </div>
            </div>`);
    }

    function showResults(result, earned) {
        promptBox.hidden = true;
        const rows = timed
            ? [
                  ['perfect', result.counts.perfect],
                  ['good', result.counts.good],
                  ['ok', result.counts.ok],
                  ['missed', result.counts.miss],
                  ['wrong', result.wrong],
                  result.shortHolds ? ['early', result.shortHolds] : ['streak', result.maxCombo],
              ]
            : [
                  ['firstTry', `${num(result.firstTry)} / ${num(result.total)}`],
                  ['wrong', result.wrong],
                  ['streak', result.maxCombo],
              ];
        const passed = earned > 0;
        openOverlay(`
            <div class="card card--result" role="dialog" aria-labelledby="result-title">
                <span class="eyebrow">${escapeHtml(title)}</span>
                <h2 id="result-title">${t(`result.title${earned}`)}</h2>
                <div class="result__stars">${stars(earned)}</div>
                <p class="result__score">${t('result.score', { percent: Math.round(result.accuracy * 100), score: result.score })}</p>
                <dl class="result__grid">${rows.map(([k, v]) => `<div><dt>${t(`result.${k}`)}</dt><dd>${typeof v === 'number' ? num(v) : v}</dd></div>`).join('')}</dl>
                <p class="card__lead">${timed ? timedTip(result) : findTip(result)}</p>
                ${passed ? '' : `<p class="muted">${t('result.need')}</p>`}
                <div class="card__actions">
                    ${passed && nextLesson ? `<a class="btn btn--primary" href="#/lesson/${nextLesson.id}">${t('result.next')}${icon('next')}</a>` : ''}
                    <button class="btn ${passed && nextLesson ? '' : 'btn--primary'}" data-action="retry">${icon('restart')}${t('result.again')}</button>
                    <a class="btn btn--ghost" href="#/lessons">${t('result.all')}</a>
                </div>
            </div>`);
    }

    function begin(demo) {
        synth.unlock();
        synth.allOff();
        overlay.classList.remove('overlay--open');
        overlay.innerHTML = '';
        $('[data-toolbar]').hidden = false;
        judgeBox.className = judgeClass;
        judgeBox.textContent = '';
        callbacks.onCountIn(0);
        setPaused(false);
        engine.prepare(demo);
        shown.next = undefined;
        if (timed) showTimedPrompt(engine.next, false);
        if (demo) $('[data-msg]').textContent = t('lesson.demoMsg');
        engine.start();
        view.querySelector('h1').focus({ preventScroll: true });
    }

    view.addEventListener('click', (e) => {
        const action = e.target.closest('[data-action]')?.dataset.action;
        const speed = e.target.closest('[data-speed]');
        const toggled = e.target.closest('[data-toggle]');
        if (action === 'start' || action === 'retry') begin(false);
        else if (action === 'listen') begin(true);
        else if (action === 'pause') togglePause();
        else if (speed) {
            saveSettings({ speed: Number(speed.dataset.speed) });
            view.querySelectorAll('[data-speed]').forEach((b) => b.setAttribute('aria-pressed', b === speed));
        } else if (toggled) {
            const key = toggled.dataset.toggle;
            saveSettings({ [key]: !settings[key] });
            toggled.setAttribute('aria-pressed', settings[key]);
            shown.waiting = null; // refresh the prompt text
        }
    });

    // Escape pauses / resumes; leaving the tab pauses.
    const onKey = (e) => {
        if (e.key === 'Escape' && !document.querySelector('dialog[open]') && !overlay.classList.contains('overlay--open')) togglePause();
    };
    const onHidden = () => document.hidden && togglePause(true);
    document.addEventListener('keydown', onKey);
    document.addEventListener('visibilitychange', onHidden);

    const offSettings = onSettingsChange((patch) => 'labels' in patch && applyLabels());
    const offNoteOn = input.on('noteon', (e) => engine.onNoteOn(e.midi));
    const offNoteOff = input.on('noteoff', (e) => engine.onNoteOff(e.midi));
    const offFrame = scene.onFrame((dt) => {
        engine.update(dt);
        if (engine.score !== shown.score) $('[data-score]').textContent = num((shown.score = engine.score));
        if (engine.combo !== shown.combo) $('[data-combo]').textContent = num((shown.combo = engine.combo));
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
        document.removeEventListener('keydown', onKey);
        document.removeEventListener('visibilitychange', onHidden);
        offSettings();
        offNoteOn();
        offNoteOff();
        offFrame();
        engine.dispose();
        view.remove();
    };
}
