using System;
using UnityEngine;

public class Mob : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    public EnemyData Data => enemyData;

    public static event Action<EnemyData> OnBreach;
    public static event Action<Mob> OnDeath;

    private Path _currentPath;

    private Vector3 _targetPosition;
    private int _currentWaypoint;
    [Header("Enemy hp value")]
    [SerializeField] private float _hp;
    public float CurrentHP => _hp;
    private float _maxHP;
    public float MaxHP => _maxHP;

    [Header("Health Bar")]
    [SerializeField] private EnemyHealthBar healthBar;

    private bool _isGone = false;

    private void Awake()
    {
        // Path objesinin adının sahnede "Path_Map1" olduğundan emin ol veya null check ekle
        GameObject pathObj = GameObject.Find("Path_Map1");
        if (pathObj != null)
        {
            _currentPath = pathObj.GetComponent<Path>();
        }
    }

    private void OnEnable()
    {
        // Can eşitleme kodunu buradan kaldırdık.
        // _hp = enemyData.hp;  <-- SİLİNEN SATIR

        if (_currentPath != null) // Hata almamak için kontrol
        {
            _currentWaypoint = 0;
            _targetPosition = _currentPath.GetWaypointPosition(_currentWaypoint);
        }
    }

    // ... Update ve TakeDamage kısımları aynı kalacak ...

    // Initialize fonksiyonu Spawner tarafından çağrıldığı için canı burada belirliyoruz
    public void Initialize(float hpMultiplier)
    {
        _isGone = false;

        // Can değeri burada hesaplanıyor ve OnEnable bunu ezmiyor
        _maxHP = enemyData.hp * hpMultiplier;
        _hp = _maxHP;

        // Health bar'ı initialize et
        if (healthBar != null)
        {
            healthBar.Initialize(_maxHP);
            healthBar.Show();
        }
    }

    // Update ve TakeDamage fonksiyonlarını olduğu gibi koruyabilirsin.
    void Update()
    {
        if (_isGone || _currentPath == null) return;

        Vector3 movementDirection = _targetPosition - transform.position;
        float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);

        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, enemyData.moveSpeed * Time.deltaTime);
        float distanceToWaypoint = (movementDirection).magnitude;
        if (distanceToWaypoint <= 0.1f)
        {
            if (_currentWaypoint < _currentPath.waypoints.Length - 1)
            {
                _currentWaypoint++;
                _targetPosition = _currentPath.GetWaypointPosition(_currentWaypoint);
            }
            else
            {
                _isGone = true;
                OnBreach?.Invoke(enemyData);
                gameObject.SetActive(false);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (_isGone) return;

        _hp -= damage;
        _hp = Mathf.Max(_hp, 0);

        // Health bar'ı güncelle
        if (healthBar != null)
        {
            healthBar.UpdateHealth(_hp);
        }

        if (_hp <= 0)
        {
            _isGone = true;
            if (healthBar != null) healthBar.Hide();
            OnDeath?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}