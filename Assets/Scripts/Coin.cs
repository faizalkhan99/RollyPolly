using UnityEngine;

public class Coin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered the trigger is the player
        if (other.CompareTag("Player"))
        {
            // Find the GameManager in the scene and call the CollectCoin method
            GameManager.Instance.CollectCoin();

            // Optional: Instantiate effect and play sound here

            // Destroy the coin object
            Destroy(gameObject);
        }
    }
}
