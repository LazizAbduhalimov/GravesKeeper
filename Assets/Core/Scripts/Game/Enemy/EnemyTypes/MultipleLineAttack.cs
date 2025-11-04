using PoolSystem.Alternative;
using System.Collections;
using UnityEngine;

[SelectionBase]
public class MultipleLineAttack : AttackBase
{
    [Header("Spread settings")]
    [Tooltip("Total spread angle in degrees (0..spread). Example: 45 yields lines at 0,22.5,45 when lines=3)")]
    [SerializeField] private float _spreadAngle = 45f;
    [Tooltip("Number of rays/lines to spawn inside the spread (>=1)")]
    [SerializeField] private int _lines = 3;

    [Header("Queue / timing")]
    [Tooltip("How many times the full set of lines will be fired in a row")]
    [SerializeField] private int _queues = 1;
    [Tooltip("Seconds between repeated queues")]
    [SerializeField] private float _timeBetweenQueues = 0.2f;

    [Header("Pool / spawn")]
    [SerializeField] private Transform _bulletCreateTransform;
    [SerializeField] private PoolContainer _bulletsPoolData;

    [Header("Preview")]
    [SerializeField] private PoolContainer _lineRendererPool;
    [SerializeField] private float _previewLength = 5f;

    private bool _isAttacking;
    private LineRenderer[] _previewLines;

    private void Start()
    {
        InitializePreviewLines();
    }

    private void Update()
    {
        // Let base TryAttack() manage PassedAttackTime and firing timing.
        if (!_isAttacking && !isPerformingAttack)
            PassedAttackTime += Time.deltaTime;
            
        RefreshReloadBar();
        TryAttackMultiple();
        UpdatePreviewLines();
    }
    
    private void TryAttackMultiple()
    {
        if (isPerformingAttack || _isAttacking) return;
        
        if (PassedAttackTime >= AttackRate)
        {
            StartCoroutine(PerformAttackSequence());
        }
    }

    private void InitializePreviewLines()
    {
        if (_lineRendererPool == null) return;

        int linesCount = Mathf.Max(1, _lines);
        _previewLines = new LineRenderer[linesCount];
        
        for (int i = 0; i < linesCount; i++)
        {
            var lineObj = _lineRendererPool.Pool.GetFreeElement(false);
            var line = lineObj.GetComponent<LineRenderer>();
            if (line != null)
            {
                line.positionCount = 2;
                line.gameObject.SetActive(true);
                _previewLines[i] = line;
            }
        }
    }

    private void UpdatePreviewLines()
    {
        if (_previewLines == null) return;

        // Show preview only in the last 1/4 of attack cooldown
        float thresholdTime = AttackRate * 0.75f;
        bool shouldShow = PassedAttackTime >= thresholdTime && !_isAttacking;
        
        if (!shouldShow)
        {
            foreach (var line in _previewLines)
            {
                if (line != null)
                    line.gameObject.SetActive(false);
            }
            return;
        }

        int linesCount = Mathf.Max(1, _lines);
        float start = -_spreadAngle * 0.5f;
        float step = (linesCount > 1) ? _spreadAngle / (linesCount - 1) : 0f;
        Vector3 startPos = _bulletCreateTransform != null ? 
            _bulletCreateTransform.position : transform.position;

        for (int i = 0; i < _previewLines.Length && i < linesCount; i++)
        {
            if (_previewLines[i] == null) continue;
            
            _previewLines[i].gameObject.SetActive(true);

            float angle = start + i * step;
            var direction = transform.rotation * Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            // Raycast to detect walls
            int wallLayer = LayerMask.GetMask("Wall");
            if (Physics.Raycast(startPos, direction, out RaycastHit hit, _previewLength, wallLayer))
            {
                // Hit something, stop line at hit point
                _previewLines[i].SetPosition(0, startPos);
                _previewLines[i].SetPosition(1, hit.point);
            }
            else
            {
                // No hit, draw full length
                Vector3 endPos = startPos + direction * _previewLength;
                _previewLines[i].SetPosition(0, startPos);
                _previewLines[i].SetPosition(1, endPos);
            }
        }
    }

    private void OnDestroy()
    {
        if (_previewLines != null)
        {
            foreach (var line in _previewLines)
            {
                if (line != null)
                {
                    line.gameObject.SetActive(false);
                }
            }
        }
    }

    protected override void Attack()
    {
        // Start firing the configured spread queues
        StartCoroutine(FireQueues());
    }

    private IEnumerator FireQueues()
    {
        _isAttacking = true;

        for (int q = 0; q < Mathf.Max(1, _queues); q++)
        {
            FireLines();
            if (q < _queues - 1)
                yield return new WaitForSeconds(_timeBetweenQueues);
        }

        _isAttacking = false;
    }

    private void FireLines()
    {
        int linesCount = Mathf.Max(1, _lines);
        // Center the spread around forward: start = -spread/2, end = +spread/2
        float start = -_spreadAngle * 0.5f;
        float step = (linesCount > 1) ? _spreadAngle / (linesCount - 1) : 0f;

        for (int i = 0; i < linesCount; i++)
        {
            float angle = start + i * step; // -spread/2 .. +spread/2
            var rot = transform.rotation * Quaternion.Euler(0f, angle, 0f);
            CreateBullet(_bulletCreateTransform != null ? _bulletCreateTransform.position : transform.position, rot);
        }
    }

    private void CreateBullet(Vector3 position, Quaternion rotation)
    {
        if (_bulletsPoolData == null)
            return;

        var bullet = _bulletsPoolData.Pool.GetFreeElement(false);
        bullet.transform.SetPositionAndRotation(position, rotation);
        bullet.gameObject.SetActive(true);
    }
}