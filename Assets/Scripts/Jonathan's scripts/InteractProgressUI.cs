using UnityEngine;
using UnityEngine.UI;

public class InteractProgressUI : MonoBehaviour
{
    public static InteractProgressUI Instance;

    public GameObject uiContainer;
    public Image fillImage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        HideProgress();
    }

    public void ShowProgress()
    {
        if (uiContainer != null)
        {
            uiContainer.SetActive(true);
        }

        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
        }
    }

    public void UpdateProgress(float currentTimer, float maxHoldTime)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = currentTimer / maxHoldTime;
        }
    }

    public void HideProgress()
    {
        if (uiContainer != null)
        {
            uiContainer.SetActive(false);
        }

        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
        }
    }
}