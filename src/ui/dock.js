import { LANGUAGES, t } from '../core/i18n.js';
import { saveSettings, settings } from '../core/settings.js';
import { el, icon } from './dom.js';

// The control cluster shown on every page: theme toggle, camera controls and
// a settings menu. Lives outside the screens so it survives navigation.
export function createDock({ scene, openDevices }) {
    const dock = el('<div class="dock"></div>');
    let open = null; // id of the open popover

    const segmented = (key, options, label) =>
        `<div class="field"><span class="field__label" id="lbl-${key}">${label}</span>
            <div class="segmented" role="group" aria-labelledby="lbl-${key}">${options
                .map(([value, text, lang]) => `<button data-set="${key}" data-value="${value}" aria-pressed="${settings[key] === value}" ${lang ? `lang="${lang}"` : ''}>${text}</button>`)
                .join('')}</div></div>`;

    function render() {
        const dark = settings.theme === 'dark';
        dock.innerHTML = `
            <button class="icon-btn" data-theme-toggle aria-label="${dark ? t('dock.toLight') : t('dock.toDark')}" title="${dark ? t('dock.toLight') : t('dock.toDark')}">${icon(dark ? 'sun' : 'moon')}</button>
            <div class="popwrap" data-camera-wrap ${scene.interactive ? '' : 'hidden'}>
                <button class="icon-btn" data-pop="camera" aria-expanded="${open === 'camera'}" aria-controls="pop-camera" aria-label="${t('dock.camera')}" title="${t('dock.camera')}">${icon('move')}</button>
                <div class="popover popover--camera" id="pop-camera" ${open === 'camera' ? '' : 'hidden'}>
                    <span class="popover__title">${t('camera.title')}</span>
                    <div class="dpad" dir="ltr">
                        <button class="icon-btn" data-cam="up" aria-label="${t('camera.up')}">${icon('up')}</button>
                        <button class="icon-btn" data-cam="left" aria-label="${t('camera.left')}">${icon('left')}</button>
                        <button class="icon-btn" data-cam="reset" aria-label="${t('camera.reset')}" title="${t('camera.reset')} (0)">${icon('home')}</button>
                        <button class="icon-btn" data-cam="right" aria-label="${t('camera.right')}">${icon('right')}</button>
                        <button class="icon-btn" data-cam="down" aria-label="${t('camera.down')}">${icon('down')}</button>
                    </div>
                    <div class="zoom-row">
                        <button class="btn btn--ghost" data-cam="out" aria-label="${t('camera.zoomOut')}">${icon('zoomOut')}</button>
                        <button class="btn btn--ghost" data-cam="in" aria-label="${t('camera.zoomIn')}">${icon('zoomIn')}</button>
                    </div>
                    <p class="popover__hint">${t('camera.hint')}</p>
                </div>
            </div>
            <div class="popwrap">
                <button class="icon-btn" data-pop="settings" aria-expanded="${open === 'settings'}" aria-controls="pop-settings" aria-label="${t('dock.settings')}" title="${t('dock.settings')}">${icon('gear')}</button>
                <div class="popover" id="pop-settings" ${open === 'settings' ? '' : 'hidden'}>
                    <span class="popover__title">${t('settings.title')}</span>
                    ${segmented(
                        'lang',
                        LANGUAGES.map((l) => [l.id, l.label, l.id]),
                        t('settings.language'),
                    )}
                    ${segmented(
                        'naming',
                        [
                            ['letters', 'C D E', 'en'],
                            ['solfege', 'Do Re Mi', 'en'],
                        ],
                        t('settings.noteNames'),
                    )}
                    ${segmented(
                        'theme',
                        [
                            ['dark', t('settings.dark')],
                            ['light', t('settings.light')],
                        ],
                        t('settings.theme'),
                    )}
                    <div class="field field--row">
                        <span class="field__label" id="lbl-labels">${t('settings.labels')}</span>
                        <button class="switch" role="switch" data-toggle-setting="labels" aria-checked="${settings.labels}" aria-labelledby="lbl-labels"><span></span></button>
                    </div>
                    <label class="field">
                        <span class="field__label">${t('settings.volume')}</span>
                        <span class="slider">${icon('volume')}<input type="range" min="0" max="1" step="0.05" value="${settings.volume}" data-volume /></span>
                    </label>
                    <button class="btn btn--ghost" data-devices>${icon('plug')}${t('settings.devices')}</button>
                </div>
            </div>`;
    }

    function setOpen(id) {
        open = open === id ? null : id;
        render();
        if (open) dock.querySelector(`#pop-${open}`)?.querySelector('button, input')?.focus();
    }

    const camera = {
        up: () => scene.step(0, 1),
        down: () => scene.step(0, -1),
        left: () => scene.step(-1, 0),
        right: () => scene.step(1, 0),
        in: () => scene.zoomBy(1 / 1.2),
        out: () => scene.zoomBy(1.2),
        reset: () => scene.resetCamera(),
    };

    dock.addEventListener('click', (e) => {
        const target = e.target.closest('button');
        if (!target) return;
        if (target.matches('[data-theme-toggle]')) saveSettings({ theme: settings.theme === 'dark' ? 'light' : 'dark' });
        else if (target.dataset.pop) setOpen(target.dataset.pop);
        else if (target.dataset.cam) camera[target.dataset.cam]();
        else if (target.dataset.set) saveSettings({ [target.dataset.set]: target.dataset.value });
        else if (target.dataset.toggleSetting) saveSettings({ [target.dataset.toggleSetting]: !settings[target.dataset.toggleSetting] });
        else if (target.matches('[data-devices]')) {
            open = null;
            render();
            openDevices();
        }
    });
    dock.addEventListener('input', (e) => {
        if (e.target.matches('[data-volume]')) saveSettings({ volume: Number(e.target.value) });
    });

    // Close a popover on an outside click or Escape.
    document.addEventListener('pointerdown', (e) => {
        if (open && !dock.contains(e.target)) {
            open = null;
            render();
        }
    });
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && open) {
            const button = dock.querySelector(`[data-pop="${open}"]`);
            open = null;
            render();
            dock.querySelector(`[data-pop="${button?.dataset.pop}"]`)?.focus();
            e.stopImmediatePropagation();
        }
    });

    render();
    return {
        element: dock,
        render,
        /** Re-render without stealing focus from the volume slider while dragging. */
        refresh(patch) {
            if ('volume' in patch && Object.keys(patch).length === 1) return;
            render();
        },
        camera,
    };
}
