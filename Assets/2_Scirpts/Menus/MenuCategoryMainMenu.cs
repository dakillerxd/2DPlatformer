
using UnityEngine;



public class MenuCategoryMainMenu : MenuCategory
{

    [Header("Menu Pages")] 
    public MenuPage mainMenuPage;
    public MenuPage levelSelectMenuPage;
    public MenuPage collectiblesMenuPage;
    public MenuPage optionsMenuPage;
    public MenuPage creditsMenuPage;
    
    
    [Header("Camera Positions")] 
    [SerializeField] private GameObject mainMenuPosition;
    [SerializeField] private GameObject levelSelectMenuPosition;
    [SerializeField] private GameObject collectiblesMenuPosition;
    [SerializeField] private GameObject optionsMenuPosition;
    [SerializeField] private GameObject creditsMenuPosition;
    
    
    
    public override void SelectPage(MenuPage page)
    {
        base.SelectPage(page);

        if (page == mainMenuPage)
        {
            CameraController.Instance?.SetTarget(mainMenuPosition.transform);
        }
        else if (page == levelSelectMenuPage)
        {
            CameraController.Instance?.SetTarget(levelSelectMenuPosition.transform);

        }
        else if (page == collectiblesMenuPage)
        {
            CameraController.Instance?.SetTarget(collectiblesMenuPosition.transform);

        }
        else if (page == optionsMenuPage)
        {
            CameraController.Instance?.SetTarget(optionsMenuPosition.transform);
        }
        else if (page == creditsMenuPage)
        {
            CameraController.Instance?.SetTarget(creditsMenuPosition.transform);
        }
        
        SoundManager.Instance?.PlaySoundFX("CameraWhoosh");
    }
    
    
}
