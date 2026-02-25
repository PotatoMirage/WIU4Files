using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    [SerializeField] private CheckPointTrigger[] checkpoints; //get the list of check points
    [SerializeField] private PlayerMovementScript player; //for player stats

    private bool showsaveandleave;
        
    void Start()
    {
        showsaveandleave = false;
    }

    void Update()
    {
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i].GetInsideCheckpoint())
            {
                Debug.Log("checkpoint");
                
                PlayerSave.Instance.SaveCurrentCheckpoint(i);
                PlayerSave.Instance.SaveMaxHealth(player.maxHealth);
                PlayerSave.Instance.SaveMaxDamage(15);
                //save the reset (etc. max health, damage, inv)
            
                //show the save view
                if (i == 0)
                { //first checkpoint (spawn point)
                    showsaveandleave = true;
                }
                else
                {
                    showsaveandleave = false;
                }
            }
            else
            {
                showsaveandleave = false;
            }
        }
    }

    public bool getsaveandleave()
    {
        return showsaveandleave;
    }
}
