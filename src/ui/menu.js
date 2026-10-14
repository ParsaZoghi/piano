import { t } from '../core/i18n.js';
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
                <h1 tabindex="-1" lang="en">Keystage</h1>
                <p>${t('menu.tagline')}</p>
            </div>
            <nav class="menu__actions" aria-label="${t('menu.nav')}">
                <a class="choice" href="#/free">
                    <span class="choice__icon">${icon('play')}</span>
                    <span class="choice__text"><strong>${t('menu.free')}</strong><span>${t('menu.freeSub')}</span></span>
                    ${icon('next', 'choice__arrow')}
                </a>
                <a class="choice choice--primary" href="#/lessons">
                    <span class="choice__icon">${icon('book')}</span>
                    <span class="choice__text"><strong>${t('menu.learn')}</strong><span>${t('menu.learnSub', { count: LESSONS.length })}${
                        earned ? ` ${t('menu.stars', { earned, total: LESSONS.length * 3 })}` : ''
                    }</span></span>
                    ${icon('next', 'choice__arrow')}
                </a>
            </nav>
            <button class="btn btn--ghost" data-devices>${icon('plug')}${t('menu.connect')}</button>
        </main>`);
    view.querySelector('[data-devices]').addEventListener('click', openDevices);
    ui.append(view);
    return () => view.remove();
}
