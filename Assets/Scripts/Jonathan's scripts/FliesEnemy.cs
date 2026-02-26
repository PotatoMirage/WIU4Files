using System.Collections;
using UnityEngine;

public class FliesEnemy : MonoBehaviour
{
    public float speed = 5f;
    public float chaseDuration = 4f;
    public int damageAmount = 1;
    public float scaleDuration = 0.5f;
    public LayerMask playerLayer;

    private Transform target;
    private float aliveTimer = 0f;
    public Sprite debuffIcon;
    public string debuffName = "FlySlow";
    private PlayerMovementScript targetPlayer;

    private void Start()
    {
        transform.localScale = Vector3.zero;
        StartCoroutine(ScaleUpRoutine());
    }

    private IEnumerator ScaleUpRoutine()
    {
        float elapsedTime = 0f;
        Vector3 targetScale = Vector3.one;

        while (elapsedTime < scaleDuration)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, elapsedTime / scaleDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            targetPlayer = target.GetComponent<PlayerMovementScript>();
        }
    }

    private void Update()
    {
        aliveTimer += Time.deltaTime;

        if (targetPlayer != null && targetPlayer.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        if (aliveTimer >= chaseDuration)
        {
            Destroy(gameObject);
            return;
        }

        if (target != null)
        {
            Vector3 erraticOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            Vector3 direction = ((target.position + erraticOffset) - transform.position).normalized;

            transform.position += speed * Time.deltaTime * direction;
            transform.LookAt(target.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            PlayerMovementScript player = other.GetComponent<PlayerMovementScript>();
            if (player != null && !player.IsDead)
            {
                player.ApplyDebuff(3f, 0.5f, debuffIcon, debuffName);
            }

            Destroy(gameObject);
        }
    }
}