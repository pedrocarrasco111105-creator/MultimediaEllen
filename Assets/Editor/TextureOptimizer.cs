using UnityEditor;
using UnityEngine;

public class TextureOptimizer : EditorWindow
{
    // =========================================================================================
    // CONFIGURACIÓN DE OPTIMIZACIÓN (Unity 6000 Compatible)
    // =========================================================================================

    // Tamaño Máximo: 1024 o 512 para gran reducción de peso.
    private static int maxTextureSize = 1024;

    // Compresión: 'Compressed' es la opción general y compatible.
    // Alternativas: TextureImporterCompression.Uncompressed (máxima calidad, sin ahorro), 
    // TextureImporterCompression.Medium (menor calidad, más ahorro).
    private static TextureImporterCompression compressionQuality = TextureImporterCompression.Compressed;

    // =========================================================================================

    [MenuItem("Tools/Optimizar/Comprimir Todas las Texturas")]
    public static void ShowWindow()
    {
        GetWindow<TextureOptimizer>("Optimizador de Texturas");
    }

    private void OnGUI()
    {
        GUILayout.Label("Configuración de Optimización Global", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // GUI para modificar los valores de configuración
        maxTextureSize = EditorGUILayout.IntField("Tamaño Máximo (px)", maxTextureSize);
        compressionQuality = (TextureImporterCompression)EditorGUILayout.EnumPopup("Calidad Compresión", compressionQuality);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("¡Ejecutar Compresión Masiva Ahora!"))
        {
            OptimizeAllTextures();
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "Esta acción modificará los ajustes de importación de TODAS las texturas, forzando la compresión y la reducción de tamaño. Use 'Compressed' para el mejor equilibrio.",
            MessageType.Warning);
    }


    public static void OptimizeAllTextures()
    {
        string[] allGuids = AssetDatabase.FindAssets("t:Texture");
        int count = allGuids.Length;
        int processedCount = 0;

        Debug.Log($"Iniciando optimización de {count} texturas...");

        foreach (string guid in allGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            EditorUtility.DisplayProgressBar("Optimizando Texturas", path, (float)processedCount / count);
            processedCount++;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null)
            {
                // 1. Aplicar el tamaño máximo a la configuración general
                importer.maxTextureSize = maxTextureSize;

                // 2. Desactivar Mipmaps en texturas de UI (si el nombre lo indica)
                if (path.Contains("UI") || path.Contains("Canvas"))
                {
                    importer.mipmapEnabled = false;
                }
                else
                {
                    importer.mipmapEnabled = true; // Mantenemos Mipmaps para modelos 3D
                }

                // 3. Obtener la configuración de la plataforma Standalone (PC/Mac/Linux)
                TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Standalone");

                // 4. Aplicar la compresión usando la estructura moderna
                platformSettings.overridden = true;
                platformSettings.maxTextureSize = maxTextureSize;
                platformSettings.textureCompression = compressionQuality;
                platformSettings.format = TextureImporterFormat.Automatic;

                // 5. Establecer la configuración actualizada
                importer.SetPlatformTextureSettings(platformSettings);

                // 6. Reimportar el asset con los nuevos ajustes
                AssetDatabase.ImportAsset(path);
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        Debug.Log($"Optimización completada. {count} texturas procesadas.");
    }
}