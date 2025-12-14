using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerCard : MonoBehaviour
{
    [SerializeField] private Image towerImage;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text nameText;

    private TowerData _towerData;
    public static event Action<TowerData> OnTowerSelection;

    private void OnEnable()
    {
        // Kule sayýsý deðiþince UpdateCostUI çalýþsýn
        Platform.OnTowerCountChanged += UpdateCostUI;
    }

    private void OnDisable()
    {
        Platform.OnTowerCountChanged -= UpdateCostUI;
    }

    public void Initialize(TowerData data)
    {
        _towerData = data;
        towerImage.sprite = data.sprite;
        nameText.text = data.towerName;

        // Fiyatý ekrana yazdýr
        UpdateCostUI();
    }

    // Fiyatý hesaplayýp ekrana yazan yeni fonksiyon
    private void UpdateCostUI()
    {
        if (_towerData != null)
        {
            // Platform sýnýfýndaki yeni hesaplama metodunu kullanýyoruz
            int currentCost = Platform.GetTowerCost(_towerData);
            costText.text = currentCost.ToString();
        }
    }

    public void PlaceTower()
    {
        OnTowerSelection?.Invoke(_towerData);
    }
}