using UnityEngine;
using UnityEngine.Rendering;

public class GodRaysSunTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light sunLight;   // your Directional Light
    [SerializeField] private Volume godRayVolume;

    [Header("Settings")]
    [SerializeField] private float sunDistance = 10000f; // how far to project the sun

    private GodRaysVolume _godRays;
    private Camera _cam;

    void Start()
    {
        _cam = Camera.main;

        if (godRayVolume != null)
            godRayVolume.profile.TryGet(out _godRays);

        if (sunLight == null)
        {
            var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in lights)
                if (l.type == LightType.Directional) { sunLight = l; break; }
        }
    }

    void Update()
    {
        if (_godRays == null || sunLight == null || _cam == null) return;

        // Project sun position far in the direction the light is pointing
        // Directional lights have no real position, so we fake one very far away
        Vector3 sunWorldPos = _cam.transform.position - sunLight.transform.forward * sunDistance;

        // Convert to viewport space (0,0 = bottom left, 1,1 = top right)
        Vector3 screenPos = _cam.WorldToViewportPoint(sunWorldPos);

        // If sun is behind camera, flip it so rays don't go weird
        bool behindCamera = screenPos.z < 0;
        if (behindCamera)
        {
            // Flip to opposite side of screen
            screenPos.x = 1f - screenPos.x;
            screenPos.y = 1f - screenPos.y;
        }

        _godRays.lightPosition.value = new Vector2(screenPos.x, screenPos.y);

        // Fade rays out when sun is behind camera or far off screen
        float offscreenDist = Vector2.Distance(
            new Vector2(screenPos.x, screenPos.y),
            new Vector2(0.5f, 0.5f)
        );
        float visibilityFade = behindCamera ? 0f : saturate(1f - (offscreenDist - 0.5f) * 2f);
        _godRays.intensity.value = Mathf.Lerp(0f, _baseIntensity, visibilityFade);
    }

    // Store the designer-set intensity as the max
    private float _baseIntensity;
    void OnEnable()
    {
        if (godRayVolume != null && godRayVolume.profile.TryGet(out _godRays))
            _baseIntensity = _godRays.intensity.value;
    }

    float saturate(float v) => Mathf.Clamp01(v);
}