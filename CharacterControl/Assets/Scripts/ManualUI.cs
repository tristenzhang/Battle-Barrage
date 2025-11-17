using UnityEngine;
using UnityEngine.UI;

public class ManualUI : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerNumber = 1;
    public KeyCode toggleUIKey = KeyCode.Q;
    
    [Header("UI Panel")]
    public GameObject uiPanel;
    
    [Header("Troop Prefabs")]
    public GameObject balloonTroopPrefab;
    public GameObject attackTroopPrefab;
    
    [Header("Spawn Point")]
    public Transform spawnPoint;
    
    [Header("Balloon Troop Buttons")]
    public Button regularBalloonButton;
    public Button speedBalloonButton;
    public Button tankBalloonButton;
    
    [Header("Attack Troop Buttons")]
    public Button regularAttackButton;
    public Button tankAttackButton;
    public Button longRangeAttackButton;
    
    [Header("Close Button")]
    public Button closeButton;
    
    [Header("Costs")]
    public int regularBalloonCost = 10;
    public int speedBalloonCost = 15;
    public int tankBalloonCost = 20;
    public int regularAttackCost = 25;
    public int tankAttackCost = 40;
    public int longRangeAttackCost = 35;
    
    private GameManager gameManager;
    private bool isUIVisible = false;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        
        // Hide UI at start
        if (uiPanel != null)
            uiPanel.SetActive(false);
        
        SetupButtonListeners();
    }
    
    void SetupButtonListeners()
    {
        // Balloon troop buttons
        if (regularBalloonButton != null)
            regularBalloonButton.onClick.AddListener(() => SpawnBalloonTroop(BalloonTroop.TroopType.Regular, regularBalloonCost));
        
        if (speedBalloonButton != null)
            speedBalloonButton.onClick.AddListener(() => SpawnBalloonTroop(BalloonTroop.TroopType.Speed, speedBalloonCost));
        
        if (tankBalloonButton != null)
            tankBalloonButton.onClick.AddListener(() => SpawnBalloonTroop(BalloonTroop.TroopType.Tank, tankBalloonCost));
        
        // Attack troop buttons
        if (regularAttackButton != null)
            regularAttackButton.onClick.AddListener(() => SpawnAttackTroop("Regular", regularAttackCost));
        
        if (tankAttackButton != null)
            tankAttackButton.onClick.AddListener(() => SpawnAttackTroop("Tank", tankAttackCost));
        
        if (longRangeAttackButton != null)
            longRangeAttackButton.onClick.AddListener(() => SpawnAttackTroop("LongRange", longRangeAttackCost));
        
        // Close button
        if (closeButton != null)
            closeButton.onClick.AddListener(HideUI);
    }

    void Update()
    {
        // Toggle UI with key press
        if (Input.GetKeyDown(toggleUIKey))
        {
            ToggleUI();
        }
    }

    void ToggleUI()
    {
        isUIVisible = !isUIVisible;
        if (uiPanel != null)
            uiPanel.SetActive(isUIVisible);
    }

    void HideUI()
    {
        isUIVisible = false;
        if (uiPanel != null)
            uiPanel.SetActive(false);
    }

    void SpawnBalloonTroop(BalloonTroop.TroopType type, int cost)
    {
        // Check if enough currency
        if (gameManager == null || !gameManager.SpendCurrency(playerNumber, cost))
        {
            Debug.Log("Not enough currency! Need: " + cost + " (Player " + playerNumber + ")");
            return;
        }

        // Check if prefab and spawn point exist
        if (balloonTroopPrefab == null)
        {
            Debug.LogError("Balloon Troop Prefab not assigned!");
            return;
        }
        
        if (spawnPoint == null)
        {
            Debug.LogError("Spawn Point not assigned!");
            return;
        }

        // Spawn the troop
        GameObject troop = Instantiate(balloonTroopPrefab, spawnPoint.position, Quaternion.identity);
        BalloonTroop script = troop.GetComponent<BalloonTroop>();
        
        if (script != null)
        {
            script.ownerPlayerNumber = playerNumber;
            script.troopType = type;
        }
        
        Debug.Log("Player " + playerNumber + " spawned " + type + " balloon troop for " + cost + " currency!");
    }

    void SpawnAttackTroop(string typeStr, int cost)
    {
        // Check if enough currency
        if (gameManager == null || !gameManager.SpendCurrency(playerNumber, cost))
        {
            Debug.Log("Not enough currency! Need: " + cost + " (Player " + playerNumber + ")");
            return;
        }

        // Check if prefab and spawn point exist
        if (attackTroopPrefab == null)
        {
            Debug.LogError("Attack Troop Prefab not assigned!");
            return;
        }
        
        if (spawnPoint == null)
        {
            Debug.LogError("Spawn Point not assigned!");
            return;
        }

        // Spawn the troop
        GameObject troop = Instantiate(attackTroopPrefab, spawnPoint.position, Quaternion.identity);
        
        
        
        Debug.Log("Player " + playerNumber + " spawned " + typeStr + " attack troop for " + cost + " currency!");
    }
}