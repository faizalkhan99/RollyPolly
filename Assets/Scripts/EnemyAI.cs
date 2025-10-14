using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Pathfinding")]
    [SerializeField] private float pathUpdateCooldown = 0.5f;

    private Transform playerTarget;
    private List<Vector3> path;
    private int targetIndex;
    private EnemyStunHandler enemyStunHandler; // Reference to the stun script

    void Start()
    {
        playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        enemyStunHandler = GetComponent<EnemyStunHandler>();
        StartCoroutine(UpdatePath());
    }

    void Update()
    {
        // ⭐ THIS IS THE CRUCIAL FIX ⭐
        // If the stun handler exists and the enemy is stunned, stop ALL execution in this Update frame.
        if (enemyStunHandler != null && enemyStunHandler.IsStunned)
        {
            return; // This prevents both movement and LookAt from running
        }

        // --- Path Following Logic (will only run if not stunned) ---
        if (path != null && path.Count > 0)
        {
            Vector3 currentWaypoint = path[targetIndex];
            Vector3 targetPosition = new Vector3(currentWaypoint.x, transform.position.y, currentWaypoint.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Make the enemy look where it's going
            if (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.LookAt(targetPosition);
            }

            // Check if we've reached the waypoint
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                targetIndex++;
                if (targetIndex >= path.Count)
                {
                    path = null;
                    targetIndex = 0;
                }
            }
        }
    }

    IEnumerator UpdatePath()
    {
        while (true)
        {
            // Only update the path if the player exists and we are not stunned
            if (playerTarget != null && (enemyStunHandler == null || !enemyStunHandler.IsStunned))
            {
                path = EnemyPathfinding.FindPath(transform.position, playerTarget.position);
                targetIndex = 0;
            }
            yield return new WaitForSeconds(pathUpdateCooldown);
        }
    }
}

