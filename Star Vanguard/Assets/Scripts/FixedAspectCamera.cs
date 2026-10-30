using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectCamera : MonoBehaviour
{
    public static FixedAspectCamera Instance { get; private set; }

    [Tooltip("Target Aspect Ratio - Standard 16:9 for 1920x1080")]
    public float targetAspect = 16f / 9f;
    Camera cam;
    int sw, sh;

    public Camera Cam => cam != null ? cam : (cam = GetComponent<Camera>());

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
        Apply();
    }
    void Update(){ if (Screen.width != sw || Screen.height != sh) Apply(); }

    void Apply()
    {
        sw = Screen.width; sh = Screen.height;
        float windowAspect = (float)sw / sh;
        float scale = windowAspect / targetAspect;

        if (scale < 1f) { // letterbox top/bottom
            cam.rect = new Rect(0f, (1f - scale) * 0.5f, 1f, scale);
        } else {          // pillarbox sides
            float inv = 1f / scale;
            cam.rect = new Rect((1f - inv) * 0.5f, 0f, inv, 1f);
        }
    }
}