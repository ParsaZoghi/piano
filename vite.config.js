import { defineConfig } from 'vite';
import { remoteRelay } from './server/remoteRelay.js';

export default defineConfig({
    plugins: [remoteRelay()],
    // host: true exposes the server on the local network so a phone can join.
    server: { host: true },
    preview: { host: true },
    base: '/piano/',
});
