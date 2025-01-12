using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MenuCategory : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool selectFirstPageOnEnable = true;
    [SerializeField] protected List<MenuPage> pages = new List<MenuPage>();
    public bool IsAtFirstPage => pages[0].gameObject.activeSelf;
    protected MenuPage _currentPage;

    
    
    private void OnEnable()
    {
        if (selectFirstPageOnEnable)
        {
            SelectFirstPage();
        }
    }
    
    public void OnNavigate(Vector2 input)
    {
        if (_currentPage)
        {
            _currentPage.OnNavigate(input);
        }
    }
    

    public void OnToggleMenu()
    {
        if (!_currentPage) return;
        
        if (_currentPage != pages[0])
        {
            SelectFirstPage();
        }

    }

    public virtual void SelectPage(MenuPage page)
    {
        if (!page || !pages.Contains(page)) return;
        
        _currentPage = page;
        page.gameObject.SetActive(true);
        
    }

    private void SelectFirstPage()
    {
        if (pages.Count >= 0)
        {
            SelectPage(pages[0]);
        }
    }
    
    public void OnActiveSceneChanged(Scene currentScene, Scene nextScene)
    {

        _currentPage.OnActiveSceneChanged(currentScene, nextScene);
    }
    
    public virtual void OnGameStateChange(GameStates state)
    {
        
    }

    protected void DisableNonActivePagesSelectables()
    {
        foreach (MenuPage page in pages)
        {
            page.DisableSelectables();
        }
        
    }
    

    

#if UNITY_EDITOR
    private void OnValidate()
    {
        foreach (Transform child in transform)
        {
            if (!child.TryGetComponent(out MenuPage category)) continue;
            if (!pages.Contains(category))
            {
                pages.Add(category);
            }
        }
        pages.RemoveAll(page => page == null);
    }
#endif
}