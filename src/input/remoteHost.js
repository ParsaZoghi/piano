import { input } from '../core/input.js';

// Game side of the phone keyboard: joins a relay room as "host" and turns the
// phone's messages into ordinary note events.
const listeners = new Set();
const notify = () => listeners.forEach((fn) => fn());

let socket = null;
let wanted = false;
let retryTimer = 0;

function roomCode() {
    let code = sessionStorage.getItem('keystage.room');
    if (!code) {
        const alphabet = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
        const bytes = crypto.getRandomValues(new Uint8Array(5));
        code = [...bytes].map((b) => alphabet[b % alphabet.length]).join('');
        sessionStorage.setItem('keystage.room', code);
    }
    return code;
}

function connect() {
    const proto = location.protocol === 'https:' ? 'wss' : 'ws';
    socket = new WebSocket(`${proto}://${location.host}/__remote?room=${remoteHost.room}&role=host`);
    socket.onopen = () => {
        remoteHost.status = 'ready';
        notify();
    };
    socket.onmessage = (e) => {
        let msg;
        try {
            msg = JSON.parse(e.data);
        } catch {
            return;
        }
        if (msg.t === 'on') input.noteOn(msg.n, msg.v, 'remote');
        else if (msg.t === 'off') input.noteOff(msg.n, 'remote');
        else if (msg.t === 'peers') {
            remoteHost.peers = msg.n;
            if (msg.n === 0) input.releaseAll('remote');
            notify();
        }
    };
    socket.onclose = () => {
        socket = null;
        remoteHost.peers = 0;
        input.releaseAll('remote');
        if (wanted) {
            remoteHost.status = 'connecting';
            retryTimer = setTimeout(connect, 2000);
        }
        notify();
    };
}

export const remoteHost = {
    status: 'off', // off | connecting | ready | unavailable
    peers: 0,
    room: '',
    url: '',

    onChange(fn) {
        listeners.add(fn);
        return () => listeners.delete(fn);
    },

    async start() {
        if (wanted) return;
        this.status = 'connecting';
        notify();
        let info;
        try {
            const res = await fetch('/__remote/info');
            info = await res.json();
        } catch {
            // No relay: the page is served by a static host, not by Vite.
            this.status = 'unavailable';
            return notify();
        }
        const local = /^(localhost|127\.0\.0\.1|\[::1\])$/.test(location.hostname);
        const host = local ? info.addresses?.[0] : location.hostname;
        if (!host) {
            this.status = 'unavailable';
            return notify();
        }
        this.room = roomCode();
        const port = location.port ? `:${location.port}` : '';
        this.url = `${location.protocol}//${host}${port}/?remote=${this.room}`;
        wanted = true;
        connect();
    },

    stop() {
        wanted = false;
        clearTimeout(retryTimer);
        socket?.close();
        this.status = 'off';
        notify();
    },
};
