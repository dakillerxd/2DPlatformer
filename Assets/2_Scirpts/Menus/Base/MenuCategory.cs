using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MenuCategory : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool selectFirstPageOnEnable = true;
    [SerializeField] protected List<MenuPage> pages = new List<MenuPage>();
    public bool IsAtFirstPage => pages[0].gameObject.activeSelf;
    public MenuPage currentPage;

    private void Awake()
    {
        if (selectFirstPageOnEnable)
        {
            SelectFirstPage();
        }
    }

    private void OnEnable()
    {
        if (selectFirstPageOnEnable)
        {
            SelectFirstPage();
        }
    }
    
    public void OnNavigate(Vector2 input)
    {
        if (currentPage)
        {
            currentPage.OnNavigate(input);
        }
    }
    

    public void OnToggleMenu()
    {
        if (!currentPage) return;
        
        if (currentPage != pages[0])
        {
            SelectFirstPage();
        }

    }

    public virtual void SelectPage(MenuPage page)
    {
        if (!page || !pages.Contains(page)) return;
        
        currentPage = page;
        page.gameObject.SetActive(true);
        // page.ResetAllSelectables(); // Resetting the position bugs the selectables that are in a layout group
        page.ResetSelectablesState();
        StartCoroutine(page.SelectFirstAvailableSelectableCar());
        DisableNonActivePagesSelectables();
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

        currentPage.OnActiveSceneChanged(currentScene, nextScene);
    }
    
    public virtual void OnGameStateChange(GameStates state)
    {
        
    }

    protected void DisableNonActivePagesSelectables()
    {
        foreach (MenuPage page in pages)
        {
            if (currentPage == page) continue; // skip the current page
            page.DisableAllSelectables();
            page.ResetAllSelectablesSize();
        }
        
    }

    public void DisableAllPage()
    {
        foreach (MenuPage page in pages)
        {
            page.DisableAllSelectables();
            page.gameObject.SetActive(false);
        }
        currentPage = null;
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