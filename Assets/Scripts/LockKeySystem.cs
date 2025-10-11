using UnityEngine;

public class LockKeySystem : MonoBehaviour
{
    [SerializeField] private GameObject _respectiveGate;
    [SerializeField] private Animator _animator;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _respectiveGate.SetActive(false);
            gameObject.SetActive(false);
            _animator.speed = 0f;
        }        
    }
}
