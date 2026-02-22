using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject Settings;
    [SerializeField] private GameObject Mainmenu;

    void Start()
    {
        //setting everything first (in case)
        Settings.SetActive(false);
        Mainmenu.SetActive(true);

        //setting the player prefs for the first time
        
    }

    //for start button
    public void StartGame()
    {
        //change scenes here
    }

    //for settings page opening
    public void SettingsPage()
    {
        //put the changing of pages
        Mainmenu.SetActive(false);
        Settings.SetActive(true);
    }

    //for closing settings
    public void CloseSettings()
    {
        //changing of pages
        Settings.SetActive(false);
        Mainmenu.SetActive(true);
    }
}
