import { t } from '../core/i18n.js';
import { PIANO_MAX, PIANO_MIN, pitchClassName } from '../core/notes.js';
import { onSettingsChange, settings } from '../core/settings.js';
import { typing } from '../input/typing.js';
import { el, icon } from './dom.js';

export function mountFreePlay({ scene, ui, openDevices }) {
    scene.configure({ lo: PIANO_MIN, hi: PIANO_MAX, view: 'free' });
    typing.configure({ octaveShift: true, sustain: true });

    const kbd = (...keys) => `<bdi dir="ltr">${keys.map((k) => `<kbd>${k}</kbd>`).join(' ')}</bdi>`;
    const view = el(`
        <div class="hud">
            <header class="topbar">
                <a class="icon-btn" href="#/" aria-label="${t('common.backMenu')}">${icon('back')}</a>
                <div class="topbar__title"><span class="eyebrow">${t('free.sub')}</span><h1 tabindex="-1">${t('free.title')}</h1></div>
                <div class="topbar__tools">
                    <div class="stepper" role="group" aria-label="${t('free.octave')}">
                        <button class="icon-btn" data-octave="-1" aria-label="${t('free.octaveDown')}">${icon('minus')}</button>
                        <span class="stepper__value" data-octave-label aria-live="polite" dir="ltr"></span>
                        <button class="icon-btn" data-octave="1" aria-label="${t('free.octaveUp')}">${icon('plus')}</button>
                    </div>
                    <button class="btn btn--ghost" data-devices>${icon('plug')}<span>${t('common.devices')}</span></button>
                </div>
            </header>
            <p class="hint-bar">
                ${kbd('A')}–${kbd(';')} ${t('free.white')} · ${kbd('W', 'E', 'T', 'Y', 'U')} ${t('free.black')} · ${kbd('Z', 'X')} ${t('free.octaveKeys')} · ${kbd('Space')} ${t('free.sustain')} · ${t('free.other')}
            </p>
        </div>`);
    ui.append(view);

    const octaveLabel = view.querySelector('[data-octave-label]');

    function refresh() {
        octaveLabel.textContent = `${pitchClassName('C')}${typing.baseOctave}`;
        scene.keyboard.setLabels({ names: settings.labels, typingMap: settings.labels ? typing.labelMap() : null });
    }
    const offTyping = typing.onChange(refresh);
    const offSettings = onSettingsChange((patch) => 'labels' in patch && refresh());
    refresh();

    view.addEventListener('click', (e) => {
        const octave = e.target.closest('[data-octave]');
        if (octave) typing.setShift(typing.shift + Number(octave.dataset.octave));
        else if (e.target.closest('[data-devices]')) openDevices();
    });

    return () => {
        offTyping();
        offSettings();
        view.remove();
    };
}
