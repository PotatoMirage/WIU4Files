using UnityEngine;
using System.Collections.Generic;

public class StatusEffectUIManager : MonoBehaviour
{
    public static StatusEffectUIManager Instance;

    public Transform container;
    public GameObject iconPrefab;

    private List<StatusEffectUIIcon> activeIcons = new List<StatusEffectUIIcon>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddEffect(string effectName, Sprite icon, float duration, bool isPermanent)
    {
        for (int i = 0; i < activeIcons.Count; i++)
        {
            if (activeIcons[i].effectName == effectName)
            {
                activeIcons[i].Initialize(effectName, icon, duration, isPermanent);
                return;
            }
        }

        GameObject newIconObj = Instantiate(iconPrefab, container);
        StatusEffectUIIcon newIcon = newIconObj.GetComponent<StatusEffectUIIcon>();

        newIcon.Initialize(effectName, icon, duration, isPermanent);
        activeIcons.Add(newIcon);
    }

    public void RemoveEffect(string effectName)
    {
        for (int i = activeIcons.Count - 1; i >= 0; i--)
        {
            if (activeIcons[i].effectName == effectName)
            {
                Destroy(activeIcons[i].gameObject);
                activeIcons.RemoveAt(i);
                break;
            }
        }
    }
}