using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public PlayerInput playerInput;
    public PlayerMovementScript playerMovement;

    private InputAction interactAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactAction = playerInput.actions["Interact"];
    }

    // Update is called once per frame
    void Update()
    {
        // Press "F" to instantly kill the player for testing purposes
        if (interactAction.WasPressedThisFrame())
            playerMovement.ChangeHealth(-playerMovement.maxHealth);
    }
}