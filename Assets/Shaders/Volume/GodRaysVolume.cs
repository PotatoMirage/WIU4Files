using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenu("Custom Atmosphere/God Rays")]
public class GodRaysVolume : VolumeComponent, IPostProcessComponent
{
    [Header("Light Source")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
    public Vector2Parameter lightPosition = new Vector2Parameter(new Vector2(0.5f, 0.8f));
    public ColorParameter rayColor = new ColorParameter(new Color(1f, 0.95f, 0.7f));
    [Header("Ray Shape")]
    public ClampedFloatParameter rayLength = new ClampedFloatParameter(0.4f, 0f, 1f);
    public ClampedIntParameter samples = new ClampedIntParameter(64, 16, 128);
    public ClampedFloatParameter density = new ClampedFloatParameter(0.6f, 0f, 1f);
    public ClampedFloatParameter decay = new ClampedFloatParameter(0.95f, 0.8f, 1f);
    public ClampedFloatParameter weight = new ClampedFloatParameter(0.4f, 0f, 1f);
    public ClampedFloatParameter exposure = new ClampedFloatParameter(0.3f, 0f, 1f);
    [Header("Threshold")]
    public ClampedFloatParameter brightnessThreshold = new ClampedFloatParameter(0.7f, 0f, 1f);

    // Static values written by GodRaysSunTracker, read directly by the feature
    // bypassing the volume stack blend which resets overrides every frame
    public static float RuntimeIntensity = 0f;
    public static Vector2 RuntimeLightPosition = new Vector2(0.5f, 0.8f);

    public bool IsActive() => RuntimeIntensity > 0f;
    public bool IsTileCompatible() => false;
}