using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class CommandLineBuild
{
    public static void Build()
    {
        string path = "Builds/iOS";
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = new[] {"Assets/Scenes/Main.unity"},
            locationPathName = path,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("✅ iOS Build success: " + summary.totalSize + " bytes");
        }
        else
        {
            Debug.LogError("❌ iOS Build failed: " + summary.result);
            EditorApplication.Exit(1);
        }
    }
}
