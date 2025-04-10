using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;

public class CommandLineBuild
{
    public static void Build()
    {
        string platform = Environment.GetEnvironmentVariable("PLATFORM")?.ToLower();
        string buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER");
        string version = Environment.GetEnvironmentVariable("VERSION");
        string gameName = Environment.GetEnvironmentVariable("GAME_NAME");
        bool isDevelop = Environment.GetEnvironmentVariable("IS_DEVELOP")?.ToLower() == "true";

        string[] scenes = GetEnabledScenes();

        string buildPath = $"Builds/{platform}";
        Directory.CreateDirectory(buildPath);

        PlayerSettings.bundleVersion = version;

        ConfigureDebugSymbols(isDevelop);

        switch (platform)
        {
            case "ios":
                BuildIOS(buildPath, buildNumber);
                break;
            case "androidapk":
                BuildAndroid(buildPath, buildNumber, BuildTarget.Android, BuildOptions.None);
                break;
            case "androidaab":
                BuildAndroid(buildPath, buildNumber, BuildTarget.Android, BuildOptions.None, isAAB: true);
                break;
            default:
                throw new Exception($"Unknown platform: {platform}");
        }
    }

    private static void BuildIOS(string buildPath, string buildNumber)
    {
        PlayerSettings.iOS.buildNumber = buildNumber;
        PlayerSettings.iOS.targetOSVersionString = "13.0";
        PlayerSettings.SplashScreen.show = false;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        BuildPipeline.BuildPlayer(GetEnabledScenes(), buildPath, BuildTarget.iOS, BuildOptions.None);

        string fullXcodePath = Path.GetFullPath(buildPath);
        PostBuildProcessor.InvokePostBuildManually(BuildTarget.iOS, fullXcodePath);
    }

    private static void BuildAndroid(string buildPath, string buildNumber, BuildTarget target, BuildOptions options, bool isAAB = false)
    {
        PlayerSettings.Android.bundleVersionCode = int.Parse(buildNumber);
        EditorUserBuildSettings.buildAppBundle = isAAB;

        PlayerSettings.SplashScreen.show = false;

        string keystorePath = Environment.GetEnvironmentVariable("KEYSTORE_PATH");
        string keystorePass = Environment.GetEnvironmentVariable("KEYSTORE_PASS");
        string keyAlias = Environment.GetEnvironmentVariable("KEY_ALIAS");
        string keyPass = Environment.GetEnvironmentVariable("KEY_PASS");

        if (!string.IsNullOrEmpty(keystorePath) && !string.IsNullOrEmpty(keystorePass) &&
            !string.IsNullOrEmpty(keyAlias) && !string.IsNullOrEmpty(keyPass))
        {
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasName = keyAlias;
            PlayerSettings.Android.keyaliasPass = keyPass;
        }
        else
        {
            Debug.LogWarning("Keystore Not Found");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string extension = isAAB ? ".aab" : ".apk";
        string fullPath = Path.Combine(buildPath, $"{PlayerSettings.productName}{extension}");

        BuildPipeline.BuildPlayer(GetEnabledScenes(), fullPath, target, options);
    }

    private static void ConfigureDebugSymbols(bool isDevelop)
    {
        SetDebugSymbolForGroup(BuildTargetGroup.iOS, isDevelop);
        SetDebugSymbolForGroup(BuildTargetGroup.Android, isDevelop);
    }

    private static void SetDebugSymbolForGroup(BuildTargetGroup targetGroup, bool isDevelop)
    {
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        var defineList = defines.Split(';').Where(d => !string.IsNullOrWhiteSpace(d)).ToList();

        if (isDevelop)
        {
            if (!defineList.Contains("ENABLE_DEBUG"))
            {
                defineList.Add("ENABLE_DEBUG");
            }
        }
        else
        {
            defineList.Remove("ENABLE_DEBUG");
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defineList));
    }


    public static void Buildios()
    {
        string platform = Environment.GetEnvironmentVariable("PLATFORM");
        string buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER");
        string version = Environment.GetEnvironmentVariable("VERSION");
        string gameName = Environment.GetEnvironmentVariable("GAME_NAME");
        bool isDevelop = Environment.GetEnvironmentVariable("IS_DEVELOP").ToLower() == "true";

        string[] scenes = GetEnabledScenes();

        string buildPath = "Builds/ios";
        Directory.CreateDirectory(buildPath);

        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
        var defineList = defines.Split(';').Where(d => !string.IsNullOrWhiteSpace(d)).ToList();

        if (isDevelop)
        {
            if (!defineList.Contains("ENABLE_DEBUG"))
            {
                defineList.Add("ENABLE_DEBUG");
            }
        }
        else
        {
            defineList.Remove("ENABLE_DEBUG");
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, string.Join(";", defineList));

        PlayerSettings.bundleVersion = version;
        PlayerSettings.iOS.buildNumber = buildNumber;
        PlayerSettings.iOS.targetOSVersionString = "13.0";

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.iOS, BuildOptions.None);
    }

    private static string[] GetEnabledScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        string[] enabledScenes = new string[scenes.Length];

        for (int i = 0; i < scenes.Length; i++)
        {
            enabledScenes[i] = scenes[i].path;
        }

        return enabledScenes;
    }

}
