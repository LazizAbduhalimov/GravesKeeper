using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RotateToNearestTarget : MonoBehaviour
{
    [Header("Targets (Manual List Only)")]
    [SerializeField] private Transform[] _manualTargets;
    [SerializeField, Min(0.05f)] private float _searchInterval = 0.25f;

    [Header("Rotation Settings")]
    [SerializeField, Min(0f)] private float _rotationSpeedDegPerSec = 360f;
    [SerializeField] private bool _ignoreY = true;
    [SerializeField] private Transform _pivot; // Optional pivot to rotate (defaults to this.transform)

    [Header("Debug/Gizmos")] 
    [SerializeField] private bool _drawSearchGizmo = false;

    private Transform _currentTarget;
    private WaitForSeconds _waitSearch;

    private void Awake()
    {
        _manualTargets = FindObjectsByType<BrickMb>(FindObjectsSortMode.None).Select(b => b.transform).ToArray();
        if (_pivot == null) _pivot = transform;
        _waitSearch = new WaitForSeconds(Mathf.Max(0.05f, _searchInterval));
    }

    private void OnEnable()
    {
        StartCoroutine(FindTargetsLoop());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _currentTarget = null;
    }

    private IEnumerator FindTargetsLoop()
    {
        while (true)
        {
            _currentTarget = NearestFromManual();
            yield return _waitSearch;
        }
    }

    private void Update()
    {
    if (_currentTarget == null || !_currentTarget.gameObject.activeInHierarchy) return;

        Vector3 toTarget = _currentTarget.position - _pivot.position;
        if (_ignoreY) toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
        _pivot.rotation = Quaternion.RotateTowards(_pivot.rotation, targetRot, _rotationSpeedDegPerSec * Time.deltaTime);
    }

    private Transform NearestFromManual()
    {
        Transform best = null;
        float bestSqr = float.PositiveInfinity;
        Vector3 origin = _pivot.position;

        if (_manualTargets == null || _manualTargets.Length == 0) return null;

        foreach (var t in _manualTargets)
        {
            if (t == null || !t.gameObject.activeInHierarchy) continue;
            Vector3 d = t.position - origin;
            if (_ignoreY) d.y = 0f;
            float sqr = d.sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = t;
            }
        }
        return best;
    }

    private void OnDrawGizmosSelected()
    {
        if (!_drawSearchGizmo) return;
        if (_pivot == null) _pivot = transform;
        Gizmos.color = Color.cyan;
        // Optional: draw lines to manual targets
        if (_manualTargets != null)
        {
            foreach (var t in _manualTargets)
            {
                if (t != null && t.gameObject.activeInHierarchy)
                {
                    Gizmos.DrawLine(_pivot.position, t.position);
                }
            }
        }
        if (_currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(_pivot.position, _currentTarget.position);
        }
    }

    // Public API to set targets at runtime
    public void SetManualTargets(IEnumerable<Transform> targets)
    {
        if (targets == null) { _manualTargets = null; return; }
        var list = new List<Transform>(targets);
        _manualTargets = list.ToArray();
    }
}
