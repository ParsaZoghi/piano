import { synth } from './audio/synth.js';
import { input } from './core/input.js';
import { settings } from './core/settings.js';
import { midi } from './input/midi.js';
import { typing } from './input/typing.js';
import { PianoScene } from './three/PianoScene.js';
import { openDevices } from './ui/devices.js';
import { mountFreePlay } from './ui/freePlay.js';
import { mountLessonList } from './ui/lessonList.js';
import { mountLesson } from './ui/lessonPlay.js';
import { mountMenu } from './ui/menu.js';

export function startApp(root) {
    root.innerHTML = `<canvas id="stage" aria-label="3D piano keyboard"></canvas><div id="ui"></div>`;
    const ui = root.querySelector('#ui');
    const scene = new PianoScene(root.querySelector('#stage'));

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

        // Move focus to the new screen's heading for keyboard and screen-reader users.
        ui.querySelector('h1')?.focus({ preventScroll: true });
    }

    window.addEventListener('hashchange', route);
    route();
}
