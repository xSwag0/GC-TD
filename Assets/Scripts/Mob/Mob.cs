using System;
using UnityEngine;
using UnityEngine.UI;

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
    public float CurrentHP => _hp; // Yeni özellik: Canı dışarıdan okumak için

    private float _maxHP;
    public float MaxHP => _maxHP; // Yeni özellik: Maksimum canı dışarıdan okumak için

    [Header("Health Bar")]
    // Eski "Image healthBarFill" yerine yeni script yapısını kullanıyoruz
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
        if (_currentPath != null)
        {
            _currentWaypoint = 0;
            _targetPosition = _currentPath.GetWaypointPosition(_currentWaypoint);
        }
    }

    public void Initialize(float hpMultiplier)
    {
        _isGone = false;

        // Yeni değişken isimlerini (_maxHP) kullanıyoruz
        _maxHP = enemyData.hp * hpMultiplier;
        _hp = _maxHP;

        // Yeni HealthBar scriptini başlatıyoruz
        if (healthBar != null)
        {
            healthBar.Initialize(_maxHP);
            healthBar.Show();
        }
    }

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

        // Eski UpdateHealthBar() yerine yeni sistemin metodunu çağırıyoruz
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

    // Eski 'UpdateHealthBar' metodunu sildik çünkü artık 'EnemyHealthBar' scripti bu işi yapıyor.
}