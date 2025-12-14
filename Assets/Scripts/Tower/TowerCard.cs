using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerCard : MonoBehaviour
{
    [SerializeField] private Image towerImage;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text nameText;

    private Button _button;
    private TowerData _towerData;
    public static event Action<TowerData> OnTowerSelection;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
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

        UpdateCostUI();
    }

    private void UpdateCostUI()
    {
        if (_towerData != null)
        {
            int currentCost = Platform.GetTowerCost(_towerData);

            bool limitReached = Platform.IsLimitReached(_towerData);

            if (limitReached)
            {
                costText.text = "MAX";
                costText.color = Color.red;
                if (_button) _button.interactable = false;
            }
            else
            {
                costText.text = currentCost.ToString();
                costText.color = Color.white;
                if (_button) _button.interactable = true;
            }
        }
    }

    public void PlaceTower()
    {
        if (!Platform.IsLimitReached(_towerData))
        {
            OnTowerSelection?.Invoke(_towerData);
        }
    }
}