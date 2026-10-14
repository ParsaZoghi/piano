import { synth } from '../audio/synth.js';
import { PIANO_MAX, PIANO_MIN } from '../core/notes.js';
import { saveSettings, settings } from '../core/settings.js';
import { typing } from '../input/typing.js';
import { el, icon } from './dom.js';

export function mountFreePlay({ scene, ui, openDevices }) {
    scene.configure({ lo: PIANO_MIN, hi: PIANO_MAX, view: 'free' });
    typing.configure({ octaveShift: true, sustain: true });

    const view = el(`
        <div class="hud">
            <header class="topbar">
                <a class="icon-btn" href="#/" aria-label="Back to menu">${icon('back')}</a>
                <div class="topbar__title"><span class="eyebrow">88 keys · A0 to C8</span><h1 tabindex="-1">Free play</h1></div>
                <div class="topbar__tools">
                    <div class="stepper" role="group" aria-label="Typing keyboard octave">
                        <button class="icon-btn" data-octave="-1" aria-label="Octave down (Z)">${icon('minus')}</button>
                        <span class="stepper__value" data-octave-label aria-live="polite"></span>
                        <button class="icon-btn" data-octave="1" aria-label="Octave up (X)">${icon('plus')}</button>
                    </div>
                    <label class="slider">${icon('volume')}<span class="sr-only">Volume</span>
                        <input type="range" min="0" max="1" step="0.05" value="${settings.volume}" data-volume />
                    </label>
                    <button class="chip" data-labels aria-pressed="${settings.labels}">Key labels</button>
                    <button class="btn btn--ghost" data-devices>${icon('plug')}<span>Devices</span></button>
                </div>
            </header>
            <p class="hint-bar">
                <kbd>A</kbd>–<kbd>;</kbd> white keys · <kbd>W</kbd> <kbd>E</kbd> <kbd>T</kbd> <kbd>Y</kbd> <kbd>U</kbd> black keys · <kbd>Z</kbd> <kbd>X</kbd> octave · <kbd>Space</kbd> sustain · or click, touch, MIDI or phone
            </p>
        </div>`);
    ui.append(view);

    const octaveLabel = view.querySelector('[data-octave-label]');
    const labelsButton = view.querySelector('[data-labels]');

    function refresh() {
        octaveLabel.textContent = `C${typing.baseOctave}`;
        scene.keyboard.setLabels({ names: settings.labels, typingMap: settings.labels ? typing.labelMap() : null });
        labelsButton.setAttribute('aria-pressed', settings.labels);
    }
    const offTyping = typing.onChange(refresh);
    refresh();

    view.addEventListener('click', (e) => {
        const octave = e.target.closest('[data-octave]');
        if (octave) typing.setShift(typing.shift + Number(octave.dataset.octave));
        else if (e.target.closest('[data-labels]')) {
            saveSettings({ labels: !settings.labels });
            refresh();
        } else if (e.target.closest('[data-devices]')) openDevices();
    });
    view.querySelector('[data-volume]').addEventListener('input', (e) => {
        saveSettings({ volume: Number(e.target.value) });
        synth.setVolume(settings.volume);
    });

    return () => {
        offTyping();
        view.remove();
    };
}
