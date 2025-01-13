using UnityEngine;
using UnityEngine.UI;

public class MenuPageCredits : MenuPage
{
    [Header("UI Elements")]
    [SerializeField] private Button buttonBack;
    
    
    protected override void Start()
    {
        base.Start();
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
