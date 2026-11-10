using UnityEngine;
using UnityEngine.UI;

public class BlindManVision : MonoBehaviour
{
    private Camera cam;

    [Header("UI Opacity Settings")]
    public Image darkOverlayImage;
    [Range(0f, 1f)]
    public float visionOpacity = 0.8f; // 0 = Bilkul Clear, 1 = Poora Black

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        if (cam != null)
        {
            // Camera Far Clip Plane normal range par set kar dein
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.farClipPlane = 1000f;
        }

        // Agar Inspector se Image Assign nahi hui toh auto-find kar lo
        if (darkOverlayImage == null)
        {
            darkOverlayImage = GetComponentInChildren<Image>();
        }

        ApplyOpacity();
    }

    void Update()
    {
        ApplyOpacity();
    }

    void ApplyOpacity()
    {
        if (darkOverlayImage != null)
        {
            // Pure black color ke sath strictly alpha set karna
            Color c = Color.black;
            c.a = visionOpacity;
            darkOverlayImage.color = c;
        }
    }
} 