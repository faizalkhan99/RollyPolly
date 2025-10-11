using UnityEngine;
using Unity.Cinemachine;

public class TeleportationSystem : MonoBehaviour
{
    [SerializeField] private GameObject _interactDialogue;
    [SerializeField] private Transform _targetPos;
    [SerializeField] private RollingCuboidController _playerController; // ⭐ MODIFIED: Reference the controller script
    [SerializeField] private CinemachineCamera _vCamTransform;

    [SerializeField] private bool _canTeleport;

    void Start()
    {
        _canTeleport = false;
        _interactDialogue.SetActive(false);
    }

    void Update()
    {
        if (_canTeleport )
        {
            if (_playerController == null) return;

            // --- 1. Stop all player movement routines BEFORE teleporting ---
            _playerController.StopAllMovement();

            // --- 2. Teleport Logic ---
            Vector3 oldPos = _playerController.transform.position;
            _playerController.transform.position = _targetPos.position;
            // ⭐ NEW: This line resets the rotation to be perfectly upright.
            _playerController.transform.rotation = Quaternion.identity;
            // --- 3. Clean up the player's state AFTER teleporting ---
            _playerController.SnapToGrid();
            _playerController.AdjustHeight();

            // --- 4. Update Cinemachine Camera ---
            Vector3 delta = _targetPos.position - oldPos;
            _vCamTransform.OnTargetObjectWarped(_playerController.transform, delta);

            //PlayOneShot() teleport sfx here.
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _interactDialogue.SetActive(true);
            _canTeleport = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        _interactDialogue.SetActive(false);
        _canTeleport = false;
    }
}