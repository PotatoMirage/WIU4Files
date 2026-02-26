using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerCameraScript : MonoBehaviour
{
    [Header("Player Camera Settings")]
    public CinemachineCamera cinemachineCamera;
    public PlayerInput playerInput;
    public PlayerAttackScript playerAttack;
    public Transform player;
    public Transform targetView;
    public float rotationSpeed = 5.0f;
    public float minZoomDistance = 1.0f;
    public float maxZoomDistance = 2.5f;

    private InputAction lookAction;
    private CinemachineOrbitalFollow orbitalFollow;
    private Vector3 initialTargetViewPosition;
    private bool wasAiming;
    private float mouseX, mouseY, currentZoomDistance, targetZoomDistance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lookAction = playerInput.actions["Look"];

        orbitalFollow = cinemachineCamera.GetComponent<CinemachineOrbitalFollow>();
        initialTargetViewPosition = targetView.localPosition;
        currentZoomDistance = maxZoomDistance;
        targetZoomDistance = maxZoomDistance;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            bool isAiming = playerAttack.IsAiming || playerAttack.IsFiring;
            Vector2 lookDelta = lookAction.ReadValue<Vector2>();
            mouseX += lookDelta.x * rotationSpeed * 0.02f;
            mouseY -= lookDelta.y * rotationSpeed * 0.02f;
            mouseY = Mathf.Clamp(mouseY, -60.0f, 45.0f);

            if (isAiming && !wasAiming)
                targetZoomDistance = minZoomDistance;
            else if (!isAiming && wasAiming)
                targetZoomDistance = maxZoomDistance;

            wasAiming = isAiming;
            orbitalFollow.HorizontalAxis.Value = mouseX;
            orbitalFollow.VerticalAxis.Value = mouseY;
        }
    }

    // LateUpdate is called once per physics frame
    void LateUpdate()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        currentZoomDistance = Mathf.Lerp(currentZoomDistance, targetZoomDistance, Time.deltaTime * 5.0f);

        Vector3 cameraOrigin = player.position + Vector3.up * initialTargetViewPosition.y;
        float offsetAmount = Mathf.InverseLerp(maxZoomDistance, minZoomDistance, currentZoomDistance) * 0.6f;

        targetView.SetPositionAndRotation(cameraOrigin + targetView.right * offsetAmount, Quaternion.Euler(0, mouseX, 0));
        orbitalFollow.Radius = currentZoomDistance;
    }
}