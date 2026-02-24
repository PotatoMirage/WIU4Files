using UnityEngine;

public class PlayerSave : MonoBehaviour
{
    public static PlayerSave Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
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
        if (!PlayerPrefs.HasKey("MasterVolume") && !PlayerPrefs.HasKey("BGMVolume"))
        {
            resetAudio();
        }
        
        if (!PlayerPrefs.HasKey("CurrentStage"))
        {
            resetprogress();
        }
    }

    public void resetkeybinds(string value)
    {
        SaveKeybinds(value);
    }

    public void resetAudio() //reset the audio to the origional
    {
        SaveMasterVolume(1f);
        SaveBGMVolume(1f);
    }


    public void resetprogress() //restart progress
    {
        SaveCurrentCheckpoint(-1);
        SaveCurrentStage(0);
        SaveMaxHealth(100);
        SaveMaxDamage(2);
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

    public void SaveKeybinds(string value) //save keybind settings
    {
        PlayerPrefs.SetString("KeyBinds", value);
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

    

    public float GetMasterVolume() //get master volume
    {
        float value = PlayerPrefs.GetFloat("MasterVolume");
        return value;
    }

    public float GetBGMVolume() //get BGM volume
    {
        float value = PlayerPrefs.GetFloat("BGMVolume");
        return value;
    }

    public string GetKeyBinds()
    {
        string value = PlayerPrefs.GetString("KeyBinds");
        return value;
    }

    public int GetCurrentStage()
    {
        int value = PlayerPrefs.GetInt("CurrentStage");
        return value;
    }

    public int GetCurrentCheckpoint()
    {
        int value = PlayerPrefs.GetInt("CurrentCheckPoint");
        return value;
    }

    public int GetInventory()
    {
        //int value = PlayerPrefs.GetInt("CurrentStage");
        int value  = 0;
        return value;
    }

    public int GetMaxHealth()
    {
        int value = PlayerPrefs.GetInt("MaxHealth");
        return value;
    }

    public int GetMaxDamage()
    {
        int value = PlayerPrefs.GetInt("MaxDamage");
        return value;
    }
}
