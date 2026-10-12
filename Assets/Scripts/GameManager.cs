
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public enum GameState
{
    beforeStart,
    gamePlay,
    gameOver
}

/// <summary>
/// Central game manager for score, game states,
/// stealable items, and falling item generation.
/// Only one instance should exist in the scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Coin / Stealable Item References")]
    public GameObject coinPrefab;
    public Transform coinParent;
    public TMP_Text scoreText;

    [Header("Falling Item References")]
    public GameObject fallingItemPrefab;
    public Transform fallingItemParent;

    [Header("Game State")]
    public GameState gameState = GameState.beforeStart;
    public int score = 10;
    public int level = 0;

    [Header("Falling Item Settings")]
    public bool generateCoins = true;
    public bool generateFallingItems = false;

    void Awake()
    {
        // Singleton: allow only one GameManager.
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Keep this manager when changing scenes, if desired.
        // Remove this line if each scene should have its own manager.
        DontDestroyOnLoad(gameObject);

        Config.coinsToGenerate = 1;

        score = 10;
        level = 0;
        gameState = GameState.beforeStart;

        UpdateScoreText();

        Debug.Log("Gatavs? Spied F, lai sāktu!");
    }

    void Update()
    {
        switch (gameState)
        {
            case GameState.beforeStart:
                if (Input.GetKeyDown(KeyCode.F))
                {
                    StartGame();
                }
                break;

            case GameState.gamePlay:
                // Add timer or difficulty progression here.
                break;

            case GameState.gameOver:
                if (Input.GetKeyDown(KeyCode.F))
                {
                    RestartGame();
                }
                break;
        }
    }

    public void StartGame()
    {
        if (gameState == GameState.gamePlay)
            return;

        gameState = GameState.gamePlay;

        CancelInvoke(nameof(GenerateItems));

        InvokeRepeating(
            nameof(GenerateItems),
            0f,
            Config.repeatRate
        );

        Debug.Log("Spēle sākusies!");
    }

    void GenerateItems()
    {
        if (gameState != GameState.gamePlay)
            return;

        // Generate stealable coins/items.
        if (generateCoins && coinPrefab != null)
        {
            for (int i = 0; i < Config.coinsToGenerate; i++)
            {
                SpawnItem(coinPrefab, coinParent);
            }
        }

        // Generate a separate type of falling item.
        if (generateFallingItems && fallingItemPrefab != null)
        {
            SpawnItem(fallingItemPrefab, fallingItemParent);
        }

        level++;

        // Increase the number of coins each wave.
        if (generateCoins)
        {
            Config.coinsToGenerate++;
        }
    }

    void SpawnItem(GameObject prefab, Transform parent)
    {
        Vector3 spawnPosition = new Vector3(
            Random.Range(Config.minDropX, Config.maxDropX),
            0f,
            0f
        );

        GameObject item = Instantiate(
            prefab,
            spawnPosition,
            Quaternion.identity,
            parent
        );

        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.AddForce(
                Random.insideUnitCircle * Config.dropForce,
                ForceMode2D.Impulse
            );
        }
    }

    public void AddScore()
    {
        if (gameState != GameState.gamePlay)
            return;

        score++;
        UpdateScoreText();
    }

    public void RemoveScore()
    {
        if (gameState != GameState.gamePlay)
            return;

        score--;
        UpdateScoreText();

        if (score <= 0)
        {
            GameOver();
        }
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    public void GameOver()
    {
        if (gameState == GameState.gameOver)
            return;

        CancelInvoke(nameof(GenerateItems));

        gameState = GameState.gameOver;

        Debug.Log(
            "Spēle beigusies! Tu izturēji līdz " +
            level +
            ". līmenim. Spied F, lai sāktu no jauna."
        );
    }

    public void RestartGame()
    {
        CancelInvoke(nameof(GenerateItems));
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
