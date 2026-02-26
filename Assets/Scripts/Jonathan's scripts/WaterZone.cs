using UnityEngine;

public class WaterZone : MonoBehaviour
{
    public FishEnemy fishEnemy;
    public float timeRequiredInWater = 5f;

    private float timeInWater = 0f;
    private bool isPlayerInWater = false;
    private bool hasFishSpawned = false;
    private void Update()
    {
        if (isPlayerInWater && !hasFishSpawned)
        {
            timeInWater += Time.deltaTime;

            if (timeInWater >= timeRequiredInWater)
            {
                SpawnFish();
            }
        }
    }
    private void SpawnFish()
    {
        hasFishSpawned = true;
        if (fishEnemy != null)
        {
            fishEnemy.Appear();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInWater = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInWater = false;
            timeInWater = 0f;
            hasFishSpawned = false;

            if (fishEnemy != null)
            {
                fishEnemy.Disappear();
            }
        }
    }
}