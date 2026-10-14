import { midi } from '../input/midi.js';
import { remoteHost } from '../input/remoteHost.js';
import { el, escapeHtml, icon } from './dom.js';

let dialog = null;

function midiSection() {
    if (!midi.supported) return `<p class="muted">This browser does not support Web MIDI. Try Chrome, Edge or Firefox on a computer.</p>`;
    if (midi.status === 'denied') return `<p class="muted">MIDI access was blocked. Allow it in the site settings of your browser, then reload.</p>`;
    if (midi.status !== 'ready') {
        return `<p class="muted">Plug in a USB or Bluetooth MIDI keyboard, then allow access.</p>
            <button class="btn" data-midi ${midi.status === 'pending' ? 'disabled' : ''}>${midi.status === 'pending' ? 'Waiting for permission…' : 'Enable MIDI'}</button>`;
    }
    if (!midi.devices.length) return `<p class="status"><span class="dot"></span>MIDI is on. No keyboard detected yet: plug one in and it will appear here.</p>`;
    return `<ul class="device-list">${midi.devices.map((name) => `<li><span class="dot dot--on"></span>${escapeHtml(name)}</li>`).join('')}</ul>`;
}

function phoneSection() {
    switch (remoteHost.status) {
        case 'off':
            return `<p class="muted">Turn a phone on the same Wi-Fi into a touch keyboard. No app needed.</p>
                <button class="btn" data-phone>Show pairing code</button>`;
        case 'unavailable':
            return `<p class="muted">Phone pairing needs the game's own server. Start it with <code>npm run dev</code> or <code>npm run preview</code> and make sure the computer is on a network.</p>`;
        case 'connecting':
            return `<p class="status"><span class="dot"></span>Connecting…</p>`;
        default:
            return `<div class="pair">
                    <canvas class="pair__qr" width="168" height="168" aria-label="QR code for the phone keyboard link"></canvas>
                    <div>
                        <p>Scan with your phone camera, or open this address in the phone's browser:</p>
                        <p class="pair__url">${escapeHtml(remoteHost.url)}</p>
                        <p class="status"><span class="dot ${remoteHost.peers ? 'dot--on' : ''}"></span>${
                            remoteHost.peers ? `${remoteHost.peers} phone${remoteHost.peers > 1 ? 's' : ''} connected` : 'Waiting for a phone…'
                        }</p>
                    </div>
                </div>`;
    }
}

async function drawQr() {
    const canvas = dialog.querySelector('.pair__qr');
    if (!canvas) return;
    const { default: QRCode } = await import('qrcode');
    QRCode.toCanvas(canvas, remoteHost.url, { width: 168, margin: 1, color: { dark: '#0f0f23', light: '#ffffff' } });
}

function render() {
    if (!dialog?.open) return;
    dialog.querySelector('[data-midi-section]').innerHTML = midiSection();
    dialog.querySelector('[data-phone-section]').innerHTML = phoneSection();
    drawQr();
}

export function openDevices() {
    if (!dialog) {
        dialog = el(`
            <dialog class="dialog" aria-labelledby="devices-title">
                <header class="dialog__head">
                    <h2 id="devices-title">Ways to play</h2>
                    <button class="icon-btn" data-close aria-label="Close">${icon('close')}</button>
                </header>
                <section class="device">
                    <h3>${icon('keyboard')}Typing keyboard</h3>
                    <p class="muted">Always on. The home row plays the white keys and the row above plays the black keys. In free play, <kbd>Z</kbd> and <kbd>X</kbd> move down and up an octave and <kbd>Space</kbd> is the sustain pedal.</p>
                    <div class="keymap" aria-hidden="true">
                        <div class="keymap__row keymap__row--top"><kbd>W</kbd><kbd>E</kbd><i></i><kbd>T</kbd><kbd>Y</kbd><kbd>U</kbd><i></i><kbd>O</kbd><kbd>P</kbd></div>
                        <div class="keymap__row"><kbd>A</kbd><kbd>S</kbd><kbd>D</kbd><kbd>F</kbd><kbd>G</kbd><kbd>H</kbd><kbd>J</kbd><kbd>K</kbd><kbd>L</kbd><kbd>;</kbd></div>
                        <div class="keymap__row keymap__row--names"><span>C</span><span>D</span><span>E</span><span>F</span><span>G</span><span>A</span><span>B</span><span>C</span><span>D</span><span>E</span></div>
                    </div>
                </section>
                <section class="device">
                    <h3>${icon('plug')}MIDI keyboard</h3>
                    <div data-midi-section></div>
                </section>
                <section class="device">
                    <h3>${icon('phone')}Phone as a keyboard</h3>
                    <div data-phone-section></div>
                </section>
            </dialog>`);
        document.body.append(dialog);
        dialog.addEventListener('click', (e) => {
            if (e.target === dialog || e.target.closest('[data-close]')) dialog.close();
            else if (e.target.closest('[data-midi]')) midi.enable();
            else if (e.target.closest('[data-phone]')) remoteHost.start();
        });
        midi.onChange(render);
        remoteHost.onChange(render);
    }
    dialog.showModal();
    render();
}
