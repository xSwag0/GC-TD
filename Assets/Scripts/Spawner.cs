using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public class Spawner : MonoBehaviour
{
    public static event Action<int> OnWaveChange;
    
    private WaveData _currentWaveData;
    
    private List<EnemyType> _currentWaveEnemies = new List<EnemyType>();

    private int _waveCounter = 0;

    private float _spawnTimer;
    private int _spawnedEnemies;
    private int _spawnedBreaches;

    private float _waitingTimer;
    private float _cooldownTime = 5f;
    private bool _isOnCooldown = false;

    [Header("Enemy Pools")]
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
        Mob.OnDeath += ReactOnDeath;
    }

    private void OnDisable()
    {
        Mob.OnBreach -= ReactOnBreach;
        Mob.OnDeath -= ReactOnDeath;
    }

    private void Start()
    {
        GenerateNextWave();
        OnWaveChange?.Invoke(_waveCounter);
    }

    void Update()
    {
        if (_isOnCooldown == true)
        {
            _waitingTimer -= Time.deltaTime;
            if (_waitingTimer <= 0)
            {
                _waveCounter++;
                GenerateNextWave();

                _spawnedEnemies = 0;
                _spawnedBreaches = 0;
                _isOnCooldown = false;
                _spawnTimer = 0;

                OnWaveChange?.Invoke(_waveCounter);
            }
        }
        else
        {
            _spawnTimer -= Time.deltaTime;

            
            if (_spawnTimer <= 0 && _spawnedEnemies < _currentWaveEnemies.Count)
            {
                _spawnTimer = _currentWaveData.spawnInterval;
                SpawnMob();
                _spawnedEnemies++;
            }
            else if (_spawnedEnemies >= _currentWaveEnemies.Count && _spawnedBreaches >= _currentWaveEnemies.Count)
            {
                // Wave sonu ödülü: wave * 100 altın
                int waveReward = _waveCounter * 100;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddResources(waveReward);
                }

                _isOnCooldown = true;
                _waitingTimer = _cooldownTime;
            }
        }
    }

    private void GenerateNextWave()
    {
        _currentWaveData = ScriptableObject.CreateInstance<WaveData>();

        // Linear düşman artışı, max 60 cap
        int totalEnemyCount = Mathf.Min(60, 10 + (_waveCounter * 3));
        _currentWaveData.enemyCount = totalEnemyCount;
        // Spawn interval minimum 0.3s (daha rahat oynanış)
        _currentWaveData.spawnInterval = Mathf.Max(0.3f, 1.5f - (_waveCounter * 0.04f));
        
        _currentWaveEnemies.Clear(); 
        List<EnemyType> availableTypes = new List<EnemyType>(_poolDictionary.Keys);
        
        for (int i = 0; i < totalEnemyCount; i++)
        {
            EnemyType selectedType;
            
            selectedType = availableTypes[Random.Range(0, availableTypes.Count)];

            _currentWaveEnemies.Add(selectedType);
        }
    }

    private void SpawnMob()
    {
        EnemyType typeToSpawn = _currentWaveEnemies[_spawnedEnemies];

        if (_poolDictionary.TryGetValue(typeToSpawn, out var currentPool))
        {
            if (currentPool != null)
            {
                GameObject spawnedObject = currentPool.GetObjectFromPool();
                spawnedObject.transform.position = transform.position;

                // Dengelenmiş HP artışı
                float hpMultiplier;
                if(_waveCounter <= 14)
                {
                    hpMultiplier = 1f + (_waveCounter * 0.4f);
                }
                else
                {
                    // Wave 15+ için daha yumuşak artış (1.3 -> 0.7)
                    hpMultiplier = 1f + (_waveCounter * 0.7f);
                }
                    Mob mob = spawnedObject.GetComponent<Mob>();
                if (mob != null) mob.Initialize(hpMultiplier);

                spawnedObject.SetActive(true);
            }
        }
    }

    private void ReactOnBreach(EnemyData enemyData) { _spawnedBreaches++; }
    private void ReactOnDeath(Mob mob) { _spawnedBreaches++; }
}