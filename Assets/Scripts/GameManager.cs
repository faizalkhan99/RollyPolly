using UnityEngine;
using TMPro; // Add this namespace to use TextMeshPro

public class GameManager : MonoBehaviour
{
    // --- Singleton Setup ---
    public static GameManager Instance { get; private set; }

    // --- UI ---
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI coinCounterText;

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
        // Find all objects with the Coin script to get the total
        totalCoinsInLevel = FindObjectsOfType<Coin>().Length;
        coinsCollected = 0;

        // Set the initial UI text
        UpdateCoinUI();
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

    private void UpdateCoinUI()
    {
        if (coinCounterText != null)
        {
            coinCounterText.text = $"{coinsCollected}/{totalCoinsInLevel}";
        }
    }
}