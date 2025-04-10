using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;

public class CommandLineBuild
{
    // Main build method that takes care of platform-specific builds
    public static void Build()
    {
        // Fetch environment variables for platform, build number, version, etc.
        string platform = Environment.GetEnvironmentVariable("PLATFORM")?.ToLower();
        string buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER");
        string version = Environment.GetEnvironmentVariable("VERSION");
        string gameName = Environment.GetEnvironmentVariable("GAME_NAME");
        bool isDevelop = Environment.GetEnvironmentVariable("IS_DEVELOP")?.ToLower() == "true";

        // Get enabled scenes from the Build Settings
        string[] scenes = GetEnabledScenes();

        // Set build path based on platform
        string buildPath = $"Builds/{platform}";
        Directory.CreateDirectory(buildPath);  // Create the directory if it doesn't exist

        // Set bundle version for the build (specific to Unity)
        PlayerSettings.bundleVersion = version;

        // Configure debug symbols based on whether it's a dev build or not
        ConfigureDebugSymbols(isDevelop);

        // Switch case for different platforms to build accordingly
        switch (platform)
        {
            case "ios":
                BuildIOS(buildPath, buildNumber);  // Call IOS build method
                break;
            case "androidapk":
                BuildAndroid(buildPath, buildNumber, BuildTarget.Android, BuildOptions.None);  // Call Android APK build method
                break;
            case "androidaab":
                BuildAndroid(buildPath, buildNumber, BuildTarget.Android, BuildOptions.None, isAAB: true);  // Call Android AAB build method
                break;
            default:
                throw new Exception($"Unknown platform: {platform}");  // Throw error for unsupported platforms
        }
    }

    // iOS-specific build method
    private static void BuildIOS(string buildPath, string buildNumber)
    {
        // Set specific iOS settings for the build
        PlayerSettings.iOS.buildNumber = buildNumber;
        PlayerSettings.iOS.targetOSVersionString = "13.0";  // iOS minimum version
        PlayerSettings.SplashScreen.show = false;  // Disable splash screen

        // Save assets and refresh the database to make sure everything is up to date
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Start the build process for iOS
        BuildPipeline.BuildPlayer(GetEnabledScenes(), buildPath, BuildTarget.iOS, BuildOptions.None);
        
        // Get the full path of the iOS build directory and invoke post-build processing
        string fullXcodePath = Path.GetFullPath(buildPath);
        PostBuildProcessor.InvokePostBuildManually(BuildTarget.iOS, fullXcodePath);  // Optional post-build processing
    }

    // Android build method (handles both APK and AAB formats)
    private static void BuildAndroid(string buildPath, string buildNumber, BuildTarget target, BuildOptions options, bool isAAB = false)
    {
        // Set Android-specific build settings
        PlayerSettings.Android.bundleVersionCode = int.Parse(buildNumber);
        EditorUserBuildSettings.buildAppBundle = isAAB;  // Set whether to build AAB instead of APK

        // Handle keystore for Android signing if available
        if (!string.IsNullOrEmpty(keystorePath) && !string.IsNullOrEmpty(keystorePassword))
        {
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keystorePass = keystorePassword;
            PlayerSettings.Android.keyaliasName = keyaliasName;
            PlayerSettings.Android.keyaliasPass = keyaliasPassword;
        }
        else
        {
            Debug.LogWarning("Keystore Not Found");  // Warn if no keystore is found
        }

        // Decide on the extension based on whether it's an AAB or APK build
        string extension = isAAB ? ".aab" : ".apk";
        string fullPath = Path.Combine(buildPath, $"{PlayerSettings.productName}{extension}");

        // Start the Android build process
        BuildPipeline.BuildPlayer(GetEnabledScenes(), fullPath, target, options);
    }

    // Method to configure debug symbols depending on development status
    private static void ConfigureDebugSymbols(bool isDevelop)
    {
        SetDebugSymbolForGroup(BuildTargetGroup.iOS, isDevelop);  // iOS build symbols configuration
        SetDebugSymbolForGroup(BuildTargetGroup.Android, isDevelop);  // Android build symbols configuration
    }

    // Method to set or remove the debug symbols based on development build
    private static void SetDebugSymbolForGroup(BuildTargetGroup targetGroup, bool isDevelop)
    {
        // Fetch the current list of defines for this group
        var defineList = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup).Split(';').ToList();

        // Add or remove the "ENABLE_DEBUG" symbol based on isDevelop flag
        if (isDevelop) 
            defineList.Add("ENABLE_DEBUG");
        else 
            defineList.Remove("ENABLE_DEBUG");

        // Save the updated defines
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defineList));
    }

    // Helper function to get enabled scenes for the build
    private static string[] GetEnabledScenes()
    {
        var scenes = EditorBuildSettings.scenes;  // Fetch all scenes in Build Settings
        return scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();  // Return only the enabled scenes
    }
}
