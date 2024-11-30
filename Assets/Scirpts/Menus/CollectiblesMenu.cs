using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectiblesMenu : MonoBehaviour
{
    
    [Header("References")]
    [SerializeField] private Button buttonLevelSelectBack;
    [SerializeField] private Button buttonResetCollectibles;
    [SerializeField] private GameObject mainMenuPosition;
    [SerializeField] private TextMeshProUGUI coinsHeaderText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI unlocksHeaderText;
    [SerializeField] private TextMeshProUGUI unlocksText;
    

    private void Start()
    {
        if (buttonLevelSelectBack != null) {
            
            buttonLevelSelectBack.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonLevelSelectBack.onClick.AddListener(() => CameraController.Instance?.SetTarget(mainMenuPosition.transform));
        }
        
        if (buttonResetCollectibles != null) {
            
            buttonResetCollectibles.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonResetCollectibles.onClick.AddListener(() => GameManager.Instance?.ResetCollectibles());
            buttonResetCollectibles.onClick.AddListener(UpdateCoinText);
            buttonResetCollectibles.onClick.AddListener(UpdateUnlocksText);
        }

        UpdateCoinText();
        UpdateUnlocksText();
    }


    private void UpdateUnlocksText()
    {
        
        if (!unlocksText) return;
        if (GameManager.Instance.collectibles == null) return;
        
        int totalCollected = GameManager.Instance.TotalCollectiblesCollected();
        unlocksText.text = 
            $"2 Coins: {(totalCollected >= 2 ? "<color=green>Googly Eyes</color>" : "<color=red>Googly Eyes</color>")}\n \n" +
            $"4 Coins: {(totalCollected >= 4 ? "<color=green>Bonus level</color>" : "<color=red>Bonus level</color>")}\n \n" +
            $"6 Coins: {(totalCollected >= 6 ? "<color=green>Bonus level</color>" : "<color=red>Bonus level</color>")}\n \n";
    }
    
    private readonly StringBuilder coinStringBuilder = new StringBuilder(256);
    private void UpdateCoinText() 
    {

        if (coinsHeaderText)
        {
            coinsHeaderText.text = $"Coins: {GameManager.Instance.TotalCollectiblesCollected()} / {GameManager.Instance.TotalCollectiblesAmount()}";
        }
        
        
        if (coinsText == null) return;
        
        coinStringBuilder.Clear();
        
        if (GameManager.Instance.collectibles != null)
        {
            foreach (Collectible collectible in GameManager.Instance.collectibles)
            {
                if (collectible != null && collectible.counts)
                {
                    coinStringBuilder.AppendFormat("{0}: {1}\n \n", collectible.connectedLevel?.SceneName ?? "Unknown", collectible.collected ? "<color=green>Collected</color>" : "<color=red>Not Collected</color>");
                }
            }
            
            coinsText.text = coinStringBuilder.ToString();
        }
    }
    
}
