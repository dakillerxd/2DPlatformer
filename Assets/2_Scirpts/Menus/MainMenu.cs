
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
    [Header("Screens")] 
    [SerializeField] private GameObject mainMenuPosition;
    [SerializeField] private GameObject levelSelectMenuPosition;
    [SerializeField] private GameObject collectiblesMenuPosition;
    [SerializeField] private GameObject optionsMenuPosition;
    [SerializeField] private GameObject creditsMenuPosition;

    [Header("Button")]
    [SerializeField] private Button buttonStart;
    [SerializeField] private Button buttonResume;
    [SerializeField] private Button buttonLevelSelect;
    [SerializeField] private Button buttonCollectibles;
    [SerializeField] private Button buttonOptions;
    [SerializeField] private Button buttonCredits;
    [SerializeField] private Button buttonQuit;
    
    
    private void Start()
    {
        SetupButtons();
        CameraController.Instance?.SetTarget(mainMenuPosition.transform);
    }

    private void Update()
    {
        if (InputManager.CancelWasPressed && CameraController.Instance?.target != mainMenuPosition.transform) 
        {
            CameraController.Instance?.SetTarget(mainMenuPosition.transform);
            SoundManager.Instance?.PlaySoundFX("CameraWhoosh");
        }
    }


    private void SetupButtons()
    {
        if (buttonResume != null || buttonStart != null)
        {
            // Check save
            if (SaveManager.Instance.LoadInt("HighestLevel") < 1) { // New game
                
                buttonResume.gameObject.SetActive(false);
                buttonStart.gameObject.SetActive(true);
                
            } else if (SaveManager.Instance.LoadInt("HighestLevel") == 1) { 
            

                if (SaveManager.Instance.LoadInt("SavedCheckpoint") > 0) { // At level one with a checkpoint saved
                
                    buttonResume.gameObject.SetActive(true);
                    buttonStart.gameObject.SetActive(false);

                
                } else { // At level 1 with no checkpoint saved
                
                    buttonResume.gameObject.SetActive(false);
                    buttonStart.gameObject.SetActive(true);
                }
                
            } else if (SaveManager.Instance.LoadInt("HighestLevel") > 1) { // At higher level then 1
            
                buttonResume.gameObject.SetActive(true);
                buttonStart.gameObject.SetActive(false);
            
            }
            
            
            buttonStart.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonStart.onClick.AddListener(() => SaveManager.Instance?.SaveInt("SavedCheckpoint", 0));
            buttonStart.onClick.AddListener(() => SceneManager.LoadScene(1));
            
            
            buttonResume.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonResume.onClick.AddListener(() => SceneManager.LoadScene(SaveManager.Instance.LoadString("SavedLevel")));
        }
        
        
        if (buttonLevelSelect != null)
        {
            buttonLevelSelect.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonLevelSelect.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("CameraWhoosh"));
            buttonLevelSelect.onClick.AddListener(() => CameraController.Instance?.SetTarget(levelSelectMenuPosition.transform));
        }
        
        if (buttonCollectibles != null)
        {
            buttonCollectibles.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonCollectibles.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("CameraWhoosh"));
            buttonCollectibles.onClick.AddListener(() => CameraController.Instance?.SetTarget(collectiblesMenuPosition.transform));
        }

        if (buttonOptions != null)
        {
            buttonOptions.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonOptions.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("CameraWhoosh"));
            buttonOptions.onClick.AddListener(() => CameraController.Instance?.SetTarget(optionsMenuPosition.transform));

        }

        if (buttonCredits != null)
        {
            buttonCredits.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonCredits.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("CameraWhoosh"));
            buttonCredits.onClick.AddListener(() => CameraController.Instance?.SetTarget(creditsMenuPosition.transform));

        }

        if (buttonQuit != null)
        {
            buttonQuit.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonQuit.onClick.AddListener(() => GameManager.Instance?.QuitGame());
        }
    }
}