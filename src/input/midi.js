import { input } from '../core/input.js';
import { synth } from '../audio/synth.js';

const listeners = new Set();
const notify = () => listeners.forEach((fn) => fn());

function onMessage(e) {
    const [status, d1, d2 = 0] = e.data;
    const command = status & 0xf0;
    if (command === 0x90 && d2 > 0) input.noteOn(d1, Math.max(0.25, d2 / 127), 'midi');
    else if (command === 0x80 || (command === 0x90 && d2 === 0)) input.noteOff(d1, 'midi');
    else if (command === 0xb0 && d1 === 64) synth.setSustain(d2 >= 64); // sustain pedal
}

export const midi = {
    supported: typeof navigator !== 'undefined' && 'requestMIDIAccess' in navigator,
    status: 'idle', // idle | pending | ready | denied | unsupported
    devices: [],

    onChange(fn) {
        listeners.add(fn);
        return () => listeners.delete(fn);
    },

    /** Re-attach silently when the browser already has permission. */
    async autoEnable() {
        if (!this.supported || !navigator.permissions) return;
        try {
            const perm = await navigator.permissions.query({ name: 'midi' });
            if (perm.state === 'granted') await this.enable();
        } catch {
            // Browser does not expose the "midi" permission name; wait for a click.
        }
    },

    async enable() {
        if (!this.supported) {
            this.status = 'unsupported';
            return notify();
        }
        if (this.status === 'ready' || this.status === 'pending') return;
        this.status = 'pending';
        notify();
        try {
            const access = await navigator.requestMIDIAccess();
            const refresh = () => {
                this.devices = [];
                for (const port of access.inputs.values()) {
                    port.onmidimessage = onMessage;
                    if (port.state === 'connected') this.devices.push(port.name || 'MIDI device');
                }
                input.releaseAll('midi');
                notify();
            };
            access.onstatechange = refresh;
            this.status = 'ready';
            refresh();
        } catch {
            this.status = 'denied';
            notify();
        }
    },
};
