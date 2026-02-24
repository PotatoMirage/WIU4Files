using UnityEngine;
using System.Collections;

public class EnemyDissolve : MonoBehaviour
{
    [Header("Dissolve Settings")]
    public float delayBeforeDissolve = 0f;  // Start immediately with death
    public float dissolveDuration = 2f;

    [Header("VFX")]
    public EnemyVFX enemyVFX;

    private Renderer[] renderers;          // All renderers of the enemy
    private Material[][] materialInstances; // Materials for each renderer

    private static readonly int DissolveID = Shader.PropertyToID("_DissolveAmount");

    private void Awake()
    {
        if (enemyVFX == null)
            enemyVFX = GetComponent<EnemyVFX>();

        // Get all renderers in children (supports SkinnedMeshRenderer + MeshRenderer)
        renderers = GetComponentsInChildren<Renderer>();
        materialInstances = new Material[renderers.Length][];

        // Make a copy of all materials and reset dissolve to 0
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            materialInstances[i] = new Material[mats.Length];

            for (int j = 0; j < mats.Length; j++)
            {
                // Create instance so we don’t affect shared materials
                materialInstances[i][j] = mats[j];
                materialInstances[i][j] = new Material(materialInstances[i][j]);
                materialInstances[i][j].SetFloat(DissolveID, 0f);
            }

            renderers[i].materials = materialInstances[i];
        }
    }

    public void StartDissolve()
    {
        // Spawn death particles immediately
        if (enemyVFX != null && enemyVFX.deathParticlesEffect != null)
        {
            Instantiate(
                enemyVFX.deathParticlesEffect,
                transform.position,
                Quaternion.identity
            );
        }

        StartCoroutine(DissolveRoutine());
    }

    private IEnumerator DissolveRoutine()
    {
        if (delayBeforeDissolve > 0f)
            yield return new WaitForSeconds(delayBeforeDissolve);

        float timer = 0f;

        while (timer < dissolveDuration)
        {
            timer += Time.deltaTime;
            float dissolveValue = Mathf.Clamp01(timer / dissolveDuration);

            // Apply dissolve to all materials
            for (int i = 0; i < materialInstances.Length; i++)
            {
                for (int j = 0; j < materialInstances[i].Length; j++)
                {
                    materialInstances[i][j].SetFloat(DissolveID, dissolveValue);
                }
            }

            yield return null;
        }

        // Ensure full dissolve
        for (int i = 0; i < materialInstances.Length; i++)
        {
            for (int j = 0; j < materialInstances[i].Length; j++)
            {
                materialInstances[i][j].SetFloat(DissolveID, 1f);
            }
        }

        // Destroy the enemy GameObject
        Destroy(gameObject);
    }
}