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
const NAMES = ['C', 'D', 'E', 'F', 'G', 'A', 'B', 'C', 'D', 'E'];

export function mountPhone(root, rawRoom) {
    const room = rawRoom.toUpperCase();
    document.body.classList.add('is-phone');

    const view = el(`
        <div class="phone">
            <header class="phone__bar">
                <strong class="phone__brand">${icon('music')}Keystage</strong>
                <span class="status" role="status"><span class="dot" data-dot></span><span data-status>Connecting…</span></span>
                <div class="stepper" role="group" aria-label="Octave">
                    <button class="icon-btn" data-octave="-1" aria-label="Octave down">${icon('minus')}</button>
                    <span class="stepper__value" data-octave-label>C4</span>
                    <button class="icon-btn" data-octave="1" aria-label="Octave up">${icon('plus')}</button>
                </div>
            </header>
            <div class="phone__keys" data-keys>
                ${WHITE.map((semi, i) => `<button class="pkey" data-semi="${semi}" aria-label="${NAMES[i]}"><span>${NAMES[i]}</span></button>`).join('')}
                ${BLACK.map(([semi, slot]) => `<button class="pkey pkey--black" data-semi="${semi}" style="left:${slot * 10}%" aria-label="Sharp"></button>`).join('')}
            </div>
            <p class="phone__tip">Turn your phone sideways for bigger keys. Sound plays on the computer.</p>
        </div>`);
    root.append(view);

    const statusText = view.querySelector('[data-status]');
    const dot = view.querySelector('[data-dot]');
    const setStatus = (text, on) => {
        statusText.textContent = text;
        dot.classList.toggle('dot--on', on);
    };

    if (!/^[A-Z0-9]{4,8}$/.test(room)) {
        setStatus('This link is not valid. Scan the code again.', false);
        return;
    }

    let socket = null;
    let octave = 4;
    const active = new Map(); // pointerId -> { midi, key }

    function send(type, midi) {
        if (socket?.readyState === WebSocket.OPEN) socket.send(JSON.stringify({ t: type, n: midi, v: 0.8 }));
    }

    function connect() {
        const proto = location.protocol === 'https:' ? 'wss' : 'ws';
        socket = new WebSocket(`${proto}://${location.host}/__remote?room=${room}&role=controller`);
        socket.onopen = () => setStatus('Connected. Open the game on your computer.', false);
        socket.onmessage = (e) => {
            try {
                const msg = JSON.parse(e.data);
                if (msg.t === 'host') setStatus(msg.on ? 'Connected. Play!' : 'Waiting for the game on your computer…', msg.on);
            } catch {
                // Ignore anything that is not a relay message.
            }
        };
        socket.onclose = () => {
            setStatus('Reconnecting…', false);
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
        view.querySelector('[data-octave-label]').textContent = `C${octave}`;
    });

    document.addEventListener('visibilitychange', () => document.hidden && releaseAll());
    connect();
}
