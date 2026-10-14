import './style.css';
import { setLanguage } from './core/i18n.js';
import { setNaming } from './core/notes.js';
import { onSettingsChange, settings } from './core/settings.js';

const root = document.querySelector('#app');
const params = new URLSearchParams(location.search);
const room = params.get('remote');

// The phone keyboard takes its look and language from the link it was given.
const prefs = room
    ? { theme: params.get('theme') || settings.theme, lang: params.get('lang') || settings.lang, naming: params.get('names') || settings.naming }
    : settings;

function applyPreferences(p) {
    document.documentElement.dataset.theme = p.theme;
    document.querySelector('meta[name="theme-color"]')?.setAttribute('content', p.theme === 'light' ? '#e6eaf5' : '#0f0f23');
    setLanguage(p.lang);
    setNaming(p.naming);
}
applyPreferences(prefs);
if (!room) onSettingsChange(() => applyPreferences(settings));

// ?remote=CODE is the phone keyboard; everything else is the game itself.
// Loaded separately so the phone never downloads the 3D engine.
if (room) {
    import('./remote/phone.js').then(({ mountPhone }) => mountPhone(root, room));
} else {
    import('./app.js').then(({ startApp }) => startApp(root));
}
