// BaseHealth.cs - Updated to work with new GameManager
using UnityEngine;
using UnityEngine.UI;

public class BaseHealth : MonoBehaviour
{
    [Header("Base Settings")]
    public int playerNumber = 1;
    public int maxHealth = 100;
    public int currentHealth = 50;
    public int totalHealingReceived = 0;
    public int maxHealingAllowed = 100;
    
    [Header("UI")]
    private Canvas baseCanvas;
    private Slider healthBar;
    private Image healthBarFill;
    private Text healthText;
    private Text healingText;
    private Color baseHealthColor;
    
    void Start()
    {
        baseHealthColor = playerNumber == 1 ? Color.blue : Color.red;
        SetupUI();
        UpdateUI();
    }
    
    void SetupUI()
    {
        GameObject canvasObj = new GameObject("BaseCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = Vector3.up * 3f;
        
        baseCanvas = canvasObj.AddComponent<Canvas>();
        baseCanvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(4, 2);
        
        GameObject bgObj = new GameObject("HealthBarBG");
        bgObj.transform.SetParent(canvasObj.transform);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = Color.black;
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.6f);
        bgRect.anchorMax = new Vector2(1, 0.6f);
        bgRect.sizeDelta = new Vector2(0, 0.4f);
        bgRect.anchoredPosition = new Vector2(0, 0.3f);
        
        GameObject sliderObj = new GameObject("HealthSlider");
        sliderObj.transform.SetParent(bgObj.transform);
        healthBar = sliderObj.AddComponent<Slider>();
        
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;
        
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform);
        healthBarFill = fill.AddComponent<Image>();
        healthBarFill.color = baseHealthColor;
        
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        
        healthBar.fillRect = fillRect;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
        
        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(canvasObj.transform);
        healthText = textObj.AddComponent<Text>();
        healthText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        healthText.fontSize = 22;
        healthText.alignment = TextAnchor.MiddleCenter;
        healthText.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0.3f);
        textRect.anchorMax = new Vector2(1, 0.3f);
        textRect.sizeDelta = new Vector2(0, 0.4f);
        textRect.anchoredPosition = new Vector2(0, 0);
        
        GameObject healObj = new GameObject("HealingText");
        healObj.transform.SetParent(canvasObj.transform);
        healingText = healObj.AddComponent<Text>();
        healingText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        healingText.fontSize = 18;
        healingText.alignment = TextAnchor.MiddleCenter;
        healingText.color = Color.green;
        
        RectTransform healRect = healObj.GetComponent<RectTransform>();
        healRect.anchorMin = new Vector2(0, 0);
        healRect.anchorMax = new Vector2(1, 0);
        healRect.sizeDelta = new Vector2(0, 0.4f);
        healRect.anchoredPosition = new Vector2(0, -0.3f);
    }
    
    void Update()
    {
        if (Camera.main != null && baseCanvas != null)
        {
            baseCanvas.transform.LookAt(Camera.main.transform);
            baseCanvas.transform.Rotate(0, 180, 0);
        }
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateUI();
        
        if (currentHealth <= 0)
        {
            GameOver();
        }
    }
    
    public void Heal(int amount)
    {
        int healingRemaining = maxHealingAllowed - totalHealingReceived;
        int actualHeal = Mathf.Min(amount, healingRemaining);
        actualHeal = Mathf.Min(actualHeal, maxHealth - currentHealth);
        
        if (actualHeal > 0)
        {
            currentHealth += actualHeal;
            totalHealingReceived += actualHeal;
            UpdateUI();
            Debug.Log("Base healed: " + actualHeal);
        }
    }
    
    public bool CanHeal()
    {
        return totalHealingReceived < maxHealingAllowed && currentHealth < maxHealth;
    }
    
    void UpdateUI()
    {
        if (healthBar != null)
            healthBar.value = currentHealth;
        
        if (healthBarFill != null)
        {
            healthBarFill.color = baseHealthColor;
        }
        
        if (healthText != null)
            healthText.text = "HP: " + currentHealth + " / " + maxHealth;
        
        if (healingText != null)
        {
            int remaining = maxHealingAllowed - totalHealingReceived;
            healingText.text = "Heal: " + remaining + " / " + maxHealingAllowed;
            
            if (remaining <= 0)
                healingText.color = Color.red;
        }
    }
    
    void GameOver()
    {
        Debug.Log("Player " + playerNumber + " base destroyed! GAME OVER!");
        
        // NEW: Notify GameManager
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.OnBaseDestroyed(playerNumber);
        }
    }
}