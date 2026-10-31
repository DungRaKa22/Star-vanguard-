using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;

namespace StarVanguard.EditorTools
{
    public static class StarVanguardAudioSetup
    {
        public static void WireAudioMixerInBootstrapScene()
        {
            string scenePath = "Assets/Scenes/BootstrapScene.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            var audioManager = UnityEngine.Object.FindAnyObjectByType<AudioManager>();
            if (audioManager == null)
            {
                Debug.LogError("[AUDIO-SETUP] AudioManager not found in BootstrapScene!");
                EditorApplication.Exit(1);
                return;
            }

            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/Audio/AudioMixer.mixer");
            if (mixer == null)
            {
                Debug.LogError("[AUDIO-SETUP] AudioMixer not found at Assets/Audio/AudioMixer.mixer!");
                EditorApplication.Exit(1);
                return;
            }

            var masterGroups = mixer.FindMatchingGroups("Master");
            var musicGroups = mixer.FindMatchingGroups("Music");
            var sfxGroups = mixer.FindMatchingGroups("SFX");

            var so = new SerializedObject(audioManager);
            var mixerProp = so.FindProperty("m_AudioMixer");
            var masterProp = so.FindProperty("m_MasterGroup");
            var musicProp = so.FindProperty("m_MusicGroup");
            var sfxProp = so.FindProperty("m_SFXGroup");

            if (mixerProp != null) mixerProp.objectReferenceValue = mixer;
            if (masterProp != null && masterGroups.Length > 0) masterProp.objectReferenceValue = masterGroups[0];
            if (musicProp != null && musicGroups.Length > 0) musicProp.objectReferenceValue = musicGroups[0];
            if (sfxProp != null && sfxGroups.Length > 0) sfxProp.objectReferenceValue = sfxGroups[0];

            so.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[AUDIO-SETUP] Successfully wired AudioMixer and groups into AudioManager in BootstrapScene.unity!");
            EditorApplication.Exit(0);
        }
    }
}
