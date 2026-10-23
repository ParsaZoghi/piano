# Keystage (Unity branch)

Unity 6000.6.1f1 (URP, Input System, UI Toolkit) port of the web game on `main`.
See README.md for the layout. The web source on `main` is the reference: keep
behaviour, text keys and lesson data in sync with it (`git show main:src/...`).

## Working with Unity

- When the editor is open, the **UnityMCP** server (`.mcp.json`, MCP for Unity
  v10.3.0, stdio) gives tools like `manage_editor` (play/stop), `read_console`,
  `manage_scene`, `manage_gameobject`. Prefer them to check changes live.
- Without the editor, compile and run headless:
  - setup / scene rebuild: `Unity.exe -batchmode -quit -projectPath . -executeMethod Keystage.EditorTools.KeystageSetup.Run -logFile -`
  - Windows build: `-executeMethod Keystage.EditorTools.KeystageBuild.Windows -ks-out <path>`
  - headless editor with the MCP bridge: set `UNITY_MCP_ALLOW_BATCH=1` and run
    `-batchmode -executeMethod MCPForUnity.Editor.McpCiBoot.StartStdioForCi` (no `-quit`).
- Built games take `-ks-*` test flags (AutoTest.cs) for screenshots and autoplay.

## Conventions

- The game is built in code at runtime; the scene only holds the camera, the
  EventSystem and the `Keystage` object (UIDocument + KeystageApp).
- Text lives in `Scripts/Core/I18nData.cs` (generated from `main`'s
  `src/i18n/*.js`, then edited by hand). Never hard-code UI strings.
- Styles are in `Resources/UI/Keystage.uss`. USS has no gap, grid, sibling
  selectors or media queries: spacing and responsive classes are set in C#.
- Enter Play Mode runs without a domain reload: static state survives between
  Play sessions, so undo subscriptions in `OnDestroy` and null-check caches.
- Unity is left-handed: the web version's z values are mirrored (player at -Z).
