using System.Collections;
using UnityEngine;

public class RollingCuboidController : MonoBehaviour
{
    // ... (Your Movement header and variables are unchanged)
    [Header("Movement")]
    [SerializeField] private float rollDuration = 0.25f;
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    // ⭐ MODIFIED: Added slamDuration
    [Header("Slam Attack")]
    [SerializeField] private float chargeTime = 1.5f;
    [SerializeField] private float slamDuration = 0.1f; // How fast the slam move is
    [SerializeField] private Color maxGlowColor = Color.white;
    [SerializeField] private float maxGlowIntensity = 2f;

    // ⭐ MODIFIED: Added chargingVFX
    [Header("Slam VFX")]
    [SerializeField] private ParticleSystem chargingVFX; // Plays while charging up
    [SerializeField] private ParticleSystem chargeCompleteVFX;
    [SerializeField] private ParticleSystem slamImpactVFX;
    [SerializeField] private ParticleSystem chargedAuraVFX;

    // ... (private variables are unchanged) ...
    private bool isMoving = false;
    private Transform pivot;
    private MoveIndicatorManager indicatorManager;
    private float currentCharge = 0f;
    private bool isCharging = false;
    private bool isFullyCharged = false;
    private Material cuboidMaterial;

    void Start()
    {
        // ... (Start method is unchanged) ...
        pivot = new GameObject("CuboidPivot").transform;
        indicatorManager = FindFirstObjectByType<MoveIndicatorManager>();
        cuboidMaterial = GetComponent<Renderer>().material;
        cuboidMaterial.EnableKeyword("_EMISSION");
        SnapToGrid();
        AdjustHeight();
    }

    void Update()
    {
        // ... (Update method is unchanged) ...
        if (isMoving) return;
        indicatorManager?.UpdateIndicators(this);
        HandleSlamCharging();
        if (Input.GetKey(KeyCode.W)) TryMove(Vector3.forward);
        else if (Input.GetKey(KeyCode.S)) TryMove(Vector3.back);
        else if (Input.GetKey(KeyCode.A)) TryMove(Vector3.left);
        else if (Input.GetKey(KeyCode.D)) TryMove(Vector3.right);
    }
    
    private void HandleSlamCharging()
    {
        // ⭐ MODIFIED: Play charging VFX
        if (Input.GetKeyDown(KeyCode.Space) && IsUpright())
        {
            isCharging = true;
            currentCharge = 0f;
            isFullyCharged = false;
            chargingVFX?.Play(); // Play new effect
        }

        // ... (rest of HandleSlamCharging is unchanged) ...
        if (isCharging)
        {
            if (Input.GetKeyUp(KeyCode.Space) || !IsUpright())
            {
                CancelCharge();
                return;
            }
            currentCharge += Time.deltaTime;
            if (currentCharge >= chargeTime && !isFullyCharged)
            {
                isFullyCharged = true;
                chargeCompleteVFX?.Play();
                chargedAuraVFX?.Play();
            }
            UpdateGlow();
        }
    }

    private void CancelCharge()
    {
        // ⭐ MODIFIED: Stop charging VFX
        isCharging = false;
        isFullyCharged = false;
        currentCharge = 0f;
        chargingVFX?.Stop(); // Stop new effect
        chargedAuraVFX?.Stop();
        UpdateGlow();
    }

    private IEnumerator Slam(Vector3 direction)
    {
        bool wasFullyCharged = isFullyCharged;
        CancelCharge();
        
        // Use the faster slamDuration for the roll
        yield return StartCoroutine(Roll(direction, true));

        if (wasFullyCharged)
        {
            // ⭐ NEW: Trigger Camera Shake
            SmoothCameraFollow.Instance.StartShake(0.2f, 0.3f); // (duration, magnitude)

            slamImpactVFX?.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
            slamImpactVFX?.Play();
            // --- ADD YOUR DAMAGE LOGIC HERE ---
        }
    }
    
    // ⭐ MODIFIED: Uses slamDuration
    private IEnumerator Roll(Vector3 direction, bool isSlam = false)
    {
        isMoving = true;
        float elapsedTime = 0f;
        // ... (pivot setup code is unchanged) ...
        Vector3 rotationAxis = Vector3.Cross(Vector3.up, direction);
        Bounds bounds = GetComponent<Renderer>().bounds;
        float halfWidth = bounds.size.x / 2f;
        float halfHeight = bounds.size.y / 2f;
        float halfDepth = bounds.size.z / 2f;
        Vector3 pivotPos = transform.position + new Vector3(direction.x * halfWidth, -halfHeight, direction.z * halfDepth);
        pivot.position = pivotPos;
        transform.SetParent(pivot);
        Quaternion startRotation = pivot.rotation;
        Quaternion endRotation = Quaternion.AngleAxis(90, rotationAxis) * startRotation;

        // Use the faster slamDuration if this is a slam
        float currentRollDuration = isSlam ? slamDuration : rollDuration;
        
        while (elapsedTime < currentRollDuration)
        {
            pivot.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / currentRollDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // ... (rest of Roll method is unchanged) ...
        pivot.rotation = endRotation;
        transform.SetParent(null);
        SnapToGrid();
        AdjustHeight();
        isMoving = false;
    }

    // ... (All other methods like TryMove, IsUpright, CanRoll, Shake, etc. are unchanged) ...
    #region --- Unchanged Methods ---
    private void UpdateGlow()
    {
        float glowPercent = Mathf.Clamp01(currentCharge / chargeTime);
        Color finalColor = Color.Lerp(Color.black, maxGlowColor, glowPercent);
        cuboidMaterial.SetColor("_EmissionColor", finalColor * maxGlowIntensity * glowPercent);
    }
    
    private void TryMove(Vector3 direction)
    {
        if (!CanRoll(direction))
        {
            StartCoroutine(Shake(direction));
            if (isCharging) CancelCharge();
            return;
        }

        if (isCharging)
        {
            StartCoroutine(Slam(direction));
        }
        else
        {
            StartCoroutine(Roll(direction));
        }
    }
    
    public bool IsUpright()
    {
        return Mathf.Approximately(GetComponent<Renderer>().bounds.size.y, 2f);
    }
    
    public void GetNextRollInfo(Vector3 direction, out Vector3 targetPos, out Vector3 futureSize)
    {
        Bounds bounds = GetComponent<Renderer>().bounds;
        Vector3 absDir = new Vector3(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z));
        float sizeInRollDir = Vector3.Dot(bounds.size, absDir);
        float sizeUp = bounds.size.y;
        float distance = (sizeInRollDir / 2f) + (sizeUp / 2f);
        targetPos = transform.position + direction * distance;
        if (Mathf.Abs(direction.x) > 0)
        {
            futureSize = new Vector3(bounds.size.y, bounds.size.x, bounds.size.z);
        }
        else
        {
            futureSize = new Vector3(bounds.size.x, bounds.size.z, bounds.size.y);
        }
    }

    public bool CanRoll(Vector3 direction)
    {
        GetNextRollInfo(direction, out Vector3 targetPos, out Vector3 futureSize);
        Vector3 futureHalfExtents = futureSize / 2f * 0.9f;
        if (Physics.CheckBox(targetPos, futureHalfExtents, Quaternion.identity, wallLayer) ||
            !Physics.Raycast(targetPos + Vector3.up, Vector3.down, 3f, groundLayer))
        {
            return false;
        }
        return true;
    }
    
    private IEnumerator Shake(Vector3 direction)
    {
        isMoving = true;
        Vector3 originalPos = transform.position;
        Vector3 shakePos = originalPos + direction * shakeIntensity;
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            transform.position = Vector3.Lerp(originalPos, shakePos, Mathf.Sin(elapsed / shakeDuration * Mathf.PI));
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPos;
        isMoving = false;
    }

    private void SnapToGrid()
    {
        Vector3 p = transform.position;
        transform.position = new Vector3(Mathf.Round(p.x / 0.5f) * 0.5f, p.y, Mathf.Round(p.z / 0.5f) * 0.5f);
    }

    private void AdjustHeight()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f, groundLayer))
        {
            float cubeHeight = GetComponent<Renderer>().bounds.size.y;
            Vector3 finalPosition = transform.position;
            finalPosition.y = hit.point.y + (cubeHeight / 2f);
            transform.position = finalPosition;
        }
    }
    #endregion
}