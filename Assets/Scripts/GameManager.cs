using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI coinCounterText;
    [SerializeField] private TextMeshProUGUI healthCounterText;
    [SerializeField] private GameObject deathScreenPanel;
    [SerializeField] private GameObject pauseMenuPanel; // ⭐ NEW

    // --- Game State ---
    private int totalCoinsInLevel;
    private int coinsCollected;
    public static bool isPaused = false;
    [Header("Level Objects")]
    [SerializeField] private GameObject winScreenPanel; // ⭐ NEW
    [SerializeField] private WinGate winGate; // ⭐ NEW

    private void Awake()
    {
        // Set up the Singleton instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        deathScreenPanel.SetActive(false);
        winScreenPanel.SetActive(false);
        gameObject.SetActive(true);
        deathScreenPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        // Find all objects with the Coin script to get the total
        totalCoinsInLevel = FindObjectsOfType<Coin>().Length;
        coinsCollected = 0;

        // Set the initial UI text
        UpdateCoinUI();
    }
    public void IsGamePaused(bool ispause)
    {
        if (ispause)
        {
            ONOFF_GameUI(ispause);
            Time.timeScale = 0f;
            pauseMenuPanel.SetActive(true);
        }
        else
        {
            ONOFF_GameUI(ispause);
            pauseMenuPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
    [SerializeField] private GameObject _coinPanel;
    [SerializeField] private GameObject _healthPanel;
    [SerializeField] private GameObject _pauseBtnPanel;


    private void ONOFF_GameUI(bool pauseCondition)
    {
        if (pauseCondition)
        {
            if (_coinPanel) _coinPanel.SetActive(!pauseCondition);
            if (_healthPanel) _healthPanel.SetActive(!pauseCondition);
            if (_pauseBtnPanel) _pauseBtnPanel.SetActive(!pauseCondition);
        }
        else
        {
            if (_coinPanel) _coinPanel.SetActive(pauseCondition);
            if (_healthPanel) _healthPanel.SetActive(pauseCondition);
            if (_pauseBtnPanel) _pauseBtnPanel.SetActive(pauseCondition);
        }
    }
    public void ShowDeathScreen()
    {
        deathScreenPanel.SetActive(true);
    }
    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthCounterText != null)
        {
            healthCounterText.text = $"{currentHealth}";
        }
    }
    public void CollectCoin()
    {
        coinsCollected++;
        AudioManager.Instance.PlaySFX(SoundID.CoinCollected);
        UpdateCoinUI();

        // Check for win condition
        if (coinsCollected >= totalCoinsInLevel)
        {
            Debug.Log("LEVEL COMPLETE!");
            if (winGate != null)
            {
                winGate.ActivateGate();
                AudioManager.Instance.PlaySFX(SoundID.yay);
            }
        }
    }
    // ⭐ NEW: This function shows the win screen
    public void ShowWinScreen()
    {
        winScreenPanel.SetActive(true);
        Time.timeScale = 0f; // Pause the game
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartCurrentLevel();
        }
    }
    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateCoinUI()
    {
        if (coinCounterText != null)
        {
            coinCounterText.text = $"{coinsCollected}/{totalCoinsInLevel}";
        }
    }
    private Coroutine screenFreezeCoroutine;

    // ⭐ NEW: This public function can be called from any script.
    public void TriggerScreenFreeze(float duration)
    {
        // Stop any existing freeze to prevent conflicts
        if (screenFreezeCoroutine != null)
        {
            StopCoroutine(screenFreezeCoroutine);
        }
        screenFreezeCoroutine = StartCoroutine(ScreenFreezeCoroutine(duration));
    }

    // ⭐ NEW: This is the actual coroutine that handles the freeze.
    private IEnumerator ScreenFreezeCoroutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration); // Use Realtime to wait even when time is frozen
        Time.timeScale = 1f;
        screenFreezeCoroutine = null;
    }
}