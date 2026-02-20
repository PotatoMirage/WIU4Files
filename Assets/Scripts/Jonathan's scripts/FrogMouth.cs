using UnityEngine;

public class FrogMouth : MonoBehaviour
{
    public FrogEnemy frogEnemy;

    public void TakeDamage(int damage)
    {
        if (frogEnemy != null)
        {
            frogEnemy.TakeDamage(damage);
        }
    }
}