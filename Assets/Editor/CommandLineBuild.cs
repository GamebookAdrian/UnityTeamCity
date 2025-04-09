using UnityEditor;
using UnityEngine;
using System;

public static class CommandLineBuild
{
    public static void Build()
    {
        Debug.Log("✅ Ejecutando método CommandLineBuild.Build");

        // Leer variables de entorno
        string platform = Environment.GetEnvironmentVariable("PLATFORM") ?? "unknown";
        string version = Environment.GetEnvironmentVariable("VERSION") ?? "0.0.0";
        string buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? "0";
        string isDevelop = Environment.GetEnvironmentVariable("IS_DEVELOP") ?? "false";

        // Mostrar en consola
        Debug.Log($"🔧 Platform: {platform}");
        Debug.Log($"📦 Version: {version}");
        Debug.Log($"🔢 Build Number: {buildNumber}");
        Debug.Log($"🛠️ Develop Mode: {isDevelop}");

        // Aquí iría lógica real de build si quieres
        // Por ahora solo dejamos trazas

        Debug.Log("✅ Build de prueba completado correctamente.");
    }
}
