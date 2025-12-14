using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIControl : MonoBehaviour
{
    [SerializeField] private TMP_Text waveInfo;
    [SerializeField] private TMP_Text HPInfo;
    [SerializeField] private TMP_Text resourcesInfo;
    [SerializeField] private TMP_Text scoreInfo;

    [SerializeField] private GameObject sidePanel;
    [SerializeField] private GameObject towerCardPrefab;
    [SerializeField] private Transform cardsContainer;

    [SerializeField] private TowerData[] towers;
    private List<GameObject> activeCards = new List<GameObject>();

    private Platform _currentPlatform;

    [SerializeField] private Button speedX1Button;
    [SerializeField] private Button speedX2Button;
    [SerializeField] private Button speedX3Button;
    [SerializeField] private Button speedX4Button;

    [SerializeField] private GameObject PauseMenu;
    public static bool _isPaused { get; private set; } = false;
    [SerializeField] private GameObject GameOverMenu;

    [SerializeField] private TMP_Text gameOverMenuScore;

    [SerializeField] private Button sellButton;

    [Header("Upgrade Settings")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text upgradeButtonText;
    private TowerData _builtTowerData;

    private void OnEnable()
    {
        Spawner.OnWaveChange += UpdateWaveInfo;
        GameManager.OnDamage += UpdateHPInfo;
        GameManager.OnResourcesChanged += UpdateResourcesInfo;
        GameManager.OnScoreChanged += UpdateScoreInfo;
        Platform.OnPlatformClicked += HandlePlatformClicked;
        TowerCard.OnTowerSelection += HandleTowerSelection;
    }

    private void OnDisable()
    {
        Spawner.OnWaveChange -= UpdateWaveInfo;
        GameManager.OnDamage -= UpdateHPInfo;
        GameManager.OnResourcesChanged -= UpdateResourcesInfo;
        GameManager.OnScoreChanged -= UpdateScoreInfo;
        Platform.OnPlatformClicked -= HandlePlatformClicked;
        TowerCard.OnTowerSelection -= HandleTowerSelection;
    }

    private void Start()
    {
        _isPaused = false;
        Platform.sidePanelOpen = false;

        GameManager.Instance.SetTimeScale(0f);

        speedX1Button.onClick.AddListener(() => SetGameSpeed(0.5f));
        speedX2Button.onClick.AddListener(() => SetGameSpeed(1f));
        speedX3Button.onClick.AddListener(() => SetGameSpeed(2f));
        speedX4Button.onClick.AddListener(() => SetGameSpeed(10f));

        sellButton.gameObject.SetActive(false);
        if (upgradeButton) upgradeButton.gameObject.SetActive(false);

        sellButton.onClick.AddListener(OnSellButtonClicked);
    }

    private void UpdateWaveInfo(int currentWave) { waveInfo.text = $"Wave = {currentWave + 1}"; }

    private void UpdateHPInfo(int currentHP)
    {
        HPInfo.text = $"HP = {currentHP}";
        if (currentHP <= 0)
        {
            CreateGameOverScore(GameManager.Instance.Score);
            ShowGameOverMenu();
        }
    }

    private void UpdateResourcesInfo(int currentResources) { resourcesInfo.text = $"Gold = {currentResources}"; }
    private void UpdateScoreInfo(int currentScore) { scoreInfo.text = $"Score = {currentScore}"; }
    private void CreateGameOverScore(int currentScore) { gameOverMenuScore.text = $"Score = {currentScore}"; }

    private void HandlePlatformClicked(Platform platform)
    {
        _currentPlatform = platform;
        ShowSidePanel();
    }

    private void ShowSidePanel()
    {
        sidePanel.SetActive(true);
        Platform.sidePanelOpen = true;

        if (_currentPlatform.hasTower)
        {
            cardsContainer.gameObject.SetActive(false);
            sellButton.gameObject.SetActive(true);

            if (upgradeButton)
            {
                upgradeButton.gameObject.SetActive(true);
                UpdateUpgradeButtonState();
            }
        }
        else
        {
            sellButton.gameObject.SetActive(false);
            if (upgradeButton) upgradeButton.gameObject.SetActive(false);

            cardsContainer.gameObject.SetActive(true);
            PopulateTowerCards();
        }
    }

    private void UpdateUpgradeButtonState()
    {
        if (_currentPlatform != null && _currentPlatform.BuiltTower != null)
        {
            TowerLevelSystem levelSys = _currentPlatform.BuiltTower.GetComponent<TowerLevelSystem>();
            if (levelSys != null && upgradeButtonText != null)
            {
                if (levelSys.currentLevel >= levelSys.maxLevel)
                {
                    upgradeButtonText.text = "MAX";
                    upgradeButton.interactable = false;
                }
                else
                {
                    int cost = levelSys.CalculateCost();
                    upgradeButtonText.text = $"UPGRADE\n{cost} G";
                    upgradeButton.interactable = true;
                }
            }
        }
    }

    // Temizlenmiþ Upgrade Fonksiyonu
    public void OnUpgradeButtonClicked()
    {
        if (_currentPlatform != null && _currentPlatform.BuiltTower != null)
        {
            TowerLevelSystem levelSys = _currentPlatform.BuiltTower.GetComponent<TowerLevelSystem>();
            if (levelSys != null)
            {
                levelSys.TryUpgradeTower();
                UpdateUpgradeButtonState();
            }
        }
    }

    public void OnSellButtonClicked()
    {
        if (_currentPlatform != null && _currentPlatform.hasTower)
        {
            _currentPlatform.SellTower();
            HideSidePanel();
        }
    }

    public void HideSidePanel()
    {
        sidePanel.SetActive(false);
        Platform.sidePanelOpen = false;
    }

    private void PopulateTowerCards()
    {
        foreach (var card in activeCards) Destroy(card);
        activeCards.Clear();
        foreach (var data in towers)
        {
            GameObject cardGameObject = Instantiate(towerCardPrefab, cardsContainer);
            TowerCard card = cardGameObject.GetComponent<TowerCard>();
            card.Initialize(data);
            activeCards.Add(cardGameObject);
        }
    }

    private void HandleTowerSelection(TowerData towerData)
    {
        if (_currentPlatform.hasTower)
        {
            HideSidePanel();
        }
        else
        {
            int currentCost = Platform.GetTowerCost(towerData);

            if (GameManager.Instance.Resources >= currentCost)
            {
                GameManager.Instance.SpentResources(currentCost);
                _currentPlatform.PlaceTower(towerData);
            }
        }
        HideSidePanel();
    }

    private void SetGameSpeed(float timeScale)
    {
        GameManager.Instance.SetGameSpeed(timeScale);
    }

    public void TogglePause()
    {
        if (_isPaused)
        {
            PauseMenu.SetActive(false);
            _isPaused = false;
            GameManager.Instance.SetTimeScale(GameManager.Instance.GameSpeed);
        }
        else
        {
            PauseMenu.SetActive(true);
            _isPaused = true;
            GameManager.Instance.SetTimeScale(0f);
        }
    }

    public void RestartGame()
    {
        GameManager.Instance.SetTimeScale(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Platform.ResetTowerCount();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoToMainMenu()
    {
        GameManager.Instance.SetTimeScale(1f);
        Platform.ResetTowerCount();
        SceneManager.LoadScene("MainMenu");
    }

    private void ShowGameOverMenu()
    {
        GameManager.Instance.SetTimeScale(0f);
        GameOverMenu.SetActive(true);
    }
}