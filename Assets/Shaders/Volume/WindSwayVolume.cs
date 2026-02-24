using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenu("Custom Atmosphere/Wind Sway")]
public class WindSwayVolume : VolumeComponent, IPostProcessComponent
{
    [Header("Sway")]
    public ClampedFloatParameter intensity      = new ClampedFloatParameter(0f, 0f, 1f);
    public ClampedFloatParameter swaySpeed      = new ClampedFloatParameter(0.8f, 0f, 5f);
    public ClampedFloatParameter swayStrength   = new ClampedFloatParameter(0.004f, 0f, 0.02f);
    public ClampedFloatParameter swayScale      = new ClampedFloatParameter(3.0f,  0f, 10f);   // spatial freq of sway

    [Header("Chromatic Drift")]
    // Subtle RGB split that drifts with wind direction, like light through leaves
    public ClampedFloatParameter chromaticDrift = new ClampedFloatParameter(0.0008f, 0f, 0.005f);

    [Header("Leaf Flicker")]
    // Rapid tiny brightness flicker — light flickering through canopy
    public ClampedFloatParameter flickerStrength = new ClampedFloatParameter(0.03f, 0f, 0.15f);
    public ClampedFloatParameter flickerSpeed    = new ClampedFloatParameter(4.0f,  0f, 10f);

    public bool IsActive()         => intensity.value > 0f;
    public bool IsTileCompatible() => false;
}
