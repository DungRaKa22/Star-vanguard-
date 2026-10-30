using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using StarVanguard.UI;

namespace StarVanguard.Editor
{
    public static class StarVanguardMenuBaker
    {
        private const string k_MenuScenePath = "Assets/Scenes/MainMenuScene.unity";
        private const string k_RoundedSpritePath = "Assets/Sprites/UI/UI_Rounded.png";
        private const string k_OutlineSpritePath = "Assets/Sprites/UI/UI_RoundedOutline.png";

        [MenuItem("Star Vanguard/UI/Bake Main Menu to Scene")]
        public static void Bake()
        {
            Debug.Log("[StarVanguardMenuBaker] Generating UI sprite assets if missing...");
            GenerateUISpritesIfMissing();

            Debug.Log($"[StarVanguardMenuBaker] Opening {k_MenuScenePath}...");
            Scene menuScene = EditorSceneManager.OpenScene(k_MenuScenePath, OpenSceneMode.Single);
            if (!menuScene.IsValid())
            {
                Debug.LogError($"[StarVanguardMenuBaker] Could not open scene: {k_MenuScenePath}");
                return;
            }

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
                canvas = canvasObj.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            MainMenuScreen menuScreen = canvas.GetComponent<MainMenuScreen>();
            if (menuScreen == null)
            {
                menuScreen = Object.FindAnyObjectByType<MainMenuScreen>();
            }
            if (menuScreen == null)
            {
                menuScreen = canvas.gameObject.AddComponent<MainMenuScreen>();
            }

            UIPanelRouter router = canvas.GetComponent<UIPanelRouter>();
            if (router == null)
            {
                router = Object.FindAnyObjectByType<UIPanelRouter>();
            }
            if (router == null)
            {
                router = canvas.gameObject.AddComponent<UIPanelRouter>();
            }

            Debug.Log("[StarVanguardMenuBaker] Baking UI Hierarchy...");
            menuScreen.BakeHierarchy();

            EditorSceneManager.MarkSceneDirty(menuScene);
            bool saved = EditorSceneManager.SaveScene(menuScene);
            Debug.Log($"[StarVanguardMenuBaker] Baked MainMenuScene saved: {saved}!");
        }

        private static void GenerateUISpritesIfMissing()
        {
            string dir = "Assets/Sprites/UI";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            bool generated = false;
            if (!File.Exists(k_RoundedSpritePath))
            {
                Texture2D tex = CreateProceduralTexture(false);
                File.WriteAllBytes(k_RoundedSpritePath, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                generated = true;
            }

            if (!File.Exists(k_OutlineSpritePath))
            {
                Texture2D tex = CreateProceduralTexture(true);
                File.WriteAllBytes(k_OutlineSpritePath, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                generated = true;
            }

            if (generated)
            {
                AssetDatabase.Refresh();
            }

            ConfigureSpriteImporter(k_RoundedSpritePath);
            ConfigureSpriteImporter(k_OutlineSpritePath);
        }

        private static void ConfigureSpriteImporter(string i_AssetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(i_AssetPath) as TextureImporter;
            if (importer == null) return;

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteBorder != new Vector4(14, 14, 14, 14))
            {
                importer.spriteBorder = new Vector4(14, 14, 14, 14);
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (importer.filterMode != FilterMode.Bilinear)
            {
                importer.filterMode = FilterMode.Bilinear;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static Texture2D CreateProceduralTexture(bool i_Outline)
        {
            const int size = 64;
            const float radius = 14f;

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float cx = Mathf.Clamp(px, radius, size - radius);
                    float cy = Mathf.Clamp(py, radius, size - radius);
                    float distance = Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy));

                    float inside = Mathf.Clamp01(radius - distance + 0.5f);
                    float alpha = i_Outline
                        ? Mathf.Clamp01(inside - Mathf.Clamp01(radius - distance - 2f))
                        : inside;

                    pixels[(y * size) + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(alpha) * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }
    }
}
