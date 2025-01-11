using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private bool selectFirstCategoryOnEnable = true;
    [SerializeField] private List<MenuCategory> menuCategories = new List<MenuCategory>();
    protected MenuCategory _currentCategory;
    

    private void OnEnable()
    {
        if (selectFirstCategoryOnEnable)
        {
            SelectFirstCategory();
        }
        
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        GameManager.OnOnGameStateChange += OnGameStateChange;
        
    }
    
    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        GameManager.OnOnGameStateChange -= OnGameStateChange;
    }

    protected virtual void Update()
    {
        if (!InputManager.Instance) return;
        
        
        if (InputManager.NavigateUI != Vector2.zero)
        {
            OnNavigate(InputManager.NavigateUI);
        }
        
        if (InputManager.CancelWasPressed)
        {
            OnToggleMenu();
        }
    }

    protected void OnNavigate(Vector2 context)
    {
        if (_currentCategory)
        {
            _currentCategory.OnNavigate(context);
        }
    }
    
    protected virtual void OnActiveSceneChanged(Scene currentScene, Scene nextScene)
    {
        _currentCategory.OnActiveSceneChanged(currentScene, nextScene);
    }

    protected virtual void OnGameStateChange(GameStates state)
    {
        
    }
    
    private void OnToggleMenu()
    {
        if (!_currentCategory) return;
        
        
        if (!_currentCategory.IsAtFirstPage) // if the first page is not the one that is selected pass on the event
        {
            _currentCategory.OnToggleMenu();
                
        } else if (_currentCategory != menuCategories[0]) { // Else go to the first menu
                
            SelectFirstCategory();
        }

    }

    protected void SelectCategory(MenuCategory category)
    {
        if (!category || !menuCategories.Contains(category)) return;

        if (_currentCategory)
        {
            _currentCategory.gameObject.SetActive(false);
        }

        _currentCategory = category;
        category.gameObject.SetActive(true);
    }

    protected void SelectFirstCategory()
    {
        if (menuCategories.Count >= 0)
        {
            SelectCategory(menuCategories[0]);
        }
    }

    protected void DisableAllCategories()
    {
        foreach (MenuCategory category in menuCategories)
        {
            category.gameObject.SetActive(false);
        }
        
    }
    

#if UNITY_EDITOR
    private void OnValidate()
    {
        foreach (Transform child in transform)
        {
            if (!child.TryGetComponent(out MenuCategory category)) continue;
            if (!menuCategories.Contains(category))
            {
                menuCategories.Add(category);
            }
        }
        menuCategories.RemoveAll(menu => menu == null);
    }
#endif
}