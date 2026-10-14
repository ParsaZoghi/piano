import { synth } from './audio/synth.js';
import { input } from './core/input.js';
import { onSettingsChange, settings } from './core/settings.js';
import { midi } from './input/midi.js';
import { typing } from './input/typing.js';
import { PianoScene } from './three/PianoScene.js';
import { openDevices } from './ui/devices.js';
import { createDock } from './ui/dock.js';
import { mountFreePlay } from './ui/freePlay.js';
import { mountLessonList } from './ui/lessonList.js';
import { mountLesson } from './ui/lessonPlay.js';
import { mountMenu } from './ui/menu.js';

// Camera keys. Letters are taken by the piano, so these use arrows, +, - and 0.
const CAMERA_KEYS = {
    ArrowUp: 'up',
    ArrowDown: 'down',
    ArrowLeft: 'left',
    ArrowRight: 'right',
    Equal: 'in',
    NumpadAdd: 'in',
    Minus: 'out',
    NumpadSubtract: 'out',
    Digit0: 'reset',
    Numpad0: 'reset',
};

export function startApp(root) {
    root.innerHTML = `<canvas id="stage" aria-label="3D piano keyboard"></canvas><div id="ui"></div>`;
    const ui = root.querySelector('#ui');
    const scene = new PianoScene(root.querySelector('#stage'));
    scene.setTheme(settings.theme);

    // Every input source ends up here: make the sound and move the 3D key.
    input.on('noteon', ({ midi: note, velocity }) => {
        synth.noteOn(note, velocity);
        scene.keyboard.press(note, true);
    });
    input.on('noteoff', ({ midi: note }) => {
        synth.noteOff(note);
        scene.keyboard.press(note, false);
    });

    synth.setVolume(settings.volume);
    typing.init();
    midi.autoEnable();

    const navigate = (path) => {
        location.hash = `#${path}`;
    };
    const context = { scene, ui, navigate, openDevices };
    const dock = createDock({ scene, openDevices });
    root.append(dock.element);
    let unmount = null;

    function route() {
        unmount?.();
        input.releaseAll();
        synth.allOff();

        const [, page = '', param] = location.hash.replace(/^#/, '').split('/');
        if (page === 'free') unmount = mountFreePlay(context);
        else if (page === 'lessons') unmount = mountLessonList(context);
        else if (page === 'lesson') unmount = mountLesson(context, param);
        else unmount = mountMenu(context);

        dock.render(); // the camera button only shows where the camera can move
        // Move focus to the new screen's heading for keyboard and screen-reader users.
        ui.querySelector('h1')?.focus({ preventScroll: true });
    }

    onSettingsChange((patch) => {
        if ('theme' in patch) scene.setTheme(settings.theme);
        if ('volume' in patch) synth.setVolume(settings.volume);
        // Language and note names are baked into every screen's text: rebuild it.
        if ('lang' in patch || 'naming' in patch) route();
        else dock.refresh(patch);
    });

    window.addEventListener('keydown', (e) => {
        const action = CAMERA_KEYS[e.code];
        if (!action || e.ctrlKey || e.metaKey || e.altKey || !scene.interactive) return;
        if (e.target instanceof HTMLElement && e.target.closest('input, textarea, select, dialog')) return;
        e.preventDefault();
        dock.camera[action]();
    });

    window.addEventListener('hashchange', route);
    route();
}
