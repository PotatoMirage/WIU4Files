using System.Collections;
using UnityEngine;

public class FishEnemy : MonoBehaviour
{
    public GameObject fishGO;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Animator animator;

    public float moveSpeed = 4f;
    public int attackDamage = 10;
    public int bleedDamagePerTick = 2;
    public int bleedTicks = 5;
    public float bleedTickInterval = 1f;

    private bool isChasing = false;
    private bool isBleeding = false;
    private void Awake()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTarget = player.transform;
        }

        if (fishGO != null)
        {
            fishGO.SetActive(false);
        }
    }
    private void Update()
    {
        if (isChasing && playerTarget != null)
        {
            RotateTowardsPlayer();
            MoveTowardsPlayer();
        }
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
    private void MoveTowardsPlayer()
    {
        transform.position = Vector3.MoveTowards(transform.position, playerTarget.position, moveSpeed * Time.deltaTime);
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
        if (!isBleeding)
        {
            StartCoroutine(ApplyBleedEffect());
        }
    }
    private IEnumerator ApplyBleedEffect()
    {
        isBleeding = true;

        if (playerTarget != null)
        {
            playerTarget.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        }

        for (int i = 0; i < bleedTicks; i++)
        {
            yield return new WaitForSeconds(bleedTickInterval);
            if (playerTarget != null)
            {
                playerTarget.SendMessage("TakeDamage", bleedDamagePerTick, SendMessageOptions.DontRequireReceiver);
            }
        }

        isBleeding = false;
    }
    public void Appear()
    {
        if (fishGO != null)
        {
            fishGO.SetActive(true);
        }
        isChasing = true;
    }
    public void Disappear()
    {
        if (fishGO != null)
        {
            fishGO.SetActive(false);
        }
        isChasing = false;
        isBleeding = false;
        StopAllCoroutines();
    }
}