using UnityEngine;

public class SpiderController : MonoBehaviour
{
    [Header("Wall Health")]
    public int hitsToFall = 3;
    [HideInInspector] public int currentWallHits = 0;
    public State wallFallState;
    public State wallHitState;
}