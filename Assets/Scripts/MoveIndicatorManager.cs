using UnityEngine;

public class MoveIndicatorManager : MonoBehaviour
{
    private Renderer playerRenderer;
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Color blockedColor = Color.red;
    [SerializeField] private LayerMask groundLayer;

    private GameObject[] indicators = new GameObject[4];
    private MeshRenderer[] indicatorRenderers = new MeshRenderer[4];
    private readonly Vector3[] dirs = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            indicators[i] = Instantiate(indicatorPrefab, Vector3.zero, Quaternion.Euler(90, 0, 0), transform);
            indicatorRenderers[i] = indicators[i].GetComponent<MeshRenderer>();
            indicators[i].SetActive(false);
        }
    }

    public void UpdateIndicators(RollingCuboidController player)
    {
        if (playerRenderer == null)
        {
            playerRenderer = player.GetComponent<Renderer>();
        }
        for (int i = 0; i < dirs.Length; i++)
        {
            Vector3 dir = dirs[i];

            player.GetNextRollInfo(dir, out Vector3 targetPos, out Vector3 futureSize);

            // First, check if the move is possible.
            if (player.CanRoll(dir))
            {
                // It's a valid move, so we hide the indicator.
                indicators[i].SetActive(false);
            }
            else
            {
                // It's an illegal move, so we MUST show a red tile.
                indicators[i].SetActive(true);
                indicatorRenderers[i].material.color = blockedColor;

                // Set its shape to match the potential landing footprint.
                indicators[i].transform.localScale = new Vector3(futureSize.x, futureSize.z, 1);

                // Now, figure out where to place the red tile.
                // Try to place it on the ground first.
                if (Physics.Raycast(targetPos + Vector3.up, Vector3.down, out RaycastHit hit, 3f, groundLayer))
                {
                    indicators[i].transform.position = hit.point + Vector3.up * 0.01f;
                }
                else
                {
                    // If there's no ground, place it at the target X/Z, but at the player's height.
                    indicators[i].transform.position = new Vector3(targetPos.x, player.transform.position.y - (player.GetComponent<Renderer>().bounds.size.y / 2f) + 0.01f, targetPos.z);
                }
                indicators[i].transform.position = new Vector3(targetPos.x, player.transform.position.y - (playerRenderer.bounds.size.y / 2f) + 0.01f, targetPos.z);
            }
        }
    }
}