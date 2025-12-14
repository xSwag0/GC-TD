using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class Platform : MonoBehaviour
{
    public static event Action<Platform> OnPlatformClicked;
    public static event Action OnTowerCountChanged;

    [SerializeField] private LayerMask platformLayerMask;
    public static bool sidePanelOpen { get; set; } = false;

    // GENEL toplam kule sayısı
    private static int towerCount = 0;
    private static int costIncreaseAmount = 25;

    // Kule türüne göre sayıları tutan liste
    private static Dictionary<TowerData, int> _towerTypeCounts = new Dictionary<TowerData, int>();

    public bool hasTower = false;
    private GameObject _builtTower;
    private TowerData _builtTowerData;

    public GameObject BuiltTower => _builtTower;

    public static int GetTowerCost(TowerData data)
    {
        return data.cost + (towerCount * costIncreaseAmount);
    }

    public static int GetSpecificTowerCount(TowerData data)
    {
        if (_towerTypeCounts.ContainsKey(data))
            return _towerTypeCounts[data];
        return 0;
    }

    public static bool IsLimitReached(TowerData data)
    {
        if (data.buildLimit <= 0) return false;
        return GetSpecificTowerCount(data) >= data.buildLimit;
    }

    private void Update()
    {
        if (UIControl._isPaused) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D raycastHit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, platformLayerMask);

            if (raycastHit.collider != null)
            {
                Platform platform = raycastHit.collider.GetComponent<Platform>();
                if (platform != null)
                {
                    OnPlatformClicked?.Invoke(platform);
                }
            }
        }
    }

    public void PlaceTower(TowerData data)
    {
        // 1. Limit Kontrolü
        if (IsLimitReached(data)) return;

        // 2. Maliyeti hesapla
        int actualCost = GetTowerCost(data);

        // 3. Kuleyi yarat
        _builtTower = Instantiate(data.prefab, transform.position, Quaternion.identity, transform);
        _builtTowerData = data;
        hasTower = true;

        TowerLevelSystem levelSystem = _builtTower.GetComponent<TowerLevelSystem>();
        if (levelSystem != null)
        {
            levelSystem.SetInitialCost(actualCost);
        }

        // Tür sayacını artır
        if (!_towerTypeCounts.ContainsKey(data))
            _towerTypeCounts[data] = 0;
        _towerTypeCounts[data]++;

        towerCount++;
        OnTowerCountChanged?.Invoke();
    }

    public void SellTower()
    {
        if (_builtTower != null)
        {
            int refundAmount = 0;
            TowerLevelSystem levelSys = _builtTower.GetComponent<TowerLevelSystem>();

            if (levelSys != null)
            {
                refundAmount = levelSys.GetRefundAmount();
            }
            else
            {
                refundAmount = Mathf.RoundToInt(_builtTowerData.cost * 0.5f);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddResources(refundAmount);
            }

            // Tür sayacını azalt
            if (_builtTowerData != null && _towerTypeCounts.ContainsKey(_builtTowerData))
            {
                _towerTypeCounts[_builtTowerData]--;
            }

            Destroy(_builtTower);
            hasTower = false;
            _builtTower = null;
            _builtTowerData = null;

            towerCount--;
            OnTowerCountChanged?.Invoke();
        }
    }

    public static void ResetTowerCount()
    {
        towerCount = 0;
        _towerTypeCounts.Clear();
        OnTowerCountChanged?.Invoke();
    }
}