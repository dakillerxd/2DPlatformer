using UnityEngine;
using UnityEngine.UI;


public class MainMenuManager : MonoBehaviour
{

    [Header("Buttons")]
    [SerializeField] private Button buttonStart;
    [SerializeField] private Button buttonOptions;
    [SerializeField] private Button buttonCredits;
    [SerializeField] private Button buttonQuit;

    private void Start()
    {
        SetupButtons();
    }

    private void SetupButtons()
    {
        if (buttonStart != null)
        {
            buttonStart.onClick.RemoveAllListeners();
            buttonStart.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonStart.onClick.AddListener(() => CustomSceneManager.Instance?.LoadScene(2, false));
        }

        if (buttonOptions != null)
        {
            buttonOptions.onClick.RemoveAllListeners();
            buttonOptions.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));

        }

        if (buttonCredits != null)
        {
            buttonCredits.onClick.RemoveAllListeners();
            buttonCredits.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));

        }

        if (buttonQuit != null)
        {
            buttonQuit.onClick.RemoveAllListeners();
            buttonQuit.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonQuit.onClick.AddListener(() => QuitGame());
        }
        
    }


    

    private void QuitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
        #endif
    }
    
}