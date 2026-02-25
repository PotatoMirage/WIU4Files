using UnityEngine;
using System.Collections;

public class EnemyDissolve : MonoBehaviour
{
    public float delayBeforeDissolve; // Start immediately with death
    public float dissolveDuration;
    public EnemyVFX enemyVFX;

    private Renderer[] renderers;  // All renderers of the enemy
    private Material[][] materialInstances;// Materials for each renderer

    private static readonly int DissolveID = Shader.PropertyToID("_DissolveAmount");

    private void Awake()
    {
        bool hasNoVFX = enemyVFX == null;
        if (hasNoVFX)
        {
            EnemyVFX foundVFX = GetComponent<EnemyVFX>();
            enemyVFX = foundVFX;
        }

        Renderer[] foundRenderers = GetComponentsInChildren<Renderer>();
        renderers = foundRenderers;

        int renderersLength = renderers.Length;
        Material[][] newMaterialInstances = new Material[renderersLength][];
        materialInstances = newMaterialInstances;

        for (int i = 0; i < renderersLength; i++)
        {
            Renderer currentRenderer = renderers[i];
            Material[] mats = currentRenderer.materials;
            int matsLength = mats.Length;

            Material[] instanceArray = new Material[matsLength];
            materialInstances[i] = instanceArray;

            for (int j = 0; j < matsLength; j++)
            {
                Material originalMat = mats[j];
                Material newMat = new Material(originalMat);
                materialInstances[i][j] = newMat;

                float initialDissolve = 0f;
                materialInstances[i][j].SetFloat(DissolveID, initialDissolve);
            }

            currentRenderer.materials = materialInstances[i];
        }
    }
    private void OnEnable()
    {
        bool hasMaterials = materialInstances != null;
        if (hasMaterials)
        {
            int renderersLength = materialInstances.Length;
            for (int i = 0; i < renderersLength; i++)
            {
                Material[] currentMatArray = materialInstances[i];
                int matsLength = currentMatArray.Length;

                for (int j = 0; j < matsLength; j++)
                {
                    Material currentMat = currentMatArray[j];
                    float zeroDissolve = 0f;
                    currentMat.SetFloat(DissolveID, zeroDissolve);
                }
            }
        }
    }
    public void StartDissolve()
    {
        bool hasVFX = enemyVFX != null;
        if (hasVFX)
        {
            GameObject deathParticles = enemyVFX.deathParticlesEffect;
            bool hasDeathParticles = deathParticles != null;

            if (hasDeathParticles)
            {
                Transform currentTransform = transform;
                Vector3 currentPosition = currentTransform.position;
                Quaternion defaultRotation = Quaternion.identity;

                Instantiate(deathParticles, currentPosition, defaultRotation);
            }
        }

        IEnumerator dissolveCoroutine = DissolveRoutine();
        StartCoroutine(dissolveCoroutine);
    }
    private IEnumerator DissolveRoutine()
    {
        bool hasDelay = delayBeforeDissolve > 0f;
        if (hasDelay)
        {
            WaitForSeconds waitDelay = new WaitForSeconds(delayBeforeDissolve);
            yield return waitDelay;
        }

        float timer = 0f;

        while (timer < dissolveDuration)
        {
            float deltaTime = Time.deltaTime;
            timer += deltaTime;

            float rawDissolveValue = timer / dissolveDuration;
            float dissolveValue = Mathf.Clamp01(rawDissolveValue);

            int renderersLength = materialInstances.Length;
            for (int i = 0; i < renderersLength; i++)
            {
                Material[] currentMatArray = materialInstances[i];
                int matsLength = currentMatArray.Length;

                for (int j = 0; j < matsLength; j++)
                {
                    Material currentMat = currentMatArray[j];
                    currentMat.SetFloat(DissolveID, dissolveValue);
                }
            }

            yield return null;
        }

        int finalRenderersLength = materialInstances.Length;
        for (int i = 0; i < finalRenderersLength; i++)
        {
            Material[] currentMatArray = materialInstances[i];
            int matsLength = currentMatArray.Length;

            for (int j = 0; j < matsLength; j++)
            {
                Material currentMat = currentMatArray[j];
                float fullDissolve = 1f;
                currentMat.SetFloat(DissolveID, fullDissolve);
            }
        }

        EnemyHealth healthComponent = GetComponent<EnemyHealth>();
        bool hasHealthComponent = healthComponent != null;

        if (hasHealthComponent)
        {
            healthComponent.HandleDeath();
        }
    }
}