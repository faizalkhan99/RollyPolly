using Unity;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private Vector3 _coord;
    [SerializeField] private float _rotSpeed;

    void Update()
    {
        transform.Rotate(_coord * _rotSpeed * Time.deltaTime);
    }
}