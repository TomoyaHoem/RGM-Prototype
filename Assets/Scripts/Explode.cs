using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explode : MonoBehaviour
{
    [SerializeField] private GameObject _poofParticlePrefab;

    private void OnCollisionEnter2D(Collision2D collison)
    {
        Instantiate(_poofParticlePrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
