using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using NUnit.Framework;

public class PlayerCameraScript : MonoBehaviour
{
    [Header("Player Camera Settings")]
    public CinemachineCamera cinemachineCamera;
    public CinemachineCamera sideviewCamera;
    public PlayerInput playerInput;
    public PlayerAttackScript playerAttack;
    public Transform player;
    public Transform targetView;
    public float rotationSpeed = 5.0f;
    public float minZoomDistance = 1.0f;
    public float maxZoomDistance = 2.5f;

    [SerializeField] private GameObject sideviewonlyitems;
    [SerializeField] private GameObject normalviewonlyitems;

    private InputAction lookAction;
    private InputAction cameraswitchAction;
    private CinemachineOrbitalFollow orbitalFollow;
    private Vector3 initialTargetViewPosition;
    private bool wasAiming;
    private float mouseX, mouseY, currentZoomDistance, targetZoomDistance;
    public bool isSideview;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lookAction = playerInput.actions["Look"];
        cameraswitchAction = playerInput.actions["Special"];

        orbitalFollow = cinemachineCamera.GetComponent<CinemachineOrbitalFollow>();
        initialTargetViewPosition = targetView.localPosition;
        currentZoomDistance = maxZoomDistance;
        targetZoomDistance = maxZoomDistance;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //set sideview to false
        isSideview = false;

        //set the objects at the start
        sideviewonlyitems.SetActive(false);
        normalviewonlyitems.SetActive(true);
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

        if (cameraswitchAction.WasPressedThisFrame())
        {   //setting the bool
            if (isSideview)
            {
                isSideview = false;
            }
            else
            {
                isSideview = true;
            }
        }

        //for changing views
        if (isSideview)
        { //in side view
            sideviewCamera.Priority = 50;
            sideviewonlyitems.SetActive(true);
            normalviewonlyitems.SetActive(false);
        }
        else
        { //out of side view
            sideviewCamera.Priority = 10;
            sideviewonlyitems.SetActive(false);
            normalviewonlyitems.SetActive(true);
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