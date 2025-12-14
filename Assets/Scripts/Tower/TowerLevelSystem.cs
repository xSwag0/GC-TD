using UnityEngine;

public class TowerLevelSystem : MonoBehaviour
{
    public TowerData towerData;

    [Header("Durum")]
    public int currentLevel = 1;
    public int maxLevel = 4;
    public int totalMoneySpent = 0; // Ýade hesabý için

    [Header("Stats")]
    public float currentDamage;
    public float currentShootInterval;
    public float currentRange;

    [Header("Ayarlar")]
    public int baseUpgradeCost = 75; // Inspector'dan da ayarlayabilirsin
    public float damageMultiplier = 1.2f;
    public float speedMultiplier = 0.9f;
    public float sellRefundRate = 0.5f;

    // Týklama Spam Korumasý
    private float _lastUpgradeTime;
    private const float UpgradeCooldown = 0.5f;

    void Awake()
    {
        if (towerData != null)
        {
            currentDamage = towerData.damage;
            currentShootInterval = towerData.shootInterval;
            currentRange = towerData.range;
            totalMoneySpent = towerData.cost;
        }
    }

    public void SetInitialCost(int cost)
    {
        totalMoneySpent = cost;
    }

    public void TryUpgradeTower()
    {
        // 1. Spam Kontrolü (Oyun duraklatýlsa bile çalýþmasý için unscaledTime kullandýk)
        if (Time.unscaledTime - _lastUpgradeTime < UpgradeCooldown) return;

        // 2. Max Level Kontrolü
        if (currentLevel >= maxLevel) return;

        // 3. Maliyet Hesabý
        int cost = CalculateCost();

        // 4. Para ve Yükseltme Ýþlemi
        if (GameManager.Instance != null && GameManager.Instance.Resources >= cost)
        {
            GameManager.Instance.SpentResources(cost);

            PerformUpgrade(cost);

            // Son iþlem zamanýný güncelle
            _lastUpgradeTime = Time.unscaledTime;

            
        }
    }

    void PerformUpgrade(int costSpent)
    {
        currentLevel++;
        totalMoneySpent += costSpent; // Harcanan parayý iade havuzuna ekle

        currentDamage *= damageMultiplier;
        currentShootInterval *= speedMultiplier;
    }

    public int CalculateCost()
    {
        return baseUpgradeCost * currentLevel;
    }

    public int GetRefundAmount()
    {
        return Mathf.FloorToInt(totalMoneySpent * sellRefundRate);
    }
}