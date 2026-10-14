import os from 'node:os';
import { WebSocketServer } from 'ws';

const PATH = '/__remote';
const ROOM_RE = /^[A-Z0-9]{4,8}$/;

function lanAddresses() {
    const out = [];
    for (const list of Object.values(os.networkInterfaces())) {
        for (const net of list || []) {
            if (net.family === 'IPv4' && !net.internal) out.push(net.address);
        }
    }
    return out;
}

/**
 * Vite plugin: a tiny WebSocket relay so a phone on the same network can act as
 * a keyboard for the game. Phones join a room as "controller", the game joins
 * as "host"; note messages are forwarded controller -> host only.
 */
export function remoteRelay() {
    const rooms = new Map(); // code -> { hosts:Set, controllers:Set }
    const wss = new WebSocketServer({ noServer: true, maxPayload: 256 });

    const send = (ws, msg) => ws.readyState === 1 && ws.send(JSON.stringify(msg));

    function announce(room) {
        for (const h of room.hosts) send(h, { t: 'peers', n: room.controllers.size });
        for (const c of room.controllers) send(c, { t: 'host', on: room.hosts.size > 0 });
    }

    function onConnection(ws, code, role) {
        let room = rooms.get(code);
        if (!room) rooms.set(code, (room = { hosts: new Set(), controllers: new Set() }));
        const group = role === 'host' ? room.hosts : room.controllers;
        group.add(ws);
        announce(room);

        ws.on('message', (raw) => {
            if (role !== 'controller') return;
            let msg;
            try {
                msg = JSON.parse(raw.toString());
            } catch {
                return;
            }
            if ((msg.t !== 'on' && msg.t !== 'off') || !Number.isInteger(msg.n)) return;
            if (msg.n < 21 || msg.n > 108) return;
            const out = { t: msg.t, n: msg.n, v: Math.min(1, Math.max(0, Number(msg.v) || 0.8)) };
            for (const h of room.hosts) send(h, out);
        });

        ws.on('close', () => {
            group.delete(ws);
            if (!room.hosts.size && !room.controllers.size) rooms.delete(code);
            else announce(room);
        });
    }

    function attach(server) {
        server.httpServer?.on('upgrade', (req, socket, head) => {
            const url = new URL(req.url, 'http://localhost');
            if (url.pathname !== PATH) return; // leave Vite's own HMR socket alone
            const code = url.searchParams.get('room') || '';
            const role = url.searchParams.get('role');
            if (!ROOM_RE.test(code) || (role !== 'host' && role !== 'controller')) {
                socket.destroy();
                return;
            }
            wss.handleUpgrade(req, socket, head, (ws) => onConnection(ws, code, role));
        });

        server.middlewares.use(`${PATH}/info`, (_req, res) => {
            res.setHeader('Content-Type', 'application/json');
            res.end(JSON.stringify({ addresses: lanAddresses() }));
        });
    }

    return {
        name: 'keystage-remote-relay',
        configureServer: attach,
        configurePreviewServer: attach,
    };
}
