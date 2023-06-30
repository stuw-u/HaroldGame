using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] PlayerController player;
    [SerializeField] CameraThirdPerson cameraThirdPerson;
    [SerializeField] Transform playerSpawnLocation;
    [SerializeField] Collider playerCollider;
    [SerializeField] WeaponAsset[] weaponCollection;
    [SerializeField] CrateSpawner crateSpawner;
    [SerializeField] Material deathEffect;
    [SerializeField] EnemyManager enemyManager;

    private bool isPlayerDead = false;
    private bool isGameStarted = false;
    private bool isEnemySpawningAllowed = false;
    private bool isPaused = false;
    private float deadAnimValue;
    private int lastWeaponId = -1;
    private int maxScore = 0;
    private int score = 0;
    private List<WeaponAsset> sortedWeapons;

    static GameManager inst;
    private void Awake ()
    {
        inst = this;

        sortedWeapons = weaponCollection.ToList();
        sortedWeapons.Sort((a, b) => a.scoreToUnlock.CompareTo(b.scoreToUnlock));

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    private void OnDestroy ()
    {
        inst = null;
        deathEffect.SetFloat("_Fade", 0);
    }

    private void Update ()
    {
        deadAnimValue = Mathf.Clamp01(deadAnimValue + (isPlayerDead ? 1 : -4) * Time.deltaTime);
        deathEffect.SetFloat("_Fade", deadAnimValue);

        if (IsPlayerInControl != !Cursor.visible)
        {
            Cursor.lockState = IsPlayerInControl ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !IsPlayerInControl;
        }

        if(Input.GetKeyDown(KeyCode.Escape) && !isPaused && !isPlayerDead)
        {
            isPaused = true;
            Time.timeScale = 0;

            UIManager.OpenPauseMenu();
        }
    }

    public static void Unpause ()
    {
        inst.isPaused = false;
        Time.timeScale = 1;
    }

    public static void OnPlayerDie ()
    {
        Debug.Log("Player killed!!: " + inst.player.transform.position);
        AudioManager.Play(AudioClipName.PlayerDepthScream, Vector3.zero, true, 1.1f, 0.4f);

        inst.isPlayerDead = true;
        int highScore = PlayerPrefs.GetInt("Highscore", 0);
        if(inst.score >= highScore)
        {
            PlayerPrefs.SetInt("Highscore", inst.score);
        }
        UIManager.OnGameOver(inst.score, highScore);
    }

    public static void StartGame ()
    {
        inst.ResetGame_Internal(true);
    }

    public static void ResetGame () { if (inst != null) { inst.ResetGame_Internal(false); } }
    void ResetGame_Internal (bool firstTime)
    {
        GroundSystem.Clear();

        Debug.Log("Game reset!!");
        score = 0;
        if (!firstTime)
        {
            player.Clear(playerSpawnLocation.position, playerSpawnLocation.forward);
        }
        isGameStarted = true;
        isEnemySpawningAllowed = false;
        enemyManager.Clear();
        crateSpawner.Clear();
        if(!firstTime) cameraThirdPerson.Clear(playerSpawnLocation.forward);
        isPlayerDead = false;
    }

    public static void CollectCrate ()
    {
        inst.enemyManager.OnCollectCrate();
        inst.score++;
        UIManager.SetScore(inst.score);
        inst.maxScore = Mathf.Max(inst.maxScore, inst.score);

        if(inst.score >= 0 && !inst.isEnemySpawningAllowed)
        {
            inst.isEnemySpawningAllowed = true;
        }
        
        inst.crateSpawner.SpawnCrate();
    }

    public static WeaponAsset GetRandomWeapon ()
    {
        if(inst.sortedWeapons.Count == 0) { return inst.sortedWeapons[0]; }

        int newWeaponId = Random.Range(0, inst.sortedWeapons.Count);
        while(newWeaponId == inst.lastWeaponId)
        {
            newWeaponId = Random.Range(0, inst.sortedWeapons.Count);
        }
        inst.lastWeaponId = newWeaponId;

        return inst.sortedWeapons[newWeaponId];
    }

    public static PlayerController Player => inst.player;
    public static Collider PlayerCollider => inst.playerCollider;
    public static bool ShouldMusicPlay => inst != null && inst.isEnemySpawningAllowed && !inst.isPlayerDead && !inst.isPaused;
    public static bool IsPlayerInControl => inst != null && inst.isGameStarted && !inst.isPlayerDead && !inst.isPaused;
    public static bool IsGameStarted => inst != null && inst.isGameStarted;
    public static bool IsEnemySpawningAllowed => inst != null && inst.isGameStarted && inst.isEnemySpawningAllowed;
}
