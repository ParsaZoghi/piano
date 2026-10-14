import './style.css';

const root = document.querySelector('#app');
const room = new URLSearchParams(location.search).get('remote');

// ?remote=CODE is the phone keyboard; everything else is the game itself.
// Loaded separately so the phone never downloads the 3D engine.
if (room) {
    import('./remote/phone.js').then(({ mountPhone }) => mountPhone(root, room));
} else {
    import('./app.js').then(({ startApp }) => startApp(root));
}
