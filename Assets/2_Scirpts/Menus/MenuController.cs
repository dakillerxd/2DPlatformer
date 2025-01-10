using UnityEngine;
using System.Collections.Generic;

public class MenuController : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private bool selectFirstMenuOnEnable = true;
    [SerializeField] private List<MenuCategory> menuCategories = new List<MenuCategory>();
    

    [Header("References")]
    private MenuCategory _currentCategory;
    private Vector2 _navigate;

    private void Start()
    {
        SelectCategory(menuCategories[0]);
    }
    private void OnEnable()
    {
        if (selectFirstMenuOnEnable)
        {
            SelectFirstCategory();
        }
        

    }

    private void Update()
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

    private void OnNavigate(Vector2 context)
    {

        if (_currentCategory)
        {
            _currentCategory.OnNavigate(context);
        }
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

    public void SelectCategory(MenuCategory category)
    {
        if (!category || !menuCategories.Contains(category)) return;

        if (_currentCategory)
        {
            _currentCategory.gameObject.SetActive(false);
        }

        _currentCategory = category;
        category.gameObject.SetActive(true);
    }

    public void SelectFirstCategory()
    {
        if (menuCategories.Count >= 0)
        {
            SelectCategory(menuCategories[0]);
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