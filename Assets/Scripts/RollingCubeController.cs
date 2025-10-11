using System;
using System.Collections;
using UnityEngine;

public class RollingCuboidController : MonoBehaviour
{
    #region Unchanged Variables
    [Header("Movement")]
    [SerializeField] private float rollDuration = 0.25f;
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    #endregion

    [Header("Slam Attack")]
    [SerializeField] private float chargeTime = 1.5f;
    [SerializeField] private float slamDuration = 0.1f;
    [SerializeField] private int slamDamage = 10;
    [SerializeField] private float slamRadius = 1.5f;
    [SerializeField] private Color chargingGlowColor = Color.white;
    [SerializeField] private Color fullyChargedGlowColor = Color.red;
    [SerializeField] private float maxGlowIntensity = 2f;
    public bool IsSlamming { get; private set; }

    [Header("Slam VFX")]
    [SerializeField] private ParticleSystem slamImpactVFX;

    private HealthSystem healthSystem;
    private StunHandler stunHandler;

    private bool isMoving = false;
    private Transform pivot;
    private MoveIndicatorManager indicatorManager;
    private float currentCharge = 0f;
    private bool isCharging = false;
    private bool isFullyCharged = false;
    private Material cuboidMaterial;

    void Start()
    {
        pivot = new GameObject("CuboidPivot").transform;
        indicatorManager = FindFirstObjectByType<MoveIndicatorManager>();
        healthSystem = GetComponent<HealthSystem>();
        stunHandler = GetComponent<StunHandler>();
        cuboidMaterial = GetComponent<Renderer>().material;
        cuboidMaterial.EnableKeyword("_EMISSION");
        SnapToGrid();
        AdjustHeight();
    }

    void Update()
    {
        if (isMoving || (stunHandler != null && stunHandler.IsStunned))
        {
            return;
        }

        indicatorManager?.UpdateIndicators(this);
        HandleSlamCharging();

        if (Input.GetKey(KeyCode.W)) TryMove(Vector3.forward);
        else if (Input.GetKey(KeyCode.S)) TryMove(Vector3.back);
        else if (Input.GetKey(KeyCode.A)) TryMove(Vector3.left);
        else if (Input.GetKey(KeyCode.D)) TryMove(Vector3.right);
    }

    // ⭐ MODIFIED: This coroutine now triggers the slam VFX and a subtle camera shake
    private IEnumerator Slam(Vector3 direction)
    {
        IsSlamming = true;
        bool wasFullyCharged = isFullyCharged;
        CancelCharge();

        yield return StartCoroutine(Roll(direction, true));

        if (wasFullyCharged)
        {
            GameManager.Instance.TriggerScreenFreeze(0.05f);
            SmoothCameraFollow.Instance.StartShake(0.15f, 0.2f);

            if (slamImpactVFX != null)
            {
                Instantiate(slamImpactVFX, transform.position, Quaternion.identity);
            }
            DealSlamDamage();
        }
    }

    private void DealSlamDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, slamRadius);
        foreach (var hitCollider in hitColliders)
        {
            // Check if the object has an "Enemy" tag and a HealthSystem
            if (hitCollider.CompareTag("Enemy"))
            {
                HealthSystem enemyHealth = hitCollider.GetComponent<HealthSystem>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(slamDamage);
                }
            }
        }
    }

    #region Unchanged Methods
    private void HandleSlamCharging()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsUpright())
        {
            isCharging = true;
            currentCharge = 0f;
            isFullyCharged = false;
        }
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
            }
            UpdateGlow();
        }
    }
    private void UpdateGlow()
    {
        float glowPercent = Mathf.Clamp01(currentCharge / chargeTime);
        Color targetGlowColor;
        if (isFullyCharged)
        {
            targetGlowColor = fullyChargedGlowColor;
        }
        else
        {
            targetGlowColor = chargingGlowColor;
        }
        Color finalColor = Color.Lerp(Color.black, targetGlowColor, glowPercent);
        cuboidMaterial.SetColor("_EmissionColor", finalColor * maxGlowIntensity * glowPercent);
    }
    private void CancelCharge()
    {
        isCharging = false;
        isFullyCharged = false;
        currentCharge = 0f;
        UpdateGlow();
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
    private IEnumerator Roll(Vector3 direction, bool isSlam = false)
    {
        isMoving = true;
        float elapsedTime = 0f;
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
        float currentRollDuration = isSlam ? slamDuration : rollDuration;
        while (elapsedTime < currentRollDuration)
        {
            pivot.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / currentRollDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        pivot.rotation = endRotation;
        transform.SetParent(null);
        SnapToGrid();
        AdjustHeight();
        isMoving = false;
        IsSlamming = false;
    }
    public bool IsUpright()
    {
        return GetComponent<Renderer>().bounds.size.y > 1.5f;
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
    public void StopAllMovement()
    {
        // This stops the Roll() or Slam() coroutine immediately
        StopAllCoroutines();

        // Reset the state variables
        isMoving = false;

        // Ensure the player isn't a child of the pivot anymore
        if (pivot != null)
        {
            transform.SetParent(null);
        }
    }
    public void SnapToGrid()
    {
        Vector3 p = transform.position;
        transform.position = new Vector3(Mathf.Round(p.x / 0.5f) * 0.5f, p.y, Mathf.Round(p.z / 0.5f) * 0.5f);
    }
    public void AdjustHeight()
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
