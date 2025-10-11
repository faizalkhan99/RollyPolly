using System.Collections;
using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    // --- Singleton Setup ---
    public static SmoothCameraFollow Instance { get; private set; }

    [Header("Target & Positioning")]
    public Transform target;

    [Header("Smoothing")]
    public float smoothSpeed = 0.125f;

    private bool isShaking = false;

    void Awake()
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
    private Vector3 offset;

    void Start()
    {
        // Calculate the initial offset from the target when the game starts
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }
    void LateUpdate()
    {
        // Don't follow the target if the camera is shaking
        if (target == null || isShaking) return;

        // Ensure we have a target to follow
        if (target == null) return;

        // Calculate the desired position for the camera
        Vector3 desiredPosition = target.position + offset;

        // Smoothly move the camera from its current position to the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Apply the new position to the camera
        transform.position = smoothedPosition;
    }

    // ⭐ NEW: This function can be called from any script to start the shake
    public void StartShake(float duration, float magnitude)
    {
        StartCoroutine(Shake(duration, magnitude));
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        isShaking = true;
        Vector3 originalPos = transform.position;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // ⭐ MODIFIED: This check now works even if the player is just disabled.
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                isShaking = false;
                yield break; // Stop the coroutine immediately
            }

            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.position = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // This part will now only be reached if the shake completes normally.
        transform.position = originalPos;
        isShaking = false;
    }
}