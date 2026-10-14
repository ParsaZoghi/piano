import { num, t } from '../core/i18n.js';
import { typing } from '../input/typing.js';
import { LESSONS, UNITS, lessonGoal, lessonTitle } from '../game/lessons.js';
import { progress } from '../game/progress.js';
import { el, escapeHtml, icon, stars } from './dom.js';

function card(lesson, index) {
    const body = `
        <span class="lesson__num">${num(index + 1)}</span>
        <span class="lesson__text">
            <span class="eyebrow">${t(`mode.${lesson.mode}`)}</span>
            <strong>${escapeHtml(lessonTitle(lesson))}</strong>
            <span>${escapeHtml(lessonGoal(lesson))}</span>
        </span>`;
    if (!progress.isUnlocked(index)) {
        return `<li><div class="lesson lesson--locked" title="${t('list.lockedTitle', { n: index })}">${body}<span class="lesson__state">${icon('lock')}<span class="sr-only">${t('list.locked', { n: index })}</span></span></div></li>`;
    }
    return `<li><a class="lesson" href="#/lesson/${lesson.id}">${body}<span class="lesson__state">${stars(progress.stars(lesson.id))}</span></a></li>`;
}

export function mountLessonList({ scene, ui }) {
    scene.configure({ view: 'menu' });
    scene.keyboard.setLabels({ names: false, typingMap: null });
    typing.configure();

    const units = UNITS.map((unit, u) => {
        const items = LESSONS.map((lesson, i) => (lesson.unit === unit ? card(lesson, i) : '')).join('');
        return `<section class="unit" aria-labelledby="unit-${unit}">
                <header><span class="eyebrow">${t('list.unit', { n: u + 1 })}</span><h2 id="unit-${unit}">${t(`units.${unit}.title`)}</h2><p>${t(`units.${unit}.blurb`)}</p></header>
                <ol class="unit__list">${items}</ol>
            </section>`;
    }).join('');

    const view = el(`
        <main class="page">
            <header class="topbar topbar--page">
                <a class="icon-btn" href="#/" aria-label="${t('common.backMenu')}">${icon('back')}</a>
                <div class="topbar__title"><span class="eyebrow">${t('list.eyebrow')}</span><h1 tabindex="-1">${t('list.title')}</h1></div>
                <div class="stat"><span class="stat__label">${t('list.stars')}</span><span class="stat__value">${num(progress.totalStars())} / ${num(LESSONS.length * 3)}</span></div>
            </header>
            <div class="page__body">${units}</div>
        </main>`);
    ui.append(view);
    return () => view.remove();
}
