using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private TowerData data;

    [Header("Görsel Ayarlar")]
    [SerializeField] private float rotationSpeed = 25f;

    private CircleCollider2D _circleCollider;
    private List<Mob> _enemiesInRange;
    private ObjectPooler _projectilePool;
    private TowerLevelSystem _levelSystem;

    private float _shootTimer;

    private void OnEnable()
    {
        Mob.OnDeath += OnEnemyDestroyed;
    }

    private void OnDisable()
    {
        Mob.OnDeath -= OnEnemyDestroyed;
    }

    private void Start()
    {
        _levelSystem = GetComponent<TowerLevelSystem>();
        _circleCollider = GetComponent<CircleCollider2D>();

        // TowerLevelSystem Awake'te çalýþtýðý için veriler burada hazýrdýr
        float initialRange = _levelSystem ? _levelSystem.currentRange : data.range;
        _circleCollider.radius = initialRange;

        _enemiesInRange = new List<Mob>();
        _projectilePool = GetComponent<ObjectPooler>();

        _shootTimer = _levelSystem ? _levelSystem.currentShootInterval : data.shootInterval;
    }

    private void Update()
    {
        // Menzil güncelleme kodu yok, çünkü menzil deðiþmiyor.

        if (_enemiesInRange.Count > 0 && _enemiesInRange[0] != null)
        {
            RotateTowardsTarget();
        }

        _shootTimer -= Time.deltaTime;
        if (_shootTimer <= 0)
        {
            float interval = _levelSystem ? _levelSystem.currentShootInterval : data.shootInterval;
            _shootTimer = interval;
            Shoot();
        }
    }

    private void RotateTowardsTarget()
    {
        Vector3 direction = _enemiesInRange[0].transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, data.range);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Mob"))
        {
            _enemiesInRange.Add(collision.GetComponent<Mob>());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Mob"))
        {
            Mob mob = collision.GetComponent<Mob>();
            if (_enemiesInRange.Contains(mob))
            {
                _enemiesInRange.Remove(mob);
            }
        }
    }

    private void Shoot()
    {
        if (_enemiesInRange.Count > 0 && _enemiesInRange[0] != null)
        {
            GameObject projectileObj = _projectilePool.GetObjectFromPool();
            projectileObj.transform.position = transform.position;
            projectileObj.SetActive(true);

            Vector3 shootDirection = transform.up;
            Projectile projectile = projectileObj.GetComponent<Projectile>();

            float damageToDeal = _levelSystem ? _levelSystem.currentDamage : data.damage;
            projectile.Shoot(data, shootDirection, damageToDeal);
        }
    }

    private void OnEnemyDestroyed(Mob mob)
    {
        if (_enemiesInRange.Contains(mob))
        {
            _enemiesInRange.Remove(mob);
        }
    }
}