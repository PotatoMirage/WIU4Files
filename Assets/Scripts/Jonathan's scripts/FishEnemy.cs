using UnityEngine;

public class FishEnemy : MonoBehaviour
{
    //Fish appears when player stays in the water over 5 seconds, fish will then attack the player and make the player bleed over time.
    public GameObject fishGO;
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
        if(other.gameObject.CompareTag("Player"))
        {
            AttackPlayer();
        }
    }
    private void AttackPlayer()
    {
        //Deal damage + dot
    }
}
