using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public static event Action<int> OnDamage;
    public static event Action<int> OnResourcesChanged;
    public static event Action<int> OnScoreChanged;
    
    private int _playerHP = 30;
    private int _generalResources = 250;
    private int _score = 0;
    public int Resources => _generalResources;
    public int Score => _score;

    private float _gameSpeed = 1f;
    public float GameSpeed => _gameSpeed;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    
    private void OnEnable()
    {
        Mob.OnBreach += HandleEnemyReachEnd;
        Mob.OnDeath += HandleEnemyDestroyed;
    }
    
    private void OnDisable()
    {
        Mob.OnBreach -= HandleEnemyReachEnd;
        Mob.OnDeath -= HandleEnemyDestroyed;
    }

    private void Start()
    {
        OnDamage?.Invoke(_playerHP);
        OnResourcesChanged?.Invoke(_generalResources);
        OnScoreChanged?.Invoke(_score);
    }
    
    private void HandleEnemyReachEnd(EnemyData data)
    {
        _playerHP = Mathf.Max(0, _playerHP - data.damage);
        OnDamage?.Invoke(_playerHP);
    }

    private void HandleEnemyDestroyed(Mob mob)
    {
        AddResources(Mathf.RoundToInt(mob.Data.generalValue));
        UpdateScore(Mathf.RoundToInt(mob.Data.generalValue));
    }

    public void AddResources(int amount)
    {
        _generalResources += amount;
        OnResourcesChanged?.Invoke(_generalResources);
    }

    private void UpdateScore(int amount)
    {
        _score += amount;
        OnScoreChanged?.Invoke(_score);
    }

    public void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
    }

    public void SetGameSpeed(float newSpeed)
    {
        _gameSpeed = newSpeed;
        SetTimeScale(_gameSpeed);
    }

    public bool SpentResources(int amount)
    {
        if (_generalResources >= amount)
        {
            _generalResources -= amount;
            OnResourcesChanged?.Invoke(_generalResources);
            return true;
        }
        return false;
    }
}
