using System;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject Settings;
    [SerializeField] private GameObject Mainmenu;

    [Header("background")]
    [SerializeField] private GameObject backgroundlight;
    [SerializeField] private GameObject backgroundswamp;
    [SerializeField] private GameObject backgrounddark;

    void Start()
    {
        //setting everything first (in case)
        Settings.SetActive(false);
        Mainmenu.SetActive(true);

        //setting the player prefs for the first time
        PlayerSave.Instance.FirstSave();

        //setting the background
        if (PlayerSave.Instance.GetCurrentStage() == 1)
        {
            backgroundlight.SetActive(true);
            backgroundswamp.SetActive(false);
            backgrounddark.SetActive(false);
        }
        else if (PlayerSave.Instance.GetCurrentStage() == 2)
        {
            backgroundlight.SetActive(false);
            backgroundswamp.SetActive(true);
            backgrounddark.SetActive(false);
        }
        else if (PlayerSave.Instance.GetCurrentStage() == 3)
        {
            backgroundlight.SetActive(false);
            backgroundswamp.SetActive(false);
            backgrounddark.SetActive(true);
        }
    }

    //for start button
    public void StartGame()
    {
        //change scenes here
        if (PlayerSave.Instance.GetCurrentStage() == 1)
        {
            SceneManager.LoadScene("level 1");
        }
        else if (PlayerSave.Instance.GetCurrentStage() == 2)
        {
            SceneManager.LoadScene("level 2");
        }
        else if (PlayerSave.Instance.GetCurrentStage() == 3)
        {
            SceneManager.LoadScene("level 3");
        }
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
