using UnityEngine;

public class MenuCategoryPause : MenuCategory
{
    [Header("Menu Pages")] 
    public MenuPage pauseMenuPage;
    public MenuPage optionsMenuPage;
    
    
    public override void SelectPage(MenuPage page)
    {
        if (page == currentPage) return;
        pauseMenuPage.gameObject.SetActive(false);
        optionsMenuPage.gameObject.SetActive(false);
        
        base.SelectPage(page);
        
    }
    
}
