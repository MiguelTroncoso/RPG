#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MmorpgPrototype.Editor
{
    public static class BrandAssetGenerator
    {
        private const string GeneratedFolder = "Assets/Art/Generated";
        private const string IconPath = GeneratedFolder + "/valle-reliquias-icon-placeholder.png";
        private const string SplashPath = GeneratedFolder + "/valle-reliquias-splash-placeholder.png";

        [MenuItem("MMORPG/Branding/Generate Placeholder Store Art")]
        public static void GeneratePlaceholderStoreArt()
        {
            EnsurePlaceholderBrandAssets(true);
        }

        public static void EnsurePlaceholderBrandAssets(bool showDialog)
        {
            Directory.CreateDirectory(GeneratedFolder);

            if (!File.Exists(IconPath))
            {
                SavePng(CreateIconTexture(512), IconPath);
            }

            if (!File.Exists(SplashPath))
            {
                SavePng(CreateSplashTexture(1920, 1080), SplashPath);
            }

            AssetDatabase.Refresh();

            if (showDialog)
            {
                EditorUtility.DisplayDialog("Branding placeholder", "Icono y splash placeholder generados en Assets/Art/Generated.", "OK");
            }
        }

        private static Texture2D CreateIconTexture(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var u = (float)x / (size - 1);
                    var v = (float)y / (size - 1);
                    var centerX = u - 0.5f;
                    var centerY = v - 0.5f;
                    var vignette = Mathf.Clamp01(1f - Mathf.Sqrt(centerX * centerX + centerY * centerY) * 1.35f);
                    var color = Color.Lerp(new Color(0.04f, 0.07f, 0.09f), new Color(0.12f, 0.32f, 0.28f), v);

                    var halo = Mathf.Clamp01(1f - Mathf.Abs(centerX) * 2.8f - Mathf.Abs(centerY) * 2.1f);
                    color = Color.Lerp(color, new Color(0.2f, 0.72f, 0.72f), halo * 0.45f);

                    var diamond = Mathf.Abs(centerX) + Mathf.Abs(centerY * 1.32f);
                    if (diamond < 0.26f)
                    {
                        var core = Mathf.Clamp01(1f - diamond / 0.26f);
                        color = Color.Lerp(new Color(0.25f, 0.09f, 0.42f), new Color(0.94f, 0.84f, 0.42f), core);
                    }
                    else if (diamond < 0.31f)
                    {
                        color = new Color(0.88f, 0.74f, 0.32f);
                    }

                    color *= 0.58f + vignette * 0.58f;
                    pixels[y * size + x] = color;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        private static Texture2D CreateSplashTexture(int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var u = (float)x / (width - 1);
                    var v = (float)y / (height - 1);
                    var color = Color.Lerp(new Color(0.025f, 0.03f, 0.04f), new Color(0.08f, 0.22f, 0.19f), v);
                    var mist = Mathf.Sin((u * 9f + v * 3f) * Mathf.PI) * 0.5f + 0.5f;
                    color = Color.Lerp(color, new Color(0.14f, 0.42f, 0.36f), mist * 0.08f);

                    var centerX = Mathf.Abs(u - 0.5f);
                    var monolith = centerX + Mathf.Abs((v - 0.52f) * 0.42f);
                    if (monolith < 0.08f && v > 0.25f && v < 0.78f)
                    {
                        color = Color.Lerp(new Color(0.22f, 0.08f, 0.32f), new Color(0.82f, 0.72f, 0.38f), Mathf.Clamp01((0.08f - monolith) * 12f));
                    }

                    var ground = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((0.24f - v) * 7f));
                    color = Color.Lerp(color, new Color(0.05f, 0.1f, 0.075f), ground);

                    pixels[y * width + x] = color;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        private static void SavePng(Texture2D texture, string path)
        {
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }
    }
}
#endif
