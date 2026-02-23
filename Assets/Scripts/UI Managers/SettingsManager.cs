using System.Data;
using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsManager : MonoBehaviour
{
    [Header("Sections")]
    [SerializeField] private GameObject Settings;
    [SerializeField] private GameObject Audio;
    [SerializeField] private GameObject KeyBinds;
    [SerializeField] private GameObject Mainsettings;
    [SerializeField] private GameObject Saving;

    //[Header("Getting Audio")]


    [Header("Keybind")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private int tactions = 2;
    [SerializeField] private TMP_Text Jump;
    [SerializeField] private TMP_Text Crouch;


    private InputAction[] action;
    //[SerializeField] private GameObject SaveandLeaveButton;


    private bool isAudio;
    private bool isKeybind;
    void Start()
    {
        //set settings to false on defualt
        isAudio = false;
        isKeybind = false;
        //SaveandLeaveButton.SetActive(false); (not complete - post settings)
    }

    void Awake()
    {
        action = new InputAction[tactions];

        //input actions
        action[0] = playerInput.actions["Jump"];
        action[1] = playerInput.actions["Crouch"];
    }

    void Update()
    {
        //check if in check point and change Save and Leave button accordingly
    


        //updating the keybinding text
        Jump.text = InputControlPath.ToHumanReadableString( action[0].bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
        Crouch.text = InputControlPath.ToHumanReadableString( action[1].bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);

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

    public void AudioOpen()
    {
        //open the audio settings
        Mainsettings.SetActive(false);
        Audio.SetActive(true);
        Saving.SetActive(true);

        isAudio = true;
        isKeybind = false;
    }

    public void KeybindsOpen()
    {
        //open the audio settings
        Mainsettings.SetActive(false);
        KeyBinds.SetActive(true);
        Saving.SetActive(true);

        isAudio = false;
        isKeybind = true;
    }

    public void Save()
    {
        if (isAudio)
        {
            //save audio
        }
        else if (isKeybind)
        {
            //save keybind
            //PlayerSave.Instance.SaveKeybinds(playerInput.actions.SaveBindingOverridesAsJson());
        }

        //return to menu
        Audio.SetActive(false);
        Saving.SetActive(false);
        KeyBinds.SetActive(false);
        Mainsettings.SetActive(true);
    }

    public void reseting()
    {
        if (isAudio)
        {
            //reset audio
        }
        else if (isKeybind)
        {
            //reset keybind
        }
    }

    public void Rebinding(int actionindex)
    {
        action[actionindex].Disable();

        //rebinding
        action[actionindex].PerformInteractiveRebinding(0).OnComplete(operation =>
        {
            action[actionindex].Enable();
            operation.Dispose();

            if (CheckConflict(actionindex))
            {
                Debug.Log("Conflicting bindings");
                action[actionindex].RemoveBindingOverride(0);
            }

            
        }).Start();
    }

    private bool CheckConflict(int actionindex)
    {
        //get the name of current path
        string Bindingpath = action[actionindex].bindings[0].effectivePath;

        foreach (var actions in playerInput.actions.actionMaps)
        { //get total actions in the action maps for player input
            foreach (var otherAction in actions.actions)
            { //check the current action with all the actions in the map
                if (otherAction == action[actionindex])
                { //skip the same action
                    continue;
                }
                
                foreach (var binding in otherAction.bindings)
                { //get the bindings for each action that is correct
                    if (binding.effectivePath == Bindingpath)
                    { //if matches then return true for conflict
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
