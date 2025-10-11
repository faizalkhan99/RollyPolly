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
    [SerializeField] private GameObject deathScreenPanel; // ⭐ NEW

    // --- Game State ---
    private int totalCoinsInLevel;
    private int coinsCollected;

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
        gameObject.SetActive(true);
        deathScreenPanel.SetActive(false);
        // Find all objects with the Coin script to get the total
        totalCoinsInLevel = FindObjectsOfType<Coin>().Length;
        coinsCollected = 0;

        // Set the initial UI text
        UpdateCoinUI();
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
        UpdateCoinUI();

        // Check for win condition
        if (coinsCollected >= totalCoinsInLevel)
        {
            // You've collected all the coins!
            Debug.Log("LEVEL COMPLETE!");
            // You can add code here to show a victory screen or load the next level
        }
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