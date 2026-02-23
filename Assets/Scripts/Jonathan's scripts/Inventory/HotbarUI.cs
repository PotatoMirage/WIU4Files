using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    public Image[] slotIcons;
    public Image[] selectionHighlights;

    public void UpdateSlot(int index, ItemData item)
    {
        if (item != null)
        {
            slotIcons[index].sprite = item.icon;
            slotIcons[index].enabled = true;
        }
        else
        {
            slotIcons[index].sprite = null;
            slotIcons[index].enabled = false;
        }
    }

    public void UpdateSelection(int selectedIndex)
    {
        for (int i = 0; i < selectionHighlights.Length; i++)
        {
            selectionHighlights[i].enabled = (i == selectedIndex);
        }
    }
}