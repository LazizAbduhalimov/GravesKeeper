using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float _speed;
    [SerializeField] private float _angleError;
    [SerializeField] private int _bouncesBeforeDie;

    private int _bounces;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        if (gameObject.activeInHierarchy)
        {
            _rb.linearVelocity = transform.forward * _speed;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"{collision.gameObject.tag}, {collision.gameObject.name}");
        var go = collision.gameObject;
        if (go.TryGetComponent<PlayerMb>(out var player))
        {
            player.Stun();
        }

        if (go.TryGetComponent<HealthCompponent>(out var health))
        {
            health.TakeOneDamage();
        }
        else if (go.transform.parent &&
                 go.transform.parent.TryGetComponent<HealthCompponent>(out var healthComp))
        {
            healthComp.TakeOneDamage();
        }
        
        _bounces++;
        if (_bounces >= _bouncesBeforeDie) RefreshBullet();

        var normal = collision.contacts[0].normal;
        normal.y = 0;
        normal.Normalize();

        var reflectDir = Vector3.Reflect(transform.forward, normal);
        reflectDir = Quaternion.Euler(0f, Random.Range(-_angleError, _angleError), 0f) * reflectDir;
        transform.forward = reflectDir;
        _rb.linearVelocity = new Vector3(reflectDir.x, 0f, reflectDir.z) * _speed;
    }
    
    private void RefreshBullet()
    {
        _bounces = 0;
        gameObject.SetActive(false);
    }

    public void Initialize(float speed,float angleError, int bounces)
    {
        _speed = speed;
        _bouncesBeforeDie = bounces;
        _angleError = angleError;
    }
}
