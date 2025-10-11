using System.Collections;
using UnityEngine;

public class StunHandler : MonoBehaviour
{
    [Header("State Durations")]
    [Tooltip("How long the object cannot act after being hit.")]
    [SerializeField] public float stunDuration = 0.5f;
    [Tooltip("How long the object cannot take damage after being hit.")]
    [SerializeField] public float invincibilityDuration = 1.0f;

    // Public properties that other scripts (like your player controller) can check
    public bool IsStunned { get; private set; }
    public bool IsInvincible { get; private set; }

    // This is called by the HealthSystem script
    public void ApplyStun()
    {
        // Stop any previous stun coroutine to reset timers if hit again
        StopAllCoroutines();
        StartCoroutine(StunCoroutine());
    }

    private IEnumerator StunCoroutine()
    {
        IsStunned = true;
        IsInvincible = true;

        float maxDuration = Mathf.Max(stunDuration, invincibilityDuration);
        float timer = 0f;

        while (timer < maxDuration)
        {
            timer += Time.deltaTime;

            // Check if the stun period has ended
            if (IsStunned && timer >= stunDuration)
            {
                IsStunned = false;
                Debug.Log($"{gameObject.name} is no longer stunned.");
            }

            // Check if the invincibility period has ended
            if (IsInvincible && timer >= invincibilityDuration)
            {
                IsInvincible = false;
                Debug.Log($"{gameObject.name} is no longer invincible.");
            }

            yield return null;
        }

        // Final cleanup to ensure states are reset
        IsStunned = false;
        IsInvincible = false;
    }
}
