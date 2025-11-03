using PoolSystem.Alternative;
using UnityEngine;

[SelectionBase]
public class TurretAttack : AttackBase
{
    [SerializeField] private int _bulletNumber = 1;
    [SerializeField] private float _timeBtwAttack = 0.2f;
    [SerializeField] private float _bulletSpeed = 8;
    [SerializeField] private Transform _bulletCreateTransform;
    [SerializeField] private PoolContainer _bulletsPoolData;
    
    [Header("Preview")]
    [SerializeField] private PoolContainer _lineRendererPool;
    [SerializeField] private float _previewLength = 5f;
    
    private BarController _barController;
    
    private bool _isAttacking;
    private LineRenderer _previewLine;
    private float _passedTimeBtwAttack;
    private int _bulletsCreatedWhileAttacking;

    private void Start()
    {
        _passedTimeBtwAttack = _timeBtwAttack;
        InitializePreviewLine();
    }

    private void Update()
    {
        if (!_isAttacking)
            PassedAttackTime += Time.deltaTime;
        
        RefreshReloadBar();
        TryAttack();
        UpdatePreviewLine();
    }

    private void InitializePreviewLine()
    {
        if (_lineRendererPool == null) return;
        
        var lineObj = _lineRendererPool.Pool.GetFreeElement(false);
        _previewLine = lineObj.GetComponent<LineRenderer>();
        if (_previewLine != null)
        {
            _previewLine.positionCount = 2;
            _previewLine.gameObject.SetActive(true);
        }
    }

    private void UpdatePreviewLine()
    {
        if (_previewLine == null) return;

        // Show preview only in the last 1/4 of attack cooldown
        float thresholdTime = AttackRate * 0.75f;
        bool shouldShow = PassedAttackTime >= thresholdTime && !_isAttacking;
        
        if (!shouldShow)
        {
            _previewLine.gameObject.SetActive(false);
            return;
        }
        
        _previewLine.gameObject.SetActive(true);

        Vector3 startPos = _bulletCreateTransform != null ? 
            _bulletCreateTransform.position : transform.position;
        Vector3 direction = transform.forward;
        
        // Raycast to detect walls
        int wallLayer = LayerMask.GetMask("Wall");
        if (Physics.Raycast(startPos, direction, out RaycastHit hit, _previewLength, wallLayer))
        {
            // Hit something, stop line at hit point
            _previewLine.SetPosition(0, startPos);
            _previewLine.SetPosition(1, hit.point);
        }
        else
        {
            // No hit, draw full length
            Vector3 endPos = startPos + direction * _previewLength;
            _previewLine.SetPosition(0, startPos);
            _previewLine.SetPosition(1, endPos);
        }
    }

    private void OnDestroy()
    {
        if (_previewLine != null)
        {
            _previewLine.gameObject.SetActive(false);
        }
    }

    protected override void Attack()
    {
        CreateBullet(_bulletCreateTransform.position, transform.rotation);
    }

    private bool TryAttack()
    {
        if (PassedAttackTime < AttackRate && !_isAttacking)
            return false;

        _isAttacking = true;

        _passedTimeBtwAttack += Time.deltaTime;
        if (_passedTimeBtwAttack >= _timeBtwAttack)
        {
            Attack();
            _bulletsCreatedWhileAttacking++;
            _passedTimeBtwAttack -= _timeBtwAttack;
            PassedAttackTime -= AttackRate / _bulletNumber;

            if (_bulletsCreatedWhileAttacking >= _bulletNumber)
            {
                _passedTimeBtwAttack = _timeBtwAttack;
                _bulletsCreatedWhileAttacking = 0;
                _isAttacking = false;
            }
        }
        return true;
    }

    private void CreateBullet(Vector3 position, Quaternion rotation)
    {
        var bullet = _bulletsPoolData.Pool.GetFreeElement(false);
        var bulletComp = bullet.GetComponent<EnemyBullet>();
        bulletComp.Initialize(_bulletSpeed, 15, 3);
        bullet.transform.SetPositionAndRotation(position, rotation);
        bullet.gameObject.SetActive(true);
    }
}