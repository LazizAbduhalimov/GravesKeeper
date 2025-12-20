using System;
using System.Collections;
using System.Collections.Generic;
using PoolSystem.Alternative;
using UnityEngine;

[Serializable]
public class EnemySpawnData
{
    [Tooltip("Префаб врага")]
    public GameObject enemyPrefab;
    
    [Tooltip("Количество врагов этого типа")]
    public int count = 1;
    
    [HideInInspector]
    public PoolObject poolObject;
}

[Serializable]
public class Wave
{
    [Tooltip("Враги, которые будут спавниться в этой волне")]
    public EnemySpawnData[] enemies;
    
    [Tooltip("Интервал между спавном врагов в волне (в секундах)")]
    public float spawnInterval = 1f;
    
    [Tooltip("Задержка перед следующей волной (в секундах)")]
    public float delayAfterWave = 5f;
}

public class EnemyWave : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private Wave[] _waves;
    [SerializeField] private float _spawnCheckRadius = 0.5f;
    
    [Header("Wave Control")]
    [SerializeField] private bool _autoStart = false;
    [SerializeField] private float _startDelay = 2f;
    
    private int _currentWaveIndex = 0;
    private int _currentSpawnPointIndex = 0;
    private bool _isSpawning = false;
    private Dictionary<GameObject, PoolMono<PoolObject>> _enemyPools = new Dictionary<GameObject, PoolMono<PoolObject>>();
    
    private void Start()
    {
        RegisterEnemyPools();
        
        if (_autoStart)
        {
            StartCoroutine(StartWavesWithDelay());
        }
    }
    
    private void RegisterEnemyPools()
    {
        if (_waves == null) return;
        
        for (int w = 0; w < _waves.Length; w++)
        {
            if (_waves[w].enemies == null) continue;
            
            for (int e = 0; e < _waves[w].enemies.Length; e++)
            {
                var enemyData = _waves[w].enemies[e];
                if (enemyData.enemyPrefab != null && !_enemyPools.ContainsKey(enemyData.enemyPrefab))
                {
                    var poolObject = enemyData.enemyPrefab.GetComponent<PoolObject>();
                    if (poolObject != null)
                    {
                        enemyData.poolObject = poolObject;
                        Debug.Log(PoolServiceMono.PoolService);
                        var pool = PoolServiceMono.PoolService.GetOrRegisterPool(poolObject, 3);
                        _enemyPools[enemyData.enemyPrefab] = pool;
                    }
                    else
                    {
                        Debug.LogError($"Enemy prefab {enemyData.enemyPrefab.name} doesn't have PoolObject component!");
                    }
                }
            }
        }
    }
    
    private IEnumerator StartWavesWithDelay()
    {
        yield return new WaitForSeconds(_startDelay);
        StartWaves();
    }
    
    public void StartWaves()
    {
        if (_isSpawning) return;
        
        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            Debug.LogError("Spawn points not assigned!");
            return;
        }
        
        if (_waves == null || _waves.Length == 0)
        {
            Debug.LogError("Waves not configured!");
            return;
        }
        
        _currentWaveIndex = 0;
        StartCoroutine(SpawnWaves());
    }
    
    private IEnumerator SpawnWaves()
    {
        _isSpawning = true;
        
        while (_currentWaveIndex < _waves.Length)
        {
            yield return StartCoroutine(SpawnWave(_waves[_currentWaveIndex]));
            
            _currentWaveIndex++;
            
            if (_currentWaveIndex < _waves.Length)
            {
                yield return new WaitForSeconds(_waves[_currentWaveIndex - 1].delayAfterWave);
            }
        }
        
        _isSpawning = false;
        OnAllWavesComplete();
    }
    
    private IEnumerator SpawnWave(Wave wave)
    {
        for (int i = 0; i < wave.enemies.Length; i++)
        {
            var enemyData = wave.enemies[i];
            if (enemyData.enemyPrefab != null)
            {
                for (int j = 0; j < enemyData.count; j++)
                {
                    yield return StartCoroutine(SpawnEnemyWithWait(enemyData.enemyPrefab));
                    yield return new WaitForSeconds(wave.spawnInterval);
                }
            }
        }
    }
    
    private IEnumerator SpawnEnemyWithWait(GameObject enemyPrefab)
    {
        Transform spawnPoint = null;
        
        // Ждем пока не найдем свободную точку спавна
        while (spawnPoint == null)
        {
            spawnPoint = GetNextAvailableSpawnPoint();
            
            if (spawnPoint == null)
            {
                // Все точки заняты, ждем 0.1 секунды и пробуем снова
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        SpawnEnemy(enemyPrefab, spawnPoint);
    }
    
    private Transform GetNextAvailableSpawnPoint()
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
            return null;
        
        int checkedPoints = 0;
        
        // Проверяем точки по кругу, начиная с текущего индекса
        while (checkedPoints < _spawnPoints.Length)
        {
            Transform point = _spawnPoints[_currentSpawnPointIndex];
            
            // Проверяем, свободна ли точка
            if (IsSpawnPointAvailable(point.position))
            {
                // Двигаем индекс к следующей точке для следующего спавна
                _currentSpawnPointIndex = (_currentSpawnPointIndex + 1) % _spawnPoints.Length;
                return point;
            }
            
            // Переходим к следующей точке
            _currentSpawnPointIndex = (_currentSpawnPointIndex + 1) % _spawnPoints.Length;
            checkedPoints++;
        }
        
        // Все точки заняты
        return null;
    }
    
    private bool IsSpawnPointAvailable(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, _spawnCheckRadius);
        
        for (int i = 0; i < colliders.Length; i++)
        {
            // Проверяем, есть ли в радиусе враг с компонентом RotateToNearestTarget
            if (colliders[i].GetComponent<RotateToNearestTarget>() != null)
            {
                return false; // Точка занята
            }
        }
        
        return true; // Точка свободна
    }
    
    private void SpawnEnemy(GameObject enemyPrefab, Transform spawnPoint)
    {
        if (!_enemyPools.TryGetValue(enemyPrefab, out var pool))
        {
            Debug.LogError($"Pool not found for enemy prefab: {enemyPrefab.name}");
            return;
        }
        
        var enemy = pool.GetFreeElement(false);
        enemy.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        enemy.gameObject.SetActive(true);
    }
    
    private void OnAllWavesComplete()
    {
        Debug.Log("All waves completed!");
    }
    
    public void StopWaves()
    {
        StopAllCoroutines();
        _isSpawning = false;
    }
    
    public int GetCurrentWaveIndex() => _currentWaveIndex;
    public int GetTotalWaves() => _waves?.Length ?? 0;
    public bool IsSpawning() => _isSpawning;
}