using UnityEngine;

public class FishEnemy : MonoBehaviour
{
    //Fish appears when player stays in the water over 5 seconds, fish will then attack the player and make the player bleed over time.
    public GameObject fishGO;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Animator animator;
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
        RotateTowardsPlayer();
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
        Debug.Log("attackedplayer!");
        //Deal damage + dot
    }
    private void RotateTowardsPlayer()
    {
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }
}
