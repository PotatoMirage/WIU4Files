using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    [SerializeField] private CheckPointTrigger[] checkpoints; //get the list of check points
        
    void Start()
    {
        
    }

    void Update()
    {
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i].GetInsideCheckpoint())
            {
                PlayerSave.Instance.SaveCurrentCheckpoint(i);

                HotbarInventory hotbar = Object.FindAnyObjectByType<HotbarInventory>();
                if (hotbar != null)
                {
                    hotbar.SaveInventory();
                }
            }
        }
    }


}
