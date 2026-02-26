using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PaperToggle : MonoBehaviour
{
    public static PaperToggle Instance { get; private set; }

    private PaperVolume paper;

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        Volume volume = FindFirstObjectByType<Volume>();

        if (volume.profile.TryGet(out paper))
        {
            Debug.Log("Paper effect found!");
        }
        else
        {
            Debug.LogWarning("PaperVolume not found in Volume Profile!");
        }
    }

    public void TogglePaper()
    {
        if (paper == null) return;

        paper.intensity.value = paper.intensity.value > 0f ? 0f : 1f;
    }

    public void SetPaper(bool active)
    {
        if (paper == null) return;

        paper.intensity.value = active ? 1f : 0f;
    }
}