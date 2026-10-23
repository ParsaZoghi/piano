using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Keystage.EditorTools
{
    /// <summary>
    /// Builds everything the game needs inside the Unity project: base
    /// materials, the UI panel settings, the Main scene and player settings.
    /// Run it from the menu (Keystage > Rebuild Scene and Assets) or from the
    /// command line with -executeMethod Keystage.EditorTools.KeystageSetup.Run.
    /// It is safe to run again; it overwrites only its own files.
    /// </summary>
    public static class KeystageSetup
    {
        const string Root = "Assets/Keystage";
        const string ScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("Keystage/Rebuild Scene and Assets")]
        public static void Run()
        {
            Directory.CreateDirectory($"{Root}/Resources/Materials");
            Directory.CreateDirectory($"{Root}/UI");
            AssetDatabase.Refresh();

            CreateMaterial("Lit", "Universal Render Pipeline/Lit");
            CreateMaterial("Unlit", "Universal Render Pipeline/Unlit");
            CreateMaterial("Label", "Keystage/Label");
            var panel = CreatePanelSettings();
            CreateScene(panel);
            ConfigurePlayer();
            ConfigureAudio();
            McpDefaults.Apply(force: true);
            AssetDatabase.SaveAssets();
            Debug.Log("[Keystage] Setup finished.");
        }

        static void CreateMaterial(string name, string shaderName)
        {
            string path = $"{Root}/Resources/Materials/{name}.mat";
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError($"[Keystage] Shader not found: {shaderName}");
                return;
            }
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                existing.shader = shader;
                EditorUtility.SetDirty(existing);
                return;
            }
            AssetDatabase.CreateAsset(new Material(shader) { name = name }, path);
        }

        static PanelSettings CreatePanelSettings()
        {
            string themePath = $"{Root}/UI/KeystageTheme.tss";
            if (!File.Exists(themePath))
            {
                File.WriteAllText(themePath, "@import url(\"unity-theme://default\");\n");
                AssetDatabase.ImportAsset(themePath);
            }
            string path = $"{Root}/UI/KeystagePanel.asset";
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(panel, path);
            }
            panel.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(themePath);
            // Behave like a browser: 1 UI pixel = 1 CSS pixel at 96 dpi, scaled for high-dpi screens.
            panel.scaleMode = PanelScaleMode.ConstantPhysicalSize;
            panel.referenceDpi = 96;
            panel.fallbackDpi = 96;
            panel.clearColor = false;
            EditorUtility.SetDirty(panel);
            return panel;
        }

        static void CreateScene(PanelSettings panel)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // The game turns on linear fog at runtime; saving it in the scene keeps the
            // fog shader variants in builds (Unity strips fog variants no scene uses).
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.skybox = null;

            var cameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = cameraGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.043f, 0.043f, 0.11f);
            cameraGo.AddComponent<AudioListener>();
            var camData = cameraGo.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = false;
            camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;

            var events = new GameObject("EventSystem");
            events.AddComponent<EventSystem>();
            events.AddComponent<InputSystemUIInputModule>();

            var app = new GameObject("Keystage");
            var doc = app.AddComponent<UIDocument>();
            doc.panelSettings = panel;
            app.AddComponent<KeystageApp>();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            // The template's sample scene is not used any more.
            if (File.Exists("Assets/Scenes/SampleScene.unity")) AssetDatabase.DeleteAsset("Assets/Scenes/SampleScene.unity");
        }

        static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "Keystage";
            PlayerSettings.productName = "Keystage";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
            PlayerSettings.visibleInBackground = true;
            PlayerSettings.colorSpace = ColorSpace.Linear;
        }

        /// <summary>A small audio buffer keeps the delay between a key press and its sound short.</summary>
        static void ConfigureAudio()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/AudioManager.asset");
            if (assets == null || assets.Length == 0) return;
            var so = new SerializedObject(assets[0]);
            var dsp = so.FindProperty("m_DSPBufferSize");
            if (dsp == null) return;
            dsp.intValue = 256;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    /// <summary>
    /// Sensible defaults for the MCP for Unity package so it works with Claude
    /// Code out of the box: the stdio bridge (starts by itself whenever Unity
    /// opens), no setup wizard (the repo's .mcp.json already configures Claude
    /// Code) and telemetry off. Applied once per machine; change them any time in
    /// Window > MCP for Unity.
    /// </summary>
    [InitializeOnLoad]
    public static class McpDefaults
    {
        const string AppliedKey = "Keystage.McpDefaultsApplied";

        static McpDefaults() => Apply(force: false);

        public static void Apply(bool force)
        {
            if (!force && EditorPrefs.GetBool(AppliedKey, false)) return;
            EditorPrefs.SetBool("MCPForUnity.UseHttpTransport", false);
            EditorPrefs.SetBool("MCPForUnity.SetupCompleted", true);
            EditorPrefs.SetBool("MCPForUnity.TelemetryDisabled", true);
            EditorPrefs.SetBool(AppliedKey, true);
        }
    }
}
