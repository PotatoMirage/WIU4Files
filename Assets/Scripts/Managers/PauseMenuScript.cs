using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuScript : MonoBehaviour
{
    public PlayerInput playerInput;
    public GameObject pauseMenu;

    private InputAction escapeAction;
    private bool isPaused;

    // Awake is called when loading an instance of a script component
    void Awake()
    {
        escapeAction = playerInput.actions["Escape"];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pauseMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (escapeAction.WasPressedThisFrame())
            SetPaused(!isPaused);
    }

    void SetPaused(bool paused)
    {
        isPaused = paused;
        pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0.0f : 1.0f;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }
}