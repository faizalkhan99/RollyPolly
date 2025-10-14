using UnityEngine;

public class CollisionDamageDealer : MonoBehaviour
{
    [SerializeField] private int damageToDeal = 1;
    private EnemyStunHandler enemyStunHandler; // Reference to its own stun handler

    void Start()
    {
        enemyStunHandler = GetComponent<EnemyStunHandler>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Don't do anything if this enemy is already stunned
        if (enemyStunHandler != null && enemyStunHandler.IsStunned) return;

        // Try to damage the player
        if (collision.gameObject.CompareTag("Player"))
        {
            HealthSystem targetHealth = collision.gameObject.GetComponent<HealthSystem>();
            if (targetHealth != null)
            {
                RollingCuboidController player = collision.gameObject.GetComponent<RollingCuboidController>();
                if (player != null && player.IsSlamming)
                {
                    return; // Player is immune, their slam will stun us
                }
                
                targetHealth.TakeDamage(damageToDeal);

                // ⭐ MODIFIED: Stun self after dealing damage
                enemyStunHandler?.ApplyStun();
            }
        }
    }
}

