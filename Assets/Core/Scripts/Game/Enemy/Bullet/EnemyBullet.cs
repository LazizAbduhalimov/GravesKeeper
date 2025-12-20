using PoolSystem.Alternative;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBullet : PoolObject
{
    [Header("Bullet Settings")]
    [SerializeField] private float _speed;
    [SerializeField] private float _angleError;
    [SerializeField] private int _bouncesBeforeDie;

    private int _bounces;
    private Rigidbody _rb;
    private GameObject _owner;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void Init(GameObject owner)
    {
        _owner = owner;
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
        var go = collision.gameObject;
        var hasScore = true;
        if (go == _owner && _bounces == 0)
        {
            return;
        }

        if (go.TryGetComponent<PlayerMb>(out var player))
        {
            player.Stun();
            // Push player back
            if (player.TryGetComponent<Rigidbody>(out var playerRb))
            {
                Vector3 pushDirection = (player.transform.position - transform.position).normalized;
                pushDirection.y = 0; // Keep on ground level
                float pushForce = 4500; // Adjust force as needed
                playerRb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
            }
        }

        if (go.TryGetComponent<HealthCompponent>(out var health))
        {
            Debug.Log("Enemy bullet hit health component");
            hasScore = false;
            health.TakeOneDamage();
            var text = health.CurrentHealth == 0 ? "Broken" : $"hp left: {health.CurrentHealth}";
            Damages.Instance.SpawnNumber(text, go.transform.position);
        }
        else if (go.transform.parent &&
                 go.transform.parent.TryGetComponent<HealthCompponent>(out var healthComp))
        {
            hasScore = false;
            var text = healthComp.CurrentHealth == 0 ? "Broken" : $"hp left: {healthComp.CurrentHealth}";
            Damages.Instance.SpawnNumber(text, go.transform.position);
            healthComp.TakeOneDamage();
            
        }
        
        SoundManager.Instance.PlayOneShotFX($"hit{Random.Range(1, 4)}");
        if (hasScore)
        {
            var score = 5;
            Bank.AddScore(this, score);
            Damages.Instance.SpawnScore($"+{score}", collision.transform.position);
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
