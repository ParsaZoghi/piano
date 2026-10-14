import { t } from '../core/i18n.js';
import { pitchClassName } from '../core/notes.js';
import { el, icon } from '../ui/dom.js';

// The phone keyboard: a touch piano that sends note messages to the game
// through the relay. The sound comes from the computer, not the phone.
const WHITE = [0, 2, 4, 5, 7, 9, 11, 12, 14, 16];
const BLACK = [
    [1, 1],
    [3, 2],
    [6, 4],
    [8, 5],
    [10, 6],
    [13, 8],
    [15, 9],
]; // [semitone, index of the white key it follows + 1]
const LETTERS = ['C', 'D', 'E', 'F', 'G', 'A', 'B', 'C', 'D', 'E'];

export function mountPhone(root, rawRoom) {
    const room = rawRoom.toUpperCase();
    document.body.classList.add('is-phone');

    const view = el(`
        <div class="phone">
            <header class="phone__bar">
                <strong class="phone__brand" lang="en">${icon('music')}Keystage</strong>
                <span class="status" role="status"><span class="dot" data-dot></span><span data-status>${t('phone.connecting')}</span></span>
                <div class="stepper" role="group" aria-label="${t('phone.octave')}" dir="ltr">
                    <button class="icon-btn" data-octave="-1" aria-label="${t('phone.octaveDown')}">${icon('minus')}</button>
                    <span class="stepper__value" data-octave-label></span>
                    <button class="icon-btn" data-octave="1" aria-label="${t('phone.octaveUp')}">${icon('plus')}</button>
                </div>
            </header>
            <div class="phone__keys" data-keys dir="ltr">
                ${WHITE.map((semi, i) => `<button class="pkey" data-semi="${semi}" aria-label="${pitchClassName(LETTERS[i])}"><span>${pitchClassName(LETTERS[i])}</span></button>`).join('')}
                ${BLACK.map(([semi, slot]) => `<button class="pkey pkey--black" data-semi="${semi}" style="left:${slot * 10}%" aria-label="♯"></button>`).join('')}
            </div>
            <p class="phone__tip">${t('phone.tip')}</p>
        </div>`);
    root.append(view);

    const statusText = view.querySelector('[data-status]');
    const dot = view.querySelector('[data-dot]');
    const setStatus = (key, on) => {
        statusText.textContent = t(`phone.${key}`);
        dot.classList.toggle('dot--on', on);
    };

    if (!/^[A-Z0-9]{4,8}$/.test(room)) {
        setStatus('invalid', false);
        return;
    }

    let socket = null;
    let octave = 4;
    const active = new Map(); // pointerId -> { midi, key }
    const octaveLabel = view.querySelector('[data-octave-label]');
    octaveLabel.textContent = `${pitchClassName('C')}${octave}`;

    function send(type, midi) {
        if (socket?.readyState === WebSocket.OPEN) socket.send(JSON.stringify({ t: type, n: midi, v: 0.8 }));
    }

    function connect() {
        const proto = location.protocol === 'https:' ? 'wss' : 'ws';
        socket = new WebSocket(`${proto}://${location.host}/__remote?room=${room}&role=controller`);
        socket.onopen = () => setStatus('connected', false);
        socket.onmessage = (e) => {
            try {
                const msg = JSON.parse(e.data);
                if (msg.t === 'host') setStatus(msg.on ? 'play' : 'waitingHost', msg.on);
            } catch {
                // Ignore anything that is not a relay message.
            }
        };
        socket.onclose = () => {
            setStatus('reconnecting', false);
            releaseAll();
            setTimeout(connect, 1500);
        };
    }

    function release(pointerId) {
        const held = active.get(pointerId);
        if (!held) return;
        held.key.classList.remove('pkey--down');
        send('off', held.midi);
        active.delete(pointerId);
    }

    function releaseAll() {
        for (const id of [...active.keys()]) release(id);
    }

    function press(pointerId, key) {
        const current = active.get(pointerId);
        if (current?.key === key) return;
        release(pointerId);
        if (!key) return;
        const midi = (octave + 1) * 12 + Number(key.dataset.semi);
        key.classList.add('pkey--down');
        active.set(pointerId, { midi, key });
        send('on', midi);
        navigator.vibrate?.(8);
    }

    const keys = view.querySelector('[data-keys]');
    const keyUnder = (e) => document.elementFromPoint(e.clientX, e.clientY)?.closest('.pkey') || null;
    keys.addEventListener('pointerdown', (e) => {
        e.preventDefault();
        press(e.pointerId, keyUnder(e));
    });
    // Sliding a finger across the keys plays each one.
    keys.addEventListener('pointermove', (e) => {
        if (active.has(e.pointerId)) press(e.pointerId, keyUnder(e));
    });
    for (const type of ['pointerup', 'pointercancel', 'pointerleave']) keys.addEventListener(type, (e) => release(e.pointerId));
    keys.addEventListener('contextmenu', (e) => e.preventDefault());

    view.querySelector('.stepper').addEventListener('click', (e) => {
        const button = e.target.closest('[data-octave]');
        if (!button) return;
        releaseAll();
        octave = Math.max(1, Math.min(6, octave + Number(button.dataset.octave)));
        octaveLabel.textContent = `${pitchClassName('C')}${octave}`;
    });

    document.addEventListener('visibilitychange', () => document.hidden && releaseAll());
    connect();
}
