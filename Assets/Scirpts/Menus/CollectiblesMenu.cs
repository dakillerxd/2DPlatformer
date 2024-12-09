using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CollectiblesMenu : MonoBehaviour
{
    
    [Header("Collectibles")]
    [SerializeField] private TextMeshProUGUI coinsHeaderText;
    [SerializeField] private TextMeshProUGUI coinsText;
    

    
    [Header("Unlocks")]
    [SerializeField] private TextMeshProUGUI unlocksHeaderText;
    [SerializeField] private TextMeshProUGUI unlocksText;
    [SerializeField] private Toggle googlyEyesToggle;
    [SerializeField] private Toggle propellerHatToggle;
    [SerializeField] private GameObject normalEyes;
    [SerializeField] private GameObject googlyEyes;
    [SerializeField] private GameObject propellerHat;
    private readonly StringBuilder coinStringBuilder = new StringBuilder(256);
    
    
    [Header("Other")]
    [SerializeField] private GameObject mainMenuPosition;
    [SerializeField] private Button buttonLevelSelectBack;
    [SerializeField] private Button buttonResetCollectibles;
    
    

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
            buttonResetCollectibles.onClick.AddListener(UpdateUnlocks);
        }

        UpdateUnlocks();
        UpdateCoinText();
        UpdateUnlocksText();
    }

    private void Update()
    {
        if (normalEyes != null) normalEyes.SetActive(!GameManager.Instance.googlyEyes);
        if (googlyEyes != null) googlyEyes.SetActive(GameManager.Instance.googlyEyes);
        if (propellerHat != null) propellerHat.SetActive(GameManager.Instance.propellerHat);
    }

    private void UpdateUnlocks()
    {
        if (googlyEyesToggle != null) {
            googlyEyesToggle.isOn = GameManager.Instance.googlyEyes;
            googlyEyesToggle.interactable = GameManager.Instance.googlyEyesModeReceived;
            googlyEyesToggle.onValueChanged.AddListener((value) => GameManager.Instance?.ToggleGooglyEyeMode(value));
        }
        
        if (propellerHatToggle != null) {
            propellerHatToggle.isOn = GameManager.Instance.propellerHat;
            propellerHatToggle.interactable = GameManager.Instance.propellerHatReceived;
            propellerHatToggle.onValueChanged.AddListener((value) => GameManager.Instance?.TogglePropellerHat(value));
        }
    }


    private void UpdateUnlocksText()
    {
        
        if (!unlocksText) return;
        if (GameManager.Instance?.collectibles == null) return;
        
        unlocksText.text = 
            $"{GameManager.Instance.collectiblesForUnlock1} Coins: {(GameManager.Instance.googlyEyesModeReceived ? "<color=green>Propeller Hat</color>" : "<color=red>Propeller Hat</color>")}\n \n" +
            $"{GameManager.Instance.collectiblesForUnlock2} Coins: {(GameManager.Instance.propellerHatReceived ? "<color=green>Googly Eyes</color>" : "<color=red>Googly Eyes</color>")}\n \n" +
            $"{GameManager.Instance.collectiblesForUnlock3} Coins: {(GameManager.Instance.bonusLevel1Received ? "<color=green>Bonus level</color>" : "<color=red>Bonus level</color>")}\n \n";
    }
    
    
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
