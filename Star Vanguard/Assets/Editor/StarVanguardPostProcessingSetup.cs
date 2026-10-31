using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace StarVanguard.EditorTools
{
    public static class StarVanguardPostProcessingSetup
    {
        public static void ConfigurePostProcessingAndSave()
        {
            // 1. Configure DefaultVolumeProfile
            string profilePath = "Assets/Settings/DefaultVolumeProfile.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (profile == null)
            {
                Debug.LogError($"[POST-SETUP] Could not load VolumeProfile at {profilePath}");
                EditorApplication.Exit(1);
                return;
            }

            if (!profile.TryGet<Bloom>(out var bloom))
            {
                bloom = profile.Add<Bloom>(true);
            }
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.85f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 1.0f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.7f;

            if (!profile.TryGet<Vignette>(out var vignette))
            {
                vignette = profile.Add<Vignette>(true);
            }
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.25f;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.35f;

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            // 2. Open GameplayScene and enable renderPostProcessing on Main Camera
            string scenePath = "Assets/Scenes/GameplayScene.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            var cam = GameObject.FindWithTag("MainCamera");
            if (cam == null) cam = GameObject.Find("Main Camera");

            if (cam != null)
            {
                var camData = cam.GetComponent<UniversalAdditionalCameraData>();
                if (camData == null)
                {
                    camData = cam.AddComponent<UniversalAdditionalCameraData>();
                }
                camData.renderPostProcessing = true;
                EditorUtility.SetDirty(camData);
                EditorUtility.SetDirty(cam);
                Debug.Log("[POST-SETUP] Enabled renderPostProcessing on Main Camera in GameplayScene!");
            }
            else
            {
                Debug.LogWarning("[POST-SETUP] Main Camera not found in GameplayScene!");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[POST-SETUP] Successfully configured URP Bloom, Vignette, and Camera Post-Processing!");
            EditorApplication.Exit(0);
        }
    }
}
