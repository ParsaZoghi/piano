# Keystage for Unity

This branch (`unity`) is a Unity 6 port of the Keystage web game on the `main`
branch: a 3D piano that teaches beginners notes and rhythm with the computer
keyboard, a MIDI keyboard or a phone. Same menu, free play (88 keys), 38
lessons, themes, camera controls, English/Persian and C D E / Do Re Mi.

Everything here is free: Unity Personal, the open-source **MCP for Unity**
bridge (MIT), `uv`, and open-licence fonts. No Unity subscription and no
Unity AI/MCP product is used.

## 1. Open the project (first time)

1. Open **Unity Hub**. Sign in if it asks; the free **Personal** licence is all you need.
2. Go to **Projects → Add → Add project from disk** and pick this folder
   (the one with `Assets`, `Packages` and `ProjectSettings` in it).
3. Open it with **Unity 6000.6.1f1**. The first open takes a few minutes: Unity
   imports everything and downloads the MCP for Unity package from GitHub
   (Git must be installed; it is, if you use this repo).
4. If you don't see the game scene, double-click **Assets/Scenes/Main.unity**
   in the **Project** window (bottom of the screen).
5. Press **Play** (the ▶ button at the top). Click inside the **Game** view so it
   gets the keyboard, then play with `A S D F G H J K L ;` (white keys) and
   `W E T Y U O P` (black keys). Press ▶ again to stop.

Tip: in the Game view's toolbar choose **Play Maximized** (or "Maximize on Play")
for a bigger window.

## 2. Make a real Windows app

Menu **Keystage → Build for Windows**. When it finishes, the game is in
`Build/Windows/Keystage.exe` (the `Build` folder is not committed).

## 3. Use Claude Code with Unity (MCP)

The project is already wired for the free, open-source
[MCP for Unity](https://github.com/CoplayDev/unity-mcp) (v10.3.0, MIT):

| Piece | Where |
|-------|-------|
| Unity side (bridge inside the editor) | `Packages/manifest.json` → `com.coplaydev.unity-mcp` |
| Server Claude Code starts | `.mcp.json` → `uvx --from mcpforunityserver==10.3.0 mcp-for-unity --transport stdio` |
| `uv` / `uvx` (runs the server) | installed with `python -m pip install --user uv` |

Defaults applied by `Assets/Keystage/Editor/KeystageSetup.cs` (once per PC):
stdio transport (the bridge starts by itself whenever Unity opens, no button to
press), setup wizard skipped (the `.mcp.json` does that job) and the package's
telemetry turned off. Change any of them in **Window → MCP for Unity**.

To use it:

1. Open the project in Unity and leave it open.
2. Start a **new** Claude Code session in this folder. Claude Code reads
   `.mcp.json` and asks whether to trust the **UnityMCP** server: allow it.
3. Ask things like "enter play mode and show me the console errors" or
   "add a light to the scene". Claude can then see and change the editor.

If the server can't start, check that `uvx --version` works in a terminal. On
another PC, fix the `uvx` path in `.mcp.json` (find it with `where uvx`).

## 4. The phone keyboard

In the game open **Devices** (or the gear → Ways to play) and press **Show
pairing code**. Scan the QR code with a phone on the same Wi-Fi. The game itself
serves the phone page (port 8765). The first time, Windows Firewall asks about
Keystage or the Unity Editor: allow it on **private** networks.

## How it is built

```
Assets/
  Scenes/Main.unity            camera, event system, and the Keystage object
  Keystage/
    Scripts/
      KeystageApp.cs           starts everything, routes screens, keyboard shortcuts
      Core/                    notes, settings, translations (I18nData.cs), input bus
      Game/                    lessons and songs, scoring engines, progress
      Scene/                   3D piano, note runway, camera, colours
      Ui/                      UI Toolkit screens, dock, devices dialog, staff, icons
      Input/                   typing keyboard and MIDI (Windows winmm.dll)
      Audio/Synth.cs           the piano sound (generated, no samples)
      Remote/                  phone server (HTTP + WebSocket) and QR encoder
      AutoTest.cs              command-line test hooks (see below)
    Resources/
      UI/Keystage.uss          the styles (port of the web style.css)
      Fonts/                   Poppins, Righteous, Vazirmatn (+ licences)
      Phone/phone.html         the page phones open
      Materials/               base URP materials
    Editor/                    setup, build and MCP defaults
    Shaders/KeystageLabel.shader  text on the 3D keys
```

The whole game is built from code at runtime, so the scene stays tiny. If the
scene or materials ever break, use **Keystage → Rebuild Scene and Assets**.

### Differences from the web version

- The phone server lives inside the game instead of the Vite dev server.
- MIDI keyboards work on Windows (through `winmm.dll`, no plugin needed).
- "Reduce motion" follows Windows' *Show animations* setting.
- Everything else (lessons, songs, scoring, layout, translations) matches `main`.

### Automated checks

A built game accepts test flags, for example:

```bash
Keystage.exe -ks-reset -ks-unlock -ks-shots C:/tmp/shots
Keystage.exe -ks-autoplay -ks-autoplay-lessons n1,r2,s1 -logFile log.txt
```

`-ks-shots` saves screenshots of every screen and quits; `-ks-autoplay` plays
lessons perfectly and logs each score; `-ks-serve` starts the phone server;
`-ks-lang fa`, `-ks-theme light`, `-ks-names solfege` and `-ks-route lesson/s2`
pick the starting state. Nothing happens without these flags.

## Licences

Fonts are under the SIL Open Font License (files in `Resources/Fonts`). MCP for
Unity is MIT. Songs are public-domain melodies. The research behind the
lessons is listed in `docs/REFERENCES.md` on the `main` branch.
