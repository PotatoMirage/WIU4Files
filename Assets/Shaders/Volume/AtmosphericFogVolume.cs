using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenu("Custom Atmosphere/Fog Depth")]
public class AtmosphericFogVolume : VolumeComponent, IPostProcessComponent
{
    [Header("Fog Shape")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
    public ClampedFloatParameter fogStart = new ClampedFloatParameter(5f, 0f, 50f);
    public ClampedFloatParameter fogEnd = new ClampedFloatParameter(40f, 0f, 200f);
    public ClampedFloatParameter fogDensity = new ClampedFloatParameter(0.6f, 0f, 1f);
    [Header("Fog Color")]
    public ColorParameter fogColor = new ColorParameter(new Color(0.7f, 0.85f, 0.6f));
    public ClampedFloatParameter horizonBias = new ClampedFloatParameter(0.5f, 0f, 1f);
    [Header("Height Fog")]
    public BoolParameter useHeightFog = new BoolParameter(false);
    public ClampedFloatParameter heightFogStart = new ClampedFloatParameter(0f, -10f, 20f);
    public ClampedFloatParameter heightFogEnd = new ClampedFloatParameter(3f, 0f, 20f);
    public ClampedFloatParameter heightFogDensity = new ClampedFloatParameter(0.4f, 0f, 1f);
    [Header("Player Shield")]
    [Tooltip("Tag used to find the player GameObject at runtime.")]
    public string playerTag = "Player";
    [Tooltip("Screen-space radius that blocks fog behind the player. Start at 0.15 and tune up if fog bleeds through.")]
    public ClampedFloatParameter playerShieldRadius = new ClampedFloatParameter(0.15f, 0f, 0.5f);

    public bool IsActive() => intensity.value > 0f;
    public bool IsTileCompatible() => false;
}