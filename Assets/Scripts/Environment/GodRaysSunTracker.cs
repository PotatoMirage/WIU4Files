using UnityEngine;
using UnityEngine.Rendering;

public class GodRaysSunTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light sunLight;
    [Header("Settings")]
    [SerializeField] private float sunDistance = 10000f;
    [SerializeField] private float baseIntensity = 0.8f;

    private Camera _cam;

    void Start()
    {
        _cam = Camera.main;
        if (sunLight == null)
        {
            foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional) { sunLight = l; break; }
        }
    }

    void Update()
    {
        if (sunLight == null || _cam == null) return;

        Vector3 sunWorldPos = _cam.transform.position - sunLight.transform.forward * sunDistance;
        Vector3 screenPos = _cam.WorldToViewportPoint(sunWorldPos);

        bool behindCamera = screenPos.z < 0;

        GodRaysVolume.RuntimeLightPosition = new Vector2(
            Mathf.Clamp01(screenPos.x),
            Mathf.Clamp01(screenPos.y));

        if (behindCamera)
        {
            GodRaysVolume.RuntimeIntensity = 0f;
            return;
        }

        float offscreenDist = Vector2.Distance(
            new Vector2(screenPos.x, screenPos.y),
            new Vector2(0.5f, 0.5f));
        float visibilityFade = Mathf.Clamp01(1f - (offscreenDist - 0.3f) * 2f);

        GodRaysVolume.RuntimeIntensity = baseIntensity * visibilityFade;

        //Debug.Log($"Sun UV: ({screenPos.x:F2}, {screenPos.y:F2}), fade: {visibilityFade:F2}, intensity: {GodRaysVolume.RuntimeIntensity:F2}");
    }
}