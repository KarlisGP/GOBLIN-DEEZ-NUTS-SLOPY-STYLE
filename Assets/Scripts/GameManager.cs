
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public enum GameState
{
    beforeStart,
    gamePlay,
    gameOver,
    gameWon
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Water Timer Bar")]
    public Image waterFill;

    [Header("Prefab References")]
    public GameObject coinPrefab;
    public Transform coinParent;

    public GameObject skullPrefab;
    public Transform skullParent;

    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text resultText;

    [Header("Game Menu UI")]
    public GameObject startPanel;
    public GameObject winPanel;
    public GameObject gameOverPanel;

    [Header("Strike UI")]
    public Image[] strikeImages;
    public Color inactiveStrikeColor = Color.gray;
    public Color activeStrikeColor = Color.red;

    [Header("Spawn Settings")]
    public float coinSpawnRate = 2f;
    public float skullSpawnRate = 1.5f;

    [Range(0f, 0.4f)]
    public float horizontalSpawnMargin = 0.05f;

    [Range(1f, 2f)]
    public float spawnHeight = 1.15f;

    [Header("Game Rules")]
    public float gameDuration = 30f;
    public int maxMissedSkulls = 5;

    [Header("Game State")]
    public GameState gameState = GameState.beforeStart;
    public int score = 10;
    public int missedSkulls = 0;

    private float timeRemaining;
    private Camera mainCamera;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        mainCamera = Camera.main;

        score = 10;
        missedSkulls = 0;
        timeRemaining = gameDuration;
        gameState = GameState.beforeStart;

        if (waterFill != null)
            waterFill.fillAmount = 0f;

        UpdateScoreText();
        UpdateStrikeUI();

        if (resultText != null)
            resultText.text = "";

        // Show the start screen when the scene loads.
        ShowPanel(startPanel);
    }

    void Update()
    {
        switch (gameState)
        {
            case GameState.beforeStart:
                if (Input.GetKeyDown(KeyCode.F))
                    StartGame();
                break;

            case GameState.gamePlay:
                UpdateTimer();
                break;

            case GameState.gameOver:
            case GameState.gameWon:
                if (Input.GetKeyDown(KeyCode.F))
                    RestartGame();
                break;
        }
    }

    // Shows one menu panel and hides the others.
    void ShowPanel(GameObject panelToShow)
    {
        if (startPanel != null)
            startPanel.SetActive(panelToShow == startPanel);

        if (winPanel != null)
            winPanel.SetActive(panelToShow == winPanel);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(panelToShow == gameOverPanel);
    }

    public void StartGame()
    {
        if (gameState != GameState.beforeStart)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
            return;
        }

        timeRemaining = gameDuration;

        if (waterFill != null)
            waterFill.fillAmount = 0f;

        gameState = GameState.gamePlay;

        // Hide the start screen during gameplay.
        ShowPanel(null);

        if (resultText != null)
            resultText.text = "";

        InvokeRepeating(
            nameof(SpawnCoin),
            0f,
            Mathf.Max(0.1f, coinSpawnRate)
        );

        InvokeRepeating(
            nameof(SpawnSkull),
            0f,
            Mathf.Max(0.1f, skullSpawnRate)
        );
    }

    Vector3 GetTopSpawnPosition()
    {
        float x = Random.Range(
            horizontalSpawnMargin,
            1f - horizontalSpawnMargin
        );

        Vector3 viewportPosition = new Vector3(
            x,
            spawnHeight,
            -mainCamera.transform.position.z
        );

        Vector3 position =
            mainCamera.ViewportToWorldPoint(viewportPosition);

        position.z = 0f;
        return position;
    }

    void SpawnCoin()
    {
        if (gameState != GameState.gamePlay || coinPrefab == null)
            return;

        Instantiate(
            coinPrefab,
            GetTopSpawnPosition(),
            Quaternion.identity,
            coinParent
        );
    }

    void SpawnSkull()
    {
        if (gameState != GameState.gamePlay || skullPrefab == null)
            return;

        Instantiate(
            skullPrefab,
            GetTopSpawnPosition(),
            Quaternion.identity,
            skullParent
        );
    }

    void UpdateTimer()
    {
        timeRemaining -= Time.deltaTime;
        timeRemaining = Mathf.Max(0f, timeRemaining);

        // Fill the water bar over the duration of the game.
        float progress = gameDuration > 0f
            ? 1f - (timeRemaining / gameDuration)
            : 1f;

        if (waterFill != null)
            waterFill.fillAmount = progress;

        if (timeRemaining <= 0f)
            WinGame();
    }

    public void SkullPassed()
    {
        if (gameState != GameState.gamePlay)
            return;

        missedSkulls++;

        // Activate the next strike indicator.
        UpdateStrikeUI();

        Debug.Log(
            "Missed skulls: " + missedSkulls +
            " / " + maxMissedSkulls
        );

        if (missedSkulls >= maxMissedSkulls)
            LoseGame();
    }

    void UpdateStrikeUI()
    {
        if (strikeImages == null)
            return;

        for (int i = 0; i < strikeImages.Length; i++)
        {
            if (strikeImages[i] == null)
                continue;

            strikeImages[i].color =
                i < missedSkulls
                    ? activeStrikeColor
                    : inactiveStrikeColor;
        }
    }

    public void AddScore()
    {
        if (gameState != GameState.gamePlay)
            return;

        score++;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    void WinGame()
    {
        if (gameState != GameState.gamePlay)
            return;

        StopSpawning();
        gameState = GameState.gameWon;

        // Show the victory screen.
        ShowPanel(winPanel);

        if (resultText != null)
            resultText.text = "";

        Debug.Log("You survived the timer! You win!");
    }

    void LoseGame()
    {
        if (gameState != GameState.gamePlay)
            return;

        StopSpawning();
        gameState = GameState.gameOver;

        // Show the game-over screen.
        ShowPanel(gameOverPanel);

        if (resultText != null)
            resultText.text = "";

        Debug.Log("Too many skulls passed through. You lose!");
    }

    void StopSpawning()
    {
        CancelInvoke(nameof(SpawnCoin));
        CancelInvoke(nameof(SpawnSkull));
    }

    public void RestartGame()
    {
        StopSpawning();

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
