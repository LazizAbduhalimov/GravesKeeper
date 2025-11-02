using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float _speed;
    [SerializeField] private int _bouncesBeforeDie;
    [SerializeField] private float _angleError;

    private int _bounces;
    private Rigidbody _rb;

    private void Awake()
    {

        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        if (gameObject.activeInHierarchy == true)
        {
            _rb.linearVelocity = transform.forward * _speed;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            _bounces++;
            if (_bounces >= _bouncesBeforeDie) RefreshBullet();

            Vector3 normal = collision.contacts[0].normal;
            normal.y = 0;
            normal.Normalize();

            Vector3 reflectDir = Vector3.Reflect(transform.forward, normal);
            reflectDir = Quaternion.Euler(0f, Random.Range(-_angleError, _angleError), 0f) * reflectDir;
            transform.forward = reflectDir;

            _rb.linearVelocity = new Vector3(reflectDir.x, 0f, reflectDir.z) * _speed;
        }
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Player");
            //ЛОГИКА СТОЛКНОВЕНИЕ С ИГРОКОМ
        }

    }
    private void RefreshBullet()
    {
        _bounces = 0;
        gameObject.SetActive(false);
    }
}
