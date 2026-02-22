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

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

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

    private void Update()
    {
        aliveTimer += Time.deltaTime;
        if (aliveTimer >= chaseDuration)
        {
            Destroy(gameObject);
            return;
        }

        if (target != null)
        {
            Vector3 erraticOffset = new(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            Vector3 direction = ((target.position + erraticOffset) - transform.position).normalized;

            transform.position += speed * Time.deltaTime * direction;
            transform.LookAt(target.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            Destroy(gameObject);
        }
    }
}