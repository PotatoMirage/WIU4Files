using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenu("Custom/Whimsical Paper")]
public class PaperVolume : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
    public ColorParameter paperColor = new ColorParameter(new Color(0.96f, 0.91f, 0.78f)); // aged parchment
    public ClampedFloatParameter grainAmount = new ClampedFloatParameter(0.08f, 0f, 1f);
    public ClampedFloatParameter inkBleed = new ClampedFloatParameter(0.4f, 0f, 1f);
    public ClampedFloatParameter sketchStrength = new ClampedFloatParameter(0.35f, 0f, 1f);
    public ClampedFloatParameter vignette = new ClampedFloatParameter(0.4f, 0f, 1f);

    public bool IsActive() => intensity.value > 0f;
    public bool IsTileCompatible() => true;
}