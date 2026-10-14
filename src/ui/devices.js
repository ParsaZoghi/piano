import { t } from '../core/i18n.js';
import { pitchClassName } from '../core/notes.js';
import { midi } from '../input/midi.js';
import { remoteHost } from '../input/remoteHost.js';
import { el, escapeHtml, icon } from './dom.js';

let dialog = null;

function midiSection() {
    if (!midi.supported) return `<p class="muted">${t('devices.midiUnsupported')}</p>`;
    if (midi.status === 'denied') return `<p class="muted">${t('devices.midiDenied')}</p>`;
    if (midi.status !== 'ready') {
        return `<p class="muted">${t('devices.midiPlug')}</p>
            <button class="btn" data-midi ${midi.status === 'pending' ? 'disabled' : ''}>${midi.status === 'pending' ? t('devices.midiPending') : t('devices.midiEnable')}</button>`;
    }
    if (!midi.devices.length) return `<p class="status"><span class="dot"></span>${t('devices.midiNone')}</p>`;
    return `<ul class="device-list">${midi.devices.map((name) => `<li><span class="dot dot--on"></span><bdi>${escapeHtml(name)}</bdi></li>`).join('')}</ul>`;
}

function phoneSection() {
    switch (remoteHost.status) {
        case 'off':
            return `<p class="muted">${t('devices.phoneIntro')}</p>
                <button class="btn" data-phone>${t('devices.phoneShow')}</button>`;
        case 'unavailable':
            return `<p class="muted">${t('devices.phoneUnavailable', { dev: '<code dir="ltr">npm run dev</code>', preview: '<code dir="ltr">npm run preview</code>' })}</p>`;
        case 'connecting':
            return `<p class="status"><span class="dot"></span>${t('devices.phoneConnecting')}</p>`;
        default:
            return `<div class="pair">
                    <canvas class="pair__qr" width="168" height="168" aria-label="${t('devices.qr')}"></canvas>
                    <div>
                        <p>${t('devices.phoneScan')}</p>
                        <p class="pair__url" dir="ltr">${escapeHtml(remoteHost.url)}</p>
                        <p class="status"><span class="dot ${remoteHost.peers ? 'dot--on' : ''}"></span>${
                            remoteHost.peers ? t('devices.phoneConnected', { n: remoteHost.peers }) : t('devices.phoneWaiting')
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

function renderSections() {
    if (!dialog?.open) return;
    dialog.querySelector('[data-midi-section]').innerHTML = midiSection();
    dialog.querySelector('[data-phone-section]').innerHTML = phoneSection();
    drawQr();
}

function build() {
    dialog.innerHTML = `
        <header class="dialog__head">
            <h2 id="devices-title">${t('devices.title')}</h2>
            <button class="icon-btn" data-close aria-label="${t('common.close')}">${icon('close')}</button>
        </header>
        <section class="device">
            <h3>${icon('keyboard')}${t('devices.typing')}</h3>
            <p class="muted">${t('devices.typingText', { z: '<kbd>Z</kbd>', x: '<kbd>X</kbd>', space: '<kbd>Space</kbd>' })}</p>
            <div class="keymap" dir="ltr" aria-hidden="true">
                <div class="keymap__row keymap__row--top"><kbd>W</kbd><kbd>E</kbd><i></i><kbd>T</kbd><kbd>Y</kbd><kbd>U</kbd><i></i><kbd>O</kbd><kbd>P</kbd></div>
                <div class="keymap__row"><kbd>A</kbd><kbd>S</kbd><kbd>D</kbd><kbd>F</kbd><kbd>G</kbd><kbd>H</kbd><kbd>J</kbd><kbd>K</kbd><kbd>L</kbd><kbd>;</kbd></div>
                <div class="keymap__row keymap__row--names">${['C', 'D', 'E', 'F', 'G', 'A', 'B', 'C', 'D', 'E'].map((n) => `<span>${pitchClassName(n)}</span>`).join('')}</div>
            </div>
        </section>
        <section class="device">
            <h3>${icon('plug')}${t('devices.midi')}</h3>
            <div data-midi-section></div>
        </section>
        <section class="device">
            <h3>${icon('phone')}${t('devices.phone')}</h3>
            <div data-phone-section></div>
        </section>`;
}

export function openDevices() {
    if (!dialog) {
        dialog = el(`<dialog class="dialog" aria-labelledby="devices-title"></dialog>`);
        document.body.append(dialog);
        dialog.addEventListener('click', (e) => {
            if (e.target === dialog || e.target.closest('[data-close]')) dialog.close();
            else if (e.target.closest('[data-midi]')) midi.enable();
            else if (e.target.closest('[data-phone]')) remoteHost.start();
        });
        midi.onChange(renderSections);
        remoteHost.onChange(renderSections);
    }
    build(); // rebuilt each time so it follows the current language
    dialog.showModal();
    renderSections();
}
