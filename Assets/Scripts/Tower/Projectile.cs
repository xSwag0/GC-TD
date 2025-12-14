using UnityEngine;

public class Projectile : MonoBehaviour
{
    private TowerData _data;
    private Vector3 _shootDirection;
    private float _projectileDuration;
    private float _currentDamage;

    void Update()
    {
        if (_projectileDuration <= 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            _projectileDuration -= Time.deltaTime;
            transform.position += _shootDirection * _data.projectileSpeed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Mob"))
        {
            Mob mob = collision.GetComponent<Mob>();
            mob.TakeDamage(_currentDamage);
            gameObject.SetActive(false);
        }
    }

    public void Shoot(TowerData data, Vector3 shootDirection, float damageOverride)
    {
        _data = data;
        _shootDirection = shootDirection;
        _projectileDuration = _data.projectileDuration;
        _currentDamage = damageOverride;

        transform.localScale = Vector3.one * _data.projectileSize;

        float angle = Mathf.Atan2(_shootDirection.y, _shootDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
    }
}