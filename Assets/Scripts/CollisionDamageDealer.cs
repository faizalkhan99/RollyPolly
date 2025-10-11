using UnityEngine;

public class CollisionDamageDealer : MonoBehaviour
{
    [SerializeField] private int damageToDeal = 1;
    [SerializeField] private bool destroyOnImpact = true;

    private void OnCollisionEnter(Collision collision)
    {
        HealthSystem targetHealth = collision.gameObject.GetComponent<HealthSystem>();
        if (targetHealth != null)
        {
            // Special check to avoid dealing damage to the player if they are slamming
            RollingCuboidController player = collision.gameObject.GetComponent<RollingCuboidController>();
            if (player != null && player.IsSlamming)
            {
                return; // Do nothing, the slam will handle damage
            }
            
            targetHealth.TakeDamage(damageToDeal);

            if (destroyOnImpact)
            {
                Destroy(gameObject);
            }
        }
    }
}
