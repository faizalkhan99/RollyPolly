using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyStunHandler : MonoBehaviour
{
    [Header("Stun Settings")]
    [SerializeField] private float stunDuration;
    [Tooltip("The 'seeing stars' particle effect that plays while stunned.")]
    [SerializeField] private ParticleSystem stunVFX;

    public bool IsStunned { get; private set; }

    private Rigidbody rb;
    private bool wasKinematic;
    private Quaternion originalRotation; // ⭐ NEW: To store the rotation before the stun

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (stunVFX != null)
        {
            stunVFX.Stop();
        }
    }

    public void ApplyStun()
    {
        if (IsStunned) return;
        StartCoroutine(StunCoroutine());
    }

    private IEnumerator StunCoroutine()
    {
        IsStunned = true;

        // --- Store the enemy's current state ---
        wasKinematic = rb.isKinematic;
        originalRotation = transform.rotation; // ⭐ Store the current rotation

        // --- Apply the stun effects ---
        rb.isKinematic = true; // Ignore physics forces
        
        // ⭐ MODIFIED: Correctly apply a 60-degree tilt on the Z-axis
        transform.rotation = Quaternion.Euler(0, 0, -60);

        if (stunVFX != null)
        {
            stunVFX.Play();
        }

        // --- Wait for the stun to end ---
        yield return new WaitForSeconds(stunDuration);

        // --- Restore the enemy's original state ---
        if (stunVFX != null)
        {
            stunVFX.Stop();
        }
        
        rb.isKinematic = wasKinematic;
        transform.rotation = originalRotation; // ⭐ Restore the exact rotation from before the stun
        IsStunned = false;
    }
}

