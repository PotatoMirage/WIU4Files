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
        {// loop through 
            if (checkpoints[i].GetInsideCheckpoint())
            {
                PlayerSave.Instance.SaveCurrentCheckpoint(i);
                //save the reset (etc. max health, damage, inv)
            }
        }
    }


}
