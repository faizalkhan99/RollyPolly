using System.Collections;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [Header("Core Stats")]
    [SerializeField] private int maxHealth = 5;
    [Tooltip("The damage this object deals when it collides with another object that has a HealthSystem.")]
    [SerializeField] private int damageToDeal = 1;

    [Header("Impact Feedback")]
    [Tooltip("How long the screen freezes when this object takes damage.")]
    [SerializeField] private float screenFreezeDuration = 0.05f;
    [Tooltip("The color the object flashes when it takes damage.")]
    [SerializeField] private Color damageFlashColor = Color.red;

    [SerializeField] private int currentHealth;
    private Material objectMaterial;
    private Color originalColor;
    private StunHandler stunHandler; // Reference to the separate stun script
    private bool isPlayer = false;

    private bool levelIsActive = false; // ⭐ NEW: Grace period flag
    // ⭐ NEW: Start() is now used to begin the level after a delay
    void Start()
    {
        if (CompareTag("Player"))
        {
            isPlayer = true;
            GameManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
        }
        // Start a coroutine to activate the level after a very short delay
        StartCoroutine(StartLevelGracePeriod());
    }

    private IEnumerator StartLevelGracePeriod()
    {
        yield return new WaitForSeconds(0.1f); // Wait for 0.1 seconds
        levelIsActive = true;
    }
    void Awake()
    {
        currentHealth = maxHealth;
        stunHandler = GetComponent<StunHandler>(); // Get the StunHandler on this object
        // Get the material for the flash effect
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            objectMaterial = renderer.material;
            originalColor = objectMaterial.color;
        }

        
    }

    // This is the main function other scripts will call
    public void TakeDamage(int damage)
    {
        if (stunHandler != null && stunHandler.IsInvincible) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        if (CompareTag("Player"))
        {
            GameManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
        }
        // --- Player-Specific Feedback ---
        if (gameObject.CompareTag("Player"))
        {
            SmoothCameraFollow.Instance.StartShake(0.1f, 0.05f);

            // ⭐ MODIFIED: Call the safe, centralized screen freeze in the GameManager
            GameManager.Instance.TriggerScreenFreeze(screenFreezeDuration);
        }

        // --- Universal Feedback ---
        StartCoroutine(FlashEffect());
        stunHandler?.ApplyStun();

        if (currentHealth <= 0) Die();
    }


    private void OnCollisionEnter(Collision collision)
    {
        // Only run this logic if the object dealing damage is an Enemy
        if (!levelIsActive || !gameObject.CompareTag("Enemy"))
        {
            return;
        }

        HealthSystem targetHealth = collision.gameObject.GetComponent<HealthSystem>();
        if (targetHealth == null || targetHealth == this)
        {
            return;
        }

        // ⭐ NEW: Check if the object we hit is the player and if they are slamming
        RollingCuboidController playerController = collision.gameObject.GetComponent<RollingCuboidController>();
        if (playerController != null && playerController.IsSlamming)
        {
            return;
        }

        // If the check fails (it's not the player, or the player isn't slamming), deal damage.
        targetHealth.TakeDamage(damageToDeal);
        Destroy(gameObject);
    }

    private IEnumerator FlashEffect()
    {
        if (objectMaterial == null) yield break; // Do nothing if there's no material

        // The stun handler's invincibility duration controls how long we flash
        float duration = stunHandler != null ? stunHandler.invincibilityDuration : 0.1f;
        float flashTime = 0.1f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            objectMaterial.color = damageFlashColor;
            yield return new WaitForSeconds(flashTime);
            objectMaterial.color = originalColor;
            yield return new WaitForSeconds(flashTime);
            elapsedTime += (flashTime * 2);
        }
        objectMaterial.color = originalColor; // Ensure it resets
    }

    [Header("Death Feedback (Optional)")]
    [SerializeField] private ParticleSystem deathVFX;

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");

        // If this is the player, trigger the death screen and disable the object
        if (CompareTag("Player"))
        {
            GameManager.Instance.ShowDeathScreen();
            gameObject.SetActive(false); // Disable instead of destroying
        }
        else // If it's an enemy
        {
            Destroy(gameObject); // Enemies can still be destroyed
        }
    }

}
