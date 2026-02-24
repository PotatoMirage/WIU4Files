using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusEffectUIIcon : MonoBehaviour
{
    public string effectName;
    public Image iconImage;
    public TextMeshProUGUI timerText;

    private float remainingTime;
    private bool isPermanent;

    public void Initialize(string name, Sprite icon, float duration, bool permanent)
    {
        effectName = name;
        iconImage.sprite = icon;
        isPermanent = permanent;
        remainingTime = duration;

        if (isPermanent)
        {
            timerText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isPermanent)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime > 0f)
            {
                timerText.text = Mathf.CeilToInt(remainingTime).ToString();
            }
        }
    }
}