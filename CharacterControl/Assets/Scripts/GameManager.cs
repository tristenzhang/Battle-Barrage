// GameManager.cs - Updated with per-player currency system
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int startingBananas = 50;
    public float gameTimer = 300f;

    [Header("UI Elements")]
    public Text player1ScoreText;
    public Text player2ScoreText;
    public Text timerText;
    public Text gameOverText;

    private int player1Currency = 0;
    private int player2Currency = 0;
    private float currentTime;
    private bool gameEnded = false;

    void Start()
    {
        currentTime = gameTimer;
        
        // Give starting currency to both players
        player1Currency = startingBananas;
        player2Currency = startingBananas;
        
        UpdateUI();
        
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (gameEnded) return;
        
        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            EndGame();
        }
        UpdateUI();
    }
    
    // Add currency to specific player
    public void AddCurrency(int playerNumber, int amount)
    {
        if (playerNumber == 1)
            player1Currency += amount;
        else if (playerNumber == 2)
            player2Currency += amount;
        
        UpdateUI();
    }
    
    // Spend currency for specific player - returns true if successful
    public bool SpendCurrency(int playerNumber, int amount)
    {
        if (playerNumber == 1)
        {
            if (player1Currency >= amount)
            {
                player1Currency -= amount;
                UpdateUI();
                return true;
            }
        }
        else if (playerNumber == 2)
        {
            if (player2Currency >= amount)
            {
                player2Currency -= amount;
                UpdateUI();
                return true;
            }
        }
        
        return false;
    }
    
    // Get currency for specific player
    public int GetCurrency(int playerNumber)
    {
        return playerNumber == 1 ? player1Currency : player2Currency;
    }
    
    void UpdateUI()
    {
        if (player1ScoreText != null)
        {
            player1ScoreText.text = "P1 Currency: " + player1Currency;
        }
        if (player2ScoreText != null)
        {
            player2ScoreText.text = "P2 Currency: " + player2Currency;
        }
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = "Time: " + minutes.ToString("00") + ":" + seconds.ToString("00");
        }
    }
    
    void EndGame()
    {
        if (gameEnded) return;
        
        gameEnded = true;
        Debug.Log("Game Over!");
        
        // Determine winner
        string winner = "Tie!";
        if (player1Currency > player2Currency)
            winner = "Player 1 Wins!";
        else if (player2Currency > player1Currency)
            winner = "Player 2 Wins!";
        
        Debug.Log(winner + " - P1: " + player1Currency + " P2: " + player2Currency);
        
        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER\n" + winner + "\nP1: " + player1Currency + " | P2: " + player2Currency;
            gameOverText.gameObject.SetActive(true);
        }
        
        // Optionally pause the game
        // Time.timeScale = 0;
    }
    
    // Called when a base is destroyed
    public void OnBaseDestroyed(int playerNumber)
    {
        if (gameEnded) return;
        
        gameEnded = true;
        string winner = playerNumber == 1 ? "Player 2 Wins!" : "Player 1 Wins!";
        Debug.Log(winner + " - Base Destroyed!");
        
        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER\n" + winner + "\nBase Destroyed!";
            gameOverText.gameObject.SetActive(true);
        }
    }
}