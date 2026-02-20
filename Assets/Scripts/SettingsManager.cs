using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject Settings;
    //[SerializeField] private GameObject SaveandLeaveButton;
    void Start()
    {
        //set settings to false on defualt
        Settings.SetActive(false);
        //SaveandLeaveButton.SetActive(false); (not complete - post settings)
    }

    void Update()
    {
        //check if in check point and change Save and Leave button accordingly
    }

    public void OpenSettings()
    {
        //open Settings
        Settings.SetActive(true);

        //stop the game (pause)
    }

    public void CloseSettings()
    {
        //close Settings
        Settings.SetActive(false);

        //play the game (play)
    }

    public void SaveandLeave()
    {
        //save the progress and leave
    }
}
