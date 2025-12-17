using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Health Bar Components")]
    [SerializeField] private Transform fillBar;
    [SerializeField] private SpriteRenderer fillRenderer;

    [Header("Colors")]
    [SerializeField] private Color highHealthColor = Color.green;
    [SerializeField] private Color mediumHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;

    [Header("Settings")]
    [SerializeField] private float barWidth = 0.5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0.4f, 0);

    private float _maxHealth;
    private float _currentHealth;

    private void LateUpdate()
    {
        // Health bar'ı her zaman yukarı baksın (world space)
        transform.rotation = Quaternion.identity;
        transform.localPosition = offset;
    }

    public void Initialize(float maxHealth)
    {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
        UpdateBar();
    }

    public void UpdateHealth(float currentHealth)
    {
        _currentHealth = currentHealth;
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (fillBar == null) return;

        float healthPercent = Mathf.Clamp01(_currentHealth / _maxHealth);

        // Bar genişliğini ayarla
        fillBar.localScale = new Vector3(healthPercent * barWidth, fillBar.localScale.y, fillBar.localScale.z);

        // Bar pozisyonunu sola hizala (pivot sol tarafta gibi davransın)
        float xOffset = -(1 - healthPercent) * barWidth / 2f;
        fillBar.localPosition = new Vector3(xOffset, fillBar.localPosition.y, fillBar.localPosition.z);

        // Renge göre güncelle
        if (fillRenderer != null)
        {
            if (healthPercent > 0.6f)
                fillRenderer.color = highHealthColor;
            else if (healthPercent > 0.3f)
                fillRenderer.color = mediumHealthColor;
            else
                fillRenderer.color = lowHealthColor;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
