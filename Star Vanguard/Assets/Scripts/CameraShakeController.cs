using UnityEngine;

namespace StarVanguard
{
    /// <summary>
    /// Zero-GC procedural Camera Shake controller for Star Vanguard.
    /// Simulates cinematic impact trauma when taking damage or detonating bombs.
    /// Restores exact local camera position upon completion.
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraShakeController : MonoBehaviour
    {
        private static CameraShakeController s_Instance;

        private Vector3 m_OriginalLocalPos;
        private float m_Trauma = 0f;
        private float m_Duration = 0f;
        private float m_Timer = 0f;

        private void Awake()
        {
            s_Instance = this;
            m_OriginalLocalPos = transform.localPosition;
        }

        private void OnDestroy()
        {
            if (s_Instance == this) s_Instance = null;
        }

        public static void Shake(float i_Intensity, float i_Duration = 0.25f)
        {
            if (s_Instance == null)
            {
                var cam = Camera.main;
                if (cam != null)
                {
                    s_Instance = cam.GetComponent<CameraShakeController>();
                    if (s_Instance == null)
                    {
                        s_Instance = cam.gameObject.AddComponent<CameraShakeController>();
                    }
                }
            }

            if (s_Instance != null)
            {
                s_Instance.TriggerShake(i_Intensity, i_Duration);
            }
        }

        public void TriggerShake(float i_Intensity, float i_Duration)
        {
            m_Trauma = Mathf.Max(m_Trauma, i_Intensity);
            m_Duration = Mathf.Max(m_Duration, i_Duration);
            m_Timer = m_Duration;
        }

        private void LateUpdate()
        {
            if (m_Timer > 0f)
            {
                m_Timer -= Time.deltaTime;
                float progress = Mathf.Clamp01(m_Timer / Mathf.Max(0.001f, m_Duration));
                float currentIntensity = m_Trauma * progress * progress; // Quadratic decay

                float offsetX = (Mathf.PerlinNoise(Time.time * 25f, 0f) * 2f - 1f) * currentIntensity;
                float offsetY = (Mathf.PerlinNoise(0f, Time.time * 25f) * 2f - 1f) * currentIntensity;

                transform.localPosition = m_OriginalLocalPos + new Vector3(offsetX, offsetY, 0f);

                if (m_Timer <= 0f)
                {
                    m_Trauma = 0f;
                    m_Duration = 0f;
                    transform.localPosition = m_OriginalLocalPos;
                }
            }
        }
    }
}
