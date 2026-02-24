using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float visibilityDistance = 15f;

    private Camera mainCamera;
    private Transform playerTransform;
    private void Start()
    {
        mainCamera = Camera.main;
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }
    private void LateUpdate()
    {
        if (mainCamera == null || playerTransform == null)
        {
            return;
        }

        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= visibilityDistance)
        {
            canvasGroup.alpha = 1f;
        }
        else
        {
            canvasGroup.alpha = 0f;
        }
    }
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        healthSlider.value = currentHealth / maxHealth;
    }
}