using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("GameManager Settings")]
    public AudioClip[] levelBGM;
    public int currentLevel = 1;

    [Header("Wolf Boss Settings")]
    public bool isBossLevel;
    public WolfMovementScript wolfBoss;
    public Collider wolfBossTrigger;
    public Transform playerTransform;
    public GameObject bossHealthBar;
    public Scrollbar bossHealthScrollbar;
    public AudioClip bossBGM;
    public AudioClip bossPhase2BGM;
    public AudioClip[] bossSFX;

    private AudioSource audioSource;
    private bool isBossFightStarted, isBossDefeated, isPhase2BGMPlaying;

    // Awake is called when loading an instance of a script component
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (levelBGM.Length > 0 && currentLevel - 1 < levelBGM.Length)
        {
            audioSource.clip = levelBGM[currentLevel - 1];
            audioSource.loop = true;
            audioSource.Play();
        }

        if (isBossLevel && wolfBoss != null && wolfBossTrigger != null && bossHealthBar != null && bossHealthScrollbar != null)
            bossHealthBar.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isBossLevel || isBossDefeated) return;

        // Start the boss fight when the player steps into the boss trigger zone
        if (!isBossFightStarted && wolfBossTrigger.bounds.Contains(playerTransform.position))
        {
            isBossFightStarted = true;
            bossHealthBar.SetActive(true);
            wolfBoss.StartBossFight();
            audioSource.clip = bossBGM;
            audioSource.Play();
        }

        if (!isBossFightStarted) return;

        // Change BGM to phase 2 BGM as soon as the wolf enters phase 2 (what a mouthful)
        if (!isPhase2BGMPlaying && wolfBoss.CurrentPhase == 2)
        {
            isPhase2BGMPlaying = true;
            audioSource.clip = bossPhase2BGM;
            audioSource.Play();
        }

        // Update the scrollbar size to reflect the current health of the big bad wolf
        bossHealthScrollbar.size = (float)wolfBoss.health / wolfBoss.maxHealth;

        if (wolfBoss.IsDead)
        {
            isBossDefeated = true;
            bossHealthBar.SetActive(false);
            audioSource.clip = levelBGM[currentLevel - 1];
            audioSource.Play();

            Debug.Log("You have beaten the Big Bad Wolf!");
        }
    }

    public void PlaySFX(int index)
    {
        audioSource.PlayOneShot(bossSFX[index]);
    }
}