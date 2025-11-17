// BananaTree.cs - Updated with better visuals
using UnityEngine;
using UnityEngine.UI;

public class BananaTree : MonoBehaviour
{
    [Header("Tree Settings")]
    public int currentBananas = 50;
    public int maxBananas = 100;
    public float bananaRegenRate = 1f;
    
    [Header("UI")]
    private Canvas treeCanvas;
    private Text bananaText;
    private Slider bananaBar;
    
    void Start()
    {
        SetupUI();
        InvokeRepeating("RegenerateBananas", 1f, 1f);
    }
    
    void SetupUI()
    {
        GameObject canvasObj = new GameObject("TreeCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = Vector3.up * 5f;
        
        treeCanvas = canvasObj.AddComponent<Canvas>();
        treeCanvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(4, 1.5f);
        
        GameObject bgObj = new GameObject("BananaBarBG");
        bgObj.transform.SetParent(canvasObj.transform);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = Color.black;
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.5f);
        bgRect.anchorMax = new Vector2(1, 0.5f);
        bgRect.sizeDelta = new Vector2(0, 0.4f);
        bgRect.anchoredPosition = new Vector2(0, 0.3f);
        
        GameObject sliderObj = new GameObject("BananaSlider");
        sliderObj.transform.SetParent(bgObj.transform);
        bananaBar = sliderObj.AddComponent<Slider>();
        
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;
        
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.yellow;
        
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        
        bananaBar.fillRect = fillRect;
        bananaBar.maxValue = maxBananas;
        bananaBar.value = currentBananas;
        
        GameObject textObj = new GameObject("BananaText");
        textObj.transform.SetParent(canvasObj.transform);
        bananaText = textObj.AddComponent<Text>();
        bananaText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        bananaText.fontSize = 28;
        bananaText.alignment = TextAnchor.MiddleCenter;
        bananaText.color = Color.yellow;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 0);
        textRect.sizeDelta = new Vector2(0, 0.5f);
        textRect.anchoredPosition = new Vector2(0, -0.3f);
        
        UpdateUI();
    }
    
    void Update()
    {
        if (Camera.main != null && treeCanvas != null)
        {
            treeCanvas.transform.LookAt(Camera.main.transform);
            treeCanvas.transform.Rotate(0, 180, 0);
        }
    }
    
    void RegenerateBananas()
    {
        if (currentBananas < maxBananas)
        {
            currentBananas += Mathf.RoundToInt(bananaRegenRate);
            currentBananas = Mathf.Min(currentBananas, maxBananas);
            UpdateUI();
        }
    }
    
    public int HarvestBananas(int amount)
    {
        int harvested = Mathf.Min(amount, currentBananas);
        currentBananas -= harvested;
        UpdateUI();
        return harvested;
    }
    
    void UpdateUI()
    {
        if (bananaBar != null)
            bananaBar.value = currentBananas;
        
        if (bananaText != null)
            bananaText.text = "🍌 " + currentBananas + " / " + maxBananas;
    }
}