using PoolSystem.Alternative;
using UnityEngine;

[SelectionBase]
public class TurretAttack : AttackBase
{
    [SerializeField] private int _bulletNumber = 1;
    [SerializeField] private float _timeBtwAttack = 0.2f;
    [SerializeField] private float _bulletSpeed = 8;
    [SerializeField] private Transform _bulletCreateTransform;
    
    [Header("Preview")]
    [SerializeField] private float _previewLength = 5f;
    
    private bool _isAttacking;
    private LineRenderer _previewLine;
    private float _passedTimeBtwAttack;
    private int _bulletsCreatedWhileAttacking;
    
    // Кешируем для оптимизации
    private int _wallLayerMask;
    private float _nextRaycastTime;
    private const float RaycastInterval = 0.1f; // Проверяем не каждый кадр

    protected override void Start()
    {
        base.Start();
        _passedTimeBtwAttack = _timeBtwAttack;
        _wallLayerMask = LayerMask.GetMask("Wall");
    }

    private void Update()
    {
        if (!_isAttacking && !isPerformingAttack)
            PassedAttackTime += Time.deltaTime;
        
        RefreshReloadBar();
        CheckAndTryAttack();
        UpdatePreviewLine();
    }

    private void UpdatePreviewLine()
    {
        // Show preview only in the last 1/4 of attack cooldown
        float thresholdTime = AttackRate * 0.75f;
        bool shouldShow = PassedAttackTime >= thresholdTime && !_isAttacking;
        
        if (!shouldShow)
        {
            if (_previewLine != null)
            {
                ReturnLineRendererToPool(_previewLine);
                _previewLine = null;
            }
            return;
        }
        
        if (_previewLine == null)
        {
            _previewLine = GetLineRendererFromPool();
            if (_previewLine != null)
            {
                _previewLine.positionCount = 2;
            }
        }
        
        if (_previewLine == null) return;

        // Обновляем raycast не каждый кадр для оптимизации
        if (Time.time < _nextRaycastTime) return;
        _nextRaycastTime = Time.time + RaycastInterval;

        Vector3 startPos = _bulletCreateTransform != null ? 
            _bulletCreateTransform.position : transform.position;
        Vector3 direction = transform.forward;
        
        // Raycast to detect walls
        if (Physics.Raycast(startPos, direction, out RaycastHit hit, _previewLength, _wallLayerMask))
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

    private void OnDisable()
    {
        if (_previewLine != null)
        {
            ReturnLineRendererToPool(_previewLine);
            _previewLine = null;
        }
    }

    protected override void Attack()
    {
        // Start burst sequence
        StartCoroutine(BurstAttackSequence());
    }

    private System.Collections.IEnumerator BurstAttackSequence()
    {
        _isAttacking = true;
        _bulletsCreatedWhileAttacking = 0;
        
        for (int i = 0; i < _bulletNumber; i++)
        {
            CreateBullet(_bulletCreateTransform.position, transform.rotation);
            _bulletsCreatedWhileAttacking++;
            
            if (_bulletsCreatedWhileAttacking < _bulletNumber)
            {
                yield return new WaitForSeconds(_timeBtwAttack);
            }
        }
        
        _isAttacking = false;
        _bulletsCreatedWhileAttacking = 0;
    }

    private void CheckAndTryAttack()
    {
        if (isPerformingAttack || _isAttacking) return;
        
        if (PassedAttackTime >= AttackRate)
        {
            StartCoroutine(PerformAttackSequence());
        }
    }

    private void CreateBullet(Vector3 position, Quaternion rotation)
    {
        var bullet = MissilesPool.GetFreeElement(false);
        bullet.transform.SetPositionAndRotation(position, rotation);
        bullet.gameObject.SetActive(true);
        
        // GetComponent после SetActive для инициализации
        var bulletComp = bullet.GetComponent<EnemyBullet>();
        if (bulletComp != null)
        {
            bulletComp.Initialize(_bulletSpeed, 15, 3);
        }
    }
}