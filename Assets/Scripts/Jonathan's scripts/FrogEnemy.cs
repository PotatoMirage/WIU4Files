using UnityEngine;

public class FrogEnemy : MonoBehaviour
{
    public GameObject frogGO;
    [SerializeField] private Transform playerTarget;

    private void Awake()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTarget = player.transform;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            AttackPlayer();
        }
    }
    private void AttackPlayer()
    {
        //Deal dmg via tongue ranged attack
    }
}
