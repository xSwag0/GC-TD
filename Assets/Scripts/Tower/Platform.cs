using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class Platform : MonoBehaviour
{
    public static event Action<Platform> OnPlatformClicked;
    public static event Action OnTowerCountChanged;

    [SerializeField] private LayerMask platformLayerMask;
    public static bool sidePanelOpen { get; set; } = false;

    private static int towerCount = 0;
    private static int costIncreaseAmount = 20;

    public bool hasTower = false;
    private GameObject _builtTower;
    private TowerData _builtTowerData;

    public GameObject BuiltTower => _builtTower;

    public static int GetTowerCost(TowerData data)
    {
        return data.cost + (towerCount * costIncreaseAmount);
    }

    private void Update()
    {
        // sidePanelOpen kontrolünü buradan SİLDİK.
        // Sadece oyun duraklatılmışsa tıklamayı engelle.
        if (UIControl._isPaused) return;

        // Eğer fare şu an bir UI elemanının (Panel, Buton vb.) üzerindeyse,
        // platforma tıklamayı engelle. Bu sayede butona basarken kule seçmezsin.
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
                    // Bu olay tetiklendiğinde UIControl devreye girecek
                    // ve _currentPlatform'u yenileyip fiyatları güncelleyecek.
                    OnPlatformClicked?.Invoke(platform);
                }
            }
        }
    }

    public void PlaceTower(TowerData data)
    {
        // 1. Önce maliyeti hesapla
        int actualCost = GetTowerCost(data);

        // 2. Kuleyi yarat
        _builtTower = Instantiate(data.prefab, transform.position, Quaternion.identity, transform);
        _builtTowerData = data;
        hasTower = true;

        // 3. Kuleye ne kadara mal olduğunu bildir (İade sistemi için kritik düzeltme)
        TowerLevelSystem levelSystem = _builtTower.GetComponent<TowerLevelSystem>();
        if (levelSystem != null)
        {
            levelSystem.SetInitialCost(actualCost);
        }

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
                // Artık levelSys gerçek harcanan parayı bildiği için doğru iade yapacak
                refundAmount = levelSys.GetRefundAmount();
            }
            else
            {
                // Yedek plan (Eğer level sistemi yoksa)
                refundAmount = Mathf.RoundToInt(_builtTowerData.cost * 0.5f);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddResources(refundAmount);
            }

            Destroy(_builtTower);
            hasTower = false;
            _builtTower = null;

            towerCount--;
            OnTowerCountChanged?.Invoke();
        }
    }

    public static void ResetTowerCount()
    {
        towerCount = 0;
        OnTowerCountChanged?.Invoke();
    }
}