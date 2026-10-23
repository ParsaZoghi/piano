using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Keystage.EditorTools
{
    /// <summary>
    /// Builds a Windows version of the game. From the menu (Keystage > Build for
    /// Windows) it goes to Build/Windows; from the command line pass
    /// -executeMethod Keystage.EditorTools.KeystageBuild.Windows -ks-out PATH.
    /// </summary>
    public static class KeystageBuild
    {
        [MenuItem("Keystage/Build for Windows")]
        public static void Windows()
        {
            string output = "Build/Windows/Keystage.exe";
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "-ks-out");
            if (i >= 0 && i + 1 < args.Length) output = args[i + 1];

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Main.unity" },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            });
            Debug.Log($"[Keystage] Build {report.summary.result}: {output} ({report.summary.totalErrors} errors)");
            if (Application.isBatchMode && report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
        }
    }
}
