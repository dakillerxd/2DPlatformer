using UnityEngine;

public class MenuCategoryPause : MenuCategory
{
    [Header("Menu Pages")] 
    public MenuPage pauseMenuPage;
    public MenuPage optionsMenuPage;
    
    
    public override void SelectPage(MenuPage page)
    {
        base.SelectPage(page);
        DisableNonActivePages();
    }
    
}
