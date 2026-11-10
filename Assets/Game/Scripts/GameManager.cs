using UnityEngine;
using UnityEngine.SceneManagement; // Yeh namespace scene load karne ke liye zaroori hai
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [HideInInspector] public bool isGameOver = false;
    [HideInInspector] public int winnerRole = 0; // 1 = BlindMan, 2 = Hider
    public float matchTimer = 120f; // 2 minutes (120 seconds)

    [Header("UI References")]
    public GameObject gameOverPanel; // GameOverPanel yahan drag karna hai
    public TextMeshProUGUI resultText; // Result dikhane ke liye text
    public TextMeshProUGUI timerText; // Timer dikhane ke liye text reference

    [Header("Hiders Tracking")]
    public int totalHidersInScene = 0;
    private int caughtHidersCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterHider()
    {
        totalHidersInScene++;
    }

    public void OnHiderCaught()
    {
        caughtHidersCount++;
        Debug.Log("Hider Caught! Total caught: " + caughtHidersCount + " / " + totalHidersInScene);

        // Jab sare hiders pakre jayein tabhi BlindMan jeetega
        if (caughtHidersCount >= totalHidersInScene)
        {
            TriggerGameOver(1); // 1 = BlindMan Win
        }
    }

    private void Update()
    {
        if (!isGameOver)
        {
            if (matchTimer > 0)
            {
                matchTimer -= Time.deltaTime;

                // Timer ko har frame update karein
                UpdateTimerUI();

                if (matchTimer <= 0)
                {
                    matchTimer = 0;
                    Debug.Log("Match Timer Ended! Hider Survived. Hider Wins.");
                    TriggerGameOver(2);
                }
            }
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(matchTimer / 60f);
            int seconds = Mathf.FloorToInt(matchTimer % 60f);
            // MM:SS format mein dikhane ke liye
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void TriggerGameOver(int role)
    {
        if (isGameOver) return;

        winnerRole = role;
        isGameOver = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // GameOver Panel ko active kar dein
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (winnerRole == 1)
        {
            Debug.Log("=== GAME OVER: Blind Man Won! ===");
            if (resultText != null) resultText.text = "Blind Man Won!";
        }
        else if (winnerRole == 2)
        {
            Debug.Log("=== GAME OVER: Hider Won! ===");
            if (resultText != null) resultText.text = "Game Over Hider Won!";
        }
    }

    // --- BUTTON FUNCTIONS ---

    public void RestartGame()
    {
        // Active scene ko dobara load kar dega
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        // Apne Main Menu scene ka exact naam yahan likhein (misal ke tor par "MainMenu")
        SceneManager.LoadScene("Main Menu");
    }
}