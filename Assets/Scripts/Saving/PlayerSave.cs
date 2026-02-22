using UnityEngine;

public class PlayerSave : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FirstSave()
    {
        //if one of it does not exist on first open then set them
        if (!PlayerPrefs.HasKey("MasterVolume"))
        {
            SaveMasterVolume(100f);
        }
    }

    public void SaveMasterVolume(float value) //save master volume settings
    {
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }

    public void SaveBGMVolume(float value) //save BGM volume settings
    {
        PlayerPrefs.SetFloat("BGMVolume", value);
        PlayerPrefs.Save();
    }

    public void SaveSFXVolume(float value) //save SFX volume settings
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }

    public void SaveCurrentStage(int value) //save current stage the player is on
    {
        PlayerPrefs.SetInt("CurrentStage", value);
        PlayerPrefs.Save();
    }

    public void SaveCurrentCheckpoint(int value) //save current checkpoint the player is at
    {
        PlayerPrefs.SetInt("CurrentCheckPoint", value);
        PlayerPrefs.Save();
    }

    public void SaveInvetory(int value) //save player's inventory items 
    {
        //**to do
    }

    public void SaveMaxHealth(int value) //save player's max health 
    {
        PlayerPrefs.SetInt("MaxHealth", value);
        PlayerPrefs.Save();
    }

    public void SaveMaxDamage(int value) //save player's max damage 
    {
        PlayerPrefs.SetInt("MaxDamage", value);
        PlayerPrefs.Save();
    }
}
