using UnityEngine;
using UnityEngine.UI;

public class MenuPageCredits : MenuPage
{
    
    
    [Header("Buttons")]
    [SerializeField] private Button buttonBack;
    
    
    private void Start()
    {
        SetupButtons();
    }

    private void SetupButtons()
    {
        
        if (buttonBack != null)
        {
            buttonBack.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonBack.onClick.AddListener(() => _menuCategoryMain.SelectPage(_menuCategoryMain.mainMenuPage));
        }

    }

}
