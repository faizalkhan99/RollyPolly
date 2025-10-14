using System.Collections;
using UnityEngine;
using TMPro; // Needed for the warning text

public class WinGate : MonoBehaviour
{
    [Header("Gate Settings")]
    [Tooltip("The color the gate will glow when all coins are collected.")]
    [SerializeField] private Color activatedColor = Color.green;
    [SerializeField] private float glowIntensity = 2f;

    [Header("UI Feedback")]
    [Tooltip("The TextMeshPro object that displays 'Collect all coins first'.")]
    [SerializeField] private TextMeshProUGUI warningText;

    private Material gateMaterial;
    private bool isActivated = false;

    void Start()
    {
        // Get the material so we can change its color and glow
        gateMaterial = GetComponent<Renderer>().material;
        gateMaterial.EnableKeyword("_EMISSION"); // Ensure the glow property is usable
        gateMaterial.SetColor("_EmissionColor", Color.black); // Start with no glow

        // Hide the warning text at the start
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }
    }

    // This public function will be called by the GameManager when all coins are collected
    public void ActivateGate()
    {
        isActivated = true;
        // Set the material's emission color to make it glow green
        gateMaterial.SetColor("_EmissionColor", activatedColor * glowIntensity);
        Debug.Log("Win Gate has been activated!");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered is the player
        if (other.CompareTag("Player"))
        {
            if (isActivated)
            {
                // If the gate is active, tell the GameManager to show the win screen
                GameManager.Instance.ShowWinScreen();
            }
            else
            {
                // If the gate is not active, show the warning message
                StartCoroutine(ShowWarningMessage());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Hide the warning text when the player leaves the trigger area
        if (other.CompareTag("Player") && warningText != null)
        {
            StopAllCoroutines(); // Stop the warning message from disappearing on its own
            warningText.gameObject.SetActive(false);
        }
    }

    private IEnumerator ShowWarningMessage()
    {
        if (warningText != null)
        {
            warningText.gameObject.SetActive(true);
            // Wait for a few seconds before hiding the message automatically
            yield return new WaitForSeconds(3f);
            warningText.gameObject.SetActive(false);
        }
    }
}
