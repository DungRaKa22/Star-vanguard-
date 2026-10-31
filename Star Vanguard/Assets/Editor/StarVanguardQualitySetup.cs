using UnityEditor;
using UnityEngine;

namespace StarVanguard.EditorTools
{
    public static class StarVanguardQualitySetup
    {
        public static void SyncVSyncQualitySettings()
        {
            string[] names = QualitySettings.names;
            for (int i = 0; i < names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.vSyncCount = 1;
            }

            Debug.Log($"[QUALITY-SETUP] Synchronized vSyncCount = 1 across all {names.Length} quality tiers!");
            EditorApplication.Exit(0);
        }
    }
}
