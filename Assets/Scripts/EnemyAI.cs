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
    private HealthSystem healthSystem;
    private StunHandler stunHandler;

    void Start()
    {
        playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        healthSystem = GetComponent<HealthSystem>();
        stunHandler = GetComponent<StunHandler>();
        StartCoroutine(UpdatePath());
    }

    void Update()
    {
        // Don't move if stunned
        if (stunHandler != null && stunHandler.IsStunned)
        {
            return;
        }

        // Follow the calculated path
        if (path != null && path.Count > 0)
        {
            Vector3 currentWaypoint = path[targetIndex];
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, moveSpeed * Time.deltaTime);

            if (transform.position == currentWaypoint)
            {
                targetIndex++;
                if (targetIndex >= path.Count)
                {
                    path = null; // Path complete
                    targetIndex = 0;
                }
            }
        }
    }

    IEnumerator UpdatePath()
    {
        while (true)
        {
            if (playerTarget != null)
            {
                path = EnemyPathfinding.FindPath(transform.position, playerTarget.position);
                targetIndex = 0;
            }
            yield return new WaitForSeconds(pathUpdateCooldown);
        }
    }
}
