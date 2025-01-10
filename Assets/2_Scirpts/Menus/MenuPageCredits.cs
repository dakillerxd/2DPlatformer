using UnityEngine;
using UnityEngine.UI;

public class MenuPageCredits : MenuPage
{

    private MenuCategoryMainMenu _menuCategoryMain;
    
    [Header("Buttons")]
    [SerializeField] private Button buttonBack;
    
    
    private void Start()
    {

        SetupButtons();

    }

    private void SetupButtons()
    {
        _menuCategoryMain = GetComponentInParent<MenuCategoryMainMenu>();
        
        if (buttonBack != null)
        {
            buttonBack.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonBack.onClick.AddListener(() => _menuCategory.SelectPage(_menuCategoryMain.mainMenuPage));
        }

    }

}
