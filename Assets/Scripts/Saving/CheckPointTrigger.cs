using UnityEngine;

public class CheckPointTrigger : MonoBehaviour
{
    //for checking if player is in checkpoint
    private bool incheckpoint = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            incheckpoint = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        incheckpoint = false;
    }

    public bool GetInsideCheckpoint() //getter for the bool
    {
        return incheckpoint;
    }
}
