using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerMovementScript playerMovement;
    public Slider healthSlider;
    public float lerpSpeed = 5.0f;

    public void Start()
    {
        playerMovement = UnityEngine.Object.FindAnyObjectByType<PlayerMovementScript>();

        if (playerMovement != null && healthSlider != null)
        {
            healthSlider.maxValue = playerMovement.maxHealth;
            healthSlider.value = playerMovement.health;
        }
    }

    public void Update()
    {
        if (playerMovement != null && healthSlider != null)
        {
            healthSlider.maxValue = playerMovement.maxHealth;
            healthSlider.value = Mathf.Lerp(healthSlider.value, playerMovement.health, lerpSpeed * Time.deltaTime);
        }
    }
}