using System.Collections.Generic;
using UnityEngine;

public enum TargetingMode
{
    First,      // İlk giren düşman
    Closest,    // En yakın düşman
    LowestHP,   // En az canlı düşman
    HighestHP,  // En çok canlı düşman
    Last        // Son giren düşman
}

public class Tower : MonoBehaviour
{
    [SerializeField] private TowerData data;

    [Header("Görsel Ayarlar")]
    [SerializeField] private float rotationSpeed = 25f;

    [Header("Hedefleme")]
    public TargetingMode targetingMode = TargetingMode.First;

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

        // TowerLevelSystem Awake'te �al��t��� i�in veriler burada haz�rd�r
        float initialRange = _levelSystem ? _levelSystem.currentRange : data.range;
        _circleCollider.radius = initialRange;

        _enemiesInRange = new List<Mob>();
        _projectilePool = GetComponent<ObjectPooler>();

        _shootTimer = _levelSystem ? _levelSystem.currentShootInterval : data.shootInterval;
    }

    private void Update()
    {
        Mob target = GetTarget();

        if (target != null)
        {
            RotateTowardsTarget(target);
        }

        _shootTimer -= Time.deltaTime;
        if (_shootTimer <= 0)
        {
            float interval = _levelSystem ? _levelSystem.currentShootInterval : data.shootInterval;
            _shootTimer = interval;
            Shoot();
        }
    }

    private void RotateTowardsTarget(Mob target)
    {
        Vector3 direction = target.transform.position - transform.position;
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
        Mob target = GetTarget();
        if (target != null)
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

    // Hedefleme moduna göre en uygun düşmanı seç
    private Mob GetTarget()
    {
        if (_enemiesInRange.Count == 0) return null;

        // Null olanları temizle
        _enemiesInRange.RemoveAll(m => m == null || !m.gameObject.activeInHierarchy);
        if (_enemiesInRange.Count == 0) return null;

        switch (targetingMode)
        {
            case TargetingMode.First:
                return _enemiesInRange[0];

            case TargetingMode.Last:
                return _enemiesInRange[_enemiesInRange.Count - 1];

            case TargetingMode.Closest:
                Mob closest = _enemiesInRange[0];
                float closestDist = Vector3.Distance(transform.position, closest.transform.position);
                foreach (var mob in _enemiesInRange)
                {
                    float dist = Vector3.Distance(transform.position, mob.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = mob;
                    }
                }
                return closest;

            case TargetingMode.LowestHP:
                Mob lowestHP = _enemiesInRange[0];
                foreach (var mob in _enemiesInRange)
                {
                    if (mob.CurrentHP < lowestHP.CurrentHP)
                    {
                        lowestHP = mob;
                    }
                }
                return lowestHP;

            case TargetingMode.HighestHP:
                Mob highestHP = _enemiesInRange[0];
                foreach (var mob in _enemiesInRange)
                {
                    if (mob.CurrentHP > highestHP.CurrentHP)
                    {
                        highestHP = mob;
                    }
                }
                return highestHP;

            default:
                return _enemiesInRange[0];
        }
    }

    // Hedefleme modunu değiştir (UI için)
    public void CycleTargetingMode()
    {
        int nextMode = ((int)targetingMode + 1) % 5;
        targetingMode = (TargetingMode)nextMode;
    }

    // Hedefleme modu ismini al (UI için)
    public string GetTargetingModeName()
    {
        switch (targetingMode)
        {
            case TargetingMode.First: return "İLK";
            case TargetingMode.Last: return "SON";
            case TargetingMode.Closest: return "YAKIN";
            case TargetingMode.LowestHP: return "AZ CAN";
            case TargetingMode.HighestHP: return "ÇOK CAN";
            default: return "İLK";
        }
    }
}