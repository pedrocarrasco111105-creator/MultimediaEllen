using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureCompressor : EditorWindow
{
    private int maxSize = 1024; // Resolución máxima deseada (e.g., 2048, 1024, 512)
    private TextureImporterCompression compressionQuality = TextureImporterCompression.Compressed;

    [MenuItem("Tools/Optimizar Texturas del Proyecto")]
    public static void ShowWindow()
    {
        GetWindow<TextureCompressor>("Optimizador de Texturas");
    }

    private void OnGUI()
    {
        GUILayout.Label("Ajustes de Optimización Masiva", EditorStyles.boldLabel);

        // Configuración de la resolución máxima
        maxSize = EditorGUILayout.IntField("Resolución Máxima (e.g., 1024)", maxSize);

        // Configuración de la calidad de compresión
        compressionQuality = (TextureImporterCompression)EditorGUILayout.EnumPopup("Calidad de Compresión", compressionQuality);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("¡Optimizar Todas las Texturas Ahora!"))
        {
            OptimizeAllTextures();
        }
    }

    void OptimizeAllTextures()
    {
        // 1. Encontrar todos los archivos de textura en la carpeta Assets
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture", new[] { "Assets" });
        int count = 0;

        try
        {
            // Inicia una barra de progreso
            EditorUtility.DisplayProgressBar("Optimizando Texturas", "Iniciando...", 0f);

            for (int i = 0; i < textureGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(textureGuids[i]);
                float progress = (float)i / textureGuids.Length;

                EditorUtility.DisplayProgressBar("Optimizando Texturas", $"Procesando: {Path.GetFileName(path)}", progress);

                // 2. Obtener el importador de la textura
                TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

                if (textureImporter == null) continue;

                // 3. Aplicar los ajustes
                textureImporter.maxTextureSize = maxSize;
                textureImporter.compressionQuality = compressionQuality == TextureImporterCompression.Compressed ? 50 : 100; // Ajuste de calidad si es Compressed

                // Aplicar el formato de compresión para Standalone
                TextureImporterPlatformSettings platformSettings = textureImporter.GetPlatformTextureSettings("Standalone");
                platformSettings.overridden = true;
                platformSettings.maxTextureSize = maxSize;
                platformSettings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;

                // Aquí usamos el DXT5 por defecto para buena compresión de color/alfa
                platformSettings.format = TextureImporterFormat.DXT5;
                platformSettings.textureCompression = compressionQuality;
                textureImporter.SetPlatformTextureSettings(platformSettings);

                // IMPORTANTE: NO aplicar la compresión si es un Normal Map
                if (textureImporter.textureType != TextureImporterType.NormalMap)
                {
                    textureImporter.isReadable = false; // Desmarcar para ahorrar memoria
                }

                // 4. Aplicar y guardar los cambios
                AssetDatabase.ImportAsset(path);
                count++;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        Debug.Log($"✅ ¡Optimización completa! {count} texturas procesadas.");
    }
}