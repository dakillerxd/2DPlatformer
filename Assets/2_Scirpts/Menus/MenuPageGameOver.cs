using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuPageGameOver : MenuPage
{

    [Header("UI Elements")]
    [SerializeField] private  TMP_Text gameOverCollectiblesText;
    [SerializeField] private  TMP_Text gameOverTitleText;
    [SerializeField] private  TMP_Text gameOverMessageText;
    
    [Header("Game Over Messages")]
    [SerializeField] private List<string> loseMessages = new List<string> { "1!", "2", };
    [SerializeField] private List<string> winMessages;
    [SerializeField] private List<string> perfectWinMessages;
    
    
    
    
    private void UpdateGameOverInfo()
    {

        if (gameOverCollectiblesText != null)
        {
            
        }
        
    }


    private string GetRandomMessage(List<string> messages)
    {
        if (messages == null || messages.Count == 0)
        {
            return "No message available.";
        }
        return messages[Random.Range(0, messages.Count)];
    }
}
