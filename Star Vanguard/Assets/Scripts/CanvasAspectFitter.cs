using UnityEngine;

/// <summary>
/// Locks and fits Canvas UI strictly within the 16:9 (1920x1080) Camera viewport rect.
/// Guarantees that HUD elements (Hearts/Lives, Bombs, Score, Overheat Gauge)
/// never clip, stretch into letterbox black bars, or disappear on arbitrary aspect ratios.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CanvasAspectFitter : MonoBehaviour
{
    private RectTransform m_RectTransform;
    private Camera m_Camera;
    private Rect m_LastCamRect;
    private int m_LastWidth;
    private int m_LastHeight;

    private void Awake()
    {
        m_RectTransform = GetComponent<RectTransform>();
        m_Camera = Camera.main;
        ApplyFit();
    }

    private void Start()
    {
        if (m_Camera == null) m_Camera = Camera.main;
        ApplyFit();
    }

    private void Update()
    {
        if (m_Camera == null) EnsureCamera();
        if (m_Camera == null) return;

        if (Screen.width != m_LastWidth || Screen.height != m_LastHeight || m_Camera.rect != m_LastCamRect)
        {
            ApplyFit();
        }
    }

    private void EnsureCamera()
    {
        if (m_Camera == null) m_Camera = Camera.main;
    }

    public void ApplyFit()
    {
        if (m_RectTransform == null) m_RectTransform = GetComponent<RectTransform>();
        EnsureCamera();
        if (m_RectTransform == null) return;

        Rect camRect = m_Camera != null ? m_Camera.rect : new Rect(0f, 0f, 1f, 1f);

        m_RectTransform.anchorMin = camRect.min;
        m_RectTransform.anchorMax = camRect.max;
        m_RectTransform.offsetMin = Vector2.zero;
        m_RectTransform.offsetMax = Vector2.zero;

        m_LastCamRect = camRect;
        m_LastWidth = Screen.width;
        m_LastHeight = Screen.height;
    }
}
