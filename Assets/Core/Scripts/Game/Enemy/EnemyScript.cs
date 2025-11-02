using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    [Header("Shoot Settings")]
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private float _shootDelay;
    [SerializeField] private Transform _target;

    [Header("Pool Settings")]
    [SerializeField] private PoolManager _poolManager;

    [Header("Shoot Settings")]
    [SerializeField] private float _bulletSpeed;
    [SerializeField] private float _shootError;
    [SerializeField] private int _bulletBounces;

    private GameObject _bullet;
    private float _timer;

    public void FixedUpdate()
    {
        Shoot();
    }

    private void Shoot()
    {
        if (ShootCondition())
        {
            if (_timer < _shootDelay)
            {
                _timer += Time.fixedDeltaTime;
                return;
            }

            _timer = 0;
            var direction = _target.transform.position - transform.position;
            direction.y = transform.position.y;

            var lookRotation = Quaternion.LookRotation(direction);

            _bullet = _poolManager.GetObject(_shootPoint.position, lookRotation);
            _bullet.GetComponent<EnemyBullet>().Initialize(_bulletSpeed,_shootError, _bulletBounces);
        }
    }

    private bool ShootCondition()
    {
        if (_target)
        {
            return !_bullet || _bullet.activeInHierarchy == false;
        }

        return false;

        //“€ ÃŒ∆≈ÿ‹ »«Ã≈Õ»“‹ »À» ∆≈ –¿—ÿ»–»“‹ ÀŒ√» ” ”—ÀŒ¬»ﬂ ¬€—“–≈À¿
    }
}
