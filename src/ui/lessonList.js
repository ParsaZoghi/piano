import { typing } from '../input/typing.js';
import { LESSONS, UNITS } from '../game/lessons.js';
import { progress } from '../game/progress.js';
import { el, escapeHtml, icon, stars } from './dom.js';

const MODE_LABEL = { find: 'Notes', rhythm: 'Rhythm', play: 'Song' };

function card(lesson, index) {
    const number = index + 1;
    const body = `
        <span class="lesson__num">${number}</span>
        <span class="lesson__text">
            <span class="eyebrow">${MODE_LABEL[lesson.mode]}</span>
            <strong>${escapeHtml(lesson.title)}</strong>
            <span>${escapeHtml(lesson.goal)}</span>
        </span>`;
    if (!progress.isUnlocked(index)) {
        return `<li><div class="lesson lesson--locked" title="Earn a star in lesson ${index} to open this one">${body}<span class="lesson__state">${icon('lock')}<span class="sr-only">Locked. Earn a star in lesson ${index} to open it.</span></span></div></li>`;
    }
    return `<li><a class="lesson" href="#/lesson/${lesson.id}">${body}<span class="lesson__state">${stars(progress.stars(lesson.id))}</span></a></li>`;
}

export function mountLessonList({ scene, ui }) {
    scene.configure({ view: 'menu' });
    scene.keyboard.setLabels({ names: false, typingMap: null });
    typing.configure();

    const units = UNITS.map((unit, u) => {
        const items = LESSONS.map((lesson, i) => (lesson.unit === unit.id ? card(lesson, i) : '')).join('');
        return `<section class="unit" aria-labelledby="unit-${unit.id}">
                <header><span class="eyebrow">Unit ${u + 1}</span><h2 id="unit-${unit.id}">${unit.title}</h2><p>${unit.blurb}</p></header>
                <ol class="unit__list">${items}</ol>
            </section>`;
    }).join('');

    const view = el(`
        <main class="page">
            <header class="topbar topbar--page">
                <a class="icon-btn" href="#/" aria-label="Back to menu">${icon('back')}</a>
                <div class="topbar__title"><span class="eyebrow">Learn</span><h1 tabindex="-1">Lessons</h1></div>
                <div class="stat"><span class="stat__label">Stars</span><span class="stat__value">${progress.totalStars()} / ${LESSONS.length * 3}</span></div>
            </header>
            <div class="page__body">${units}</div>
        </main>`);
    ui.append(view);
    return () => view.remove();
}
