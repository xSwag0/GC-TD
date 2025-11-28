using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private WaveData[] waves;
    private int _currentWaveIndex = 0;
    private WaveData CurrentWave => waves[_currentWaveIndex];
    
    private float _spawnTimer;
    private int _spawnedEnemies;
    private int _spawnedBreaches;
    
    private float _waitingTimer;
    private float _cooldownTime = 3f;
    private bool _isOnCooldown = false;
    
    [SerializeField] private ObjectPooler goblinPool;
    [SerializeField] private ObjectPooler impPool;
    [SerializeField] private ObjectPooler hobgoblinPool;
    
    private Dictionary<EnemyType, ObjectPooler> _poolDictionary;

    private void Awake()
    {
        _poolDictionary = new Dictionary<EnemyType, ObjectPooler>()
        {
            { EnemyType.Goblin, goblinPool },
            { EnemyType.Imp, impPool },
            { EnemyType.Hobgoblin, hobgoblinPool }
        };
    }

    private void OnEnable()
    {
        Mob.OnBreach += ReactOnBreach;
    }
    
    private void OnDisable()
    {
        Mob.OnBreach -= ReactOnBreach;
    }
    
    void Update()
    {
        if (_isOnCooldown == true)
        {
            _waitingTimer -= Time.deltaTime;
            if (_waitingTimer <= 0)
            {
                _currentWaveIndex = (_currentWaveIndex + 1) % waves.Length;
                _spawnedEnemies = 0;
                _spawnedBreaches = 0;
                _isOnCooldown = false;
                _spawnTimer = 0;
            }
        }
        else
        {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0 && _spawnedEnemies < CurrentWave.enemyCount)
            {
                _spawnTimer = CurrentWave.spawnInterval;
                SpawnMob();
                _spawnedEnemies++;
            }else if (_spawnedEnemies >= CurrentWave.enemyCount && _spawnedBreaches >= CurrentWave.enemyCount)
            {
                _isOnCooldown = true;
                _waitingTimer = _cooldownTime;
            }
        }
    }

    private void SpawnMob()
    {
        if(_poolDictionary.TryGetValue(CurrentWave.enemyType, out var currentPool)== true )
        {
            GameObject spawnedObject = currentPool.GetObjectFromPool();
            spawnedObject.transform.position = transform.position;
            spawnedObject.SetActive(true);
        }
    }

    private void ReactOnBreach(EnemyData enemyData)
    {
        _spawnedBreaches++;
    }
}
