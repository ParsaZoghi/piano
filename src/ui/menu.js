import { typing } from '../input/typing.js';
import { LESSONS } from '../game/lessons.js';
import { progress } from '../game/progress.js';
import { el, icon } from './dom.js';

export function mountMenu({ scene, ui, openDevices }) {
    scene.configure({ view: 'menu' });
    scene.keyboard.setLabels({ names: false, typingMap: null });
    typing.configure();

    const earned = progress.totalStars();
    const view = el(`
        <main class="menu">
            <div class="menu__brand">
                <span class="menu__logo">${icon('music')}</span>
                <h1 tabindex="-1">Keystage</h1>
                <p>Learn piano notes and rhythm, one small win at a time. All you need is this keyboard.</p>
            </div>
            <nav class="menu__actions" aria-label="Main menu">
                <a class="choice" href="#/free">
                    <span class="choice__icon">${icon('play')}</span>
                    <span class="choice__text"><strong>Free play</strong><span>The full 88-key piano. No goals, just play.</span></span>
                    ${icon('next', 'choice__arrow')}
                </a>
                <a class="choice choice--primary" href="#/lessons">
                    <span class="choice__icon">${icon('book')}</span>
                    <span class="choice__text"><strong>Learn lessons</strong><span>${LESSONS.length} short lessons: notes, rhythm, then real songs.${
                        earned ? ` You have ${earned} of ${LESSONS.length * 3} stars.` : ''
                    }</span></span>
                    ${icon('next', 'choice__arrow')}
                </a>
            </nav>
            <button class="btn btn--ghost" data-devices>${icon('plug')}Connect a MIDI keyboard or phone</button>
        </main>`);
    view.querySelector('[data-devices]').addEventListener('click', openDevices);
    ui.append(view);
    return () => view.remove();
}
