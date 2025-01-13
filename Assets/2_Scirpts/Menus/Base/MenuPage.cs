using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PrimeTween;
using VInspector;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MenuPage : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private bool selectFirstSelectableOnEnable = true;
    [SerializeField] private bool forgetLastSelectableOnDisable = true;
    [SerializeField] protected List<Selectable> selectables = new List<Selectable>();
    
    [Foldout("Selectable Animation Scale")]
    [SerializeField] private bool scaleOnSelect = false;
    [ShowIf("scaleOnSelect")]
    [SerializeField] private float scaleMultiplier = 1.1f;
    [SerializeField] private float scaleDuration = 0.15f;
    [SerializeField] private Ease scaleEase = Ease.InOutBounce;
    [SerializeField] private List<Selectable> scaleExclusions = new List<Selectable>();
    [EndIf]
    [EndFoldout]
    
    
    [Foldout("Selectable Animation Rotation")]
    [SerializeField] private bool rotateOnSelect = false;
    [ShowIf("rotateOnSelect")]
    [SerializeField] private Vector3 rotationStrength = new Vector3(0, 0, 15);
    [SerializeField] private float rotationDuration = 0.15f;
    [SerializeField] private Ease rotationEase = Ease.InOutBounce;
    [SerializeField] private List<Selectable> rotateExclusions = new List<Selectable>();
    [EndIf]
    [EndFoldout]
    
    
    [Foldout("Selectable Animation Shake")]
    [SerializeField] private bool shakeOnSelect = false;
    [ShowIf("shakeOnSelect")]
    [SerializeField] private bool shakeOnDeselect = false;
    [SerializeField] private Vector3 shakeAxis = new Vector3(3, 3, 0);
    [SerializeField] private float shakeFrequency = 10f;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private Ease shakeEase = Ease.Default;
    [SerializeField] private List<Selectable> shakeExclusions = new List<Selectable>();
    [EndIf]
    [EndFoldout]
    
    
    private Selectable _currentSelectable;
    private Selectable _previousSelectable;
    protected MenuController _menuController;
    protected MenuCategory _menuCategory;
    protected  MenuCategoryMainMenu _menuCategoryMain;
    protected MenuCategoryPause _menuCategoryPause;
    private readonly Dictionary<Selectable, Vector3> _selectableOriginalScales = new Dictionary<Selectable, Vector3>();
    private readonly Dictionary<Selectable, Vector3> _selectableOriginalRotations = new Dictionary<Selectable, Vector3>();
    private readonly Dictionary<Selectable, Vector3> _selectableOriginalPositions = new Dictionary<Selectable, Vector3>();
    private readonly Dictionary<Selectable, bool> _selectableOriginalState = new Dictionary<Selectable, bool>();

  
    private void Awake()
    {
        _menuController = GetComponentInParent<MenuController>();
        _menuCategory = GetComponentInParent<MenuCategory>();
        _menuCategoryMain = GetComponentInParent<MenuCategoryMainMenu>();
        _menuCategoryPause = GetComponentInParent<MenuCategoryPause>();
        
        if (selectables.Count == 0) return;
        foreach (Selectable selectable in selectables)
        {
            SetupSelectable(selectable);
            StoreOriginalTransforms(selectable);
            StoreOriginalState(selectable);
        }
        
    }

    protected virtual void Start()
    {
        if (_menuCategory.currentPage != this || _menuController.currentCategory != _menuCategory)
        {
            DisableAllSelectables();
        }
        else
        {
            ResetAllSelectables();
        }
        
        
    }

    protected virtual void OnDisable()
    {
        DisableAllSelectables();

        if (forgetLastSelectableOnDisable)
        {
            _currentSelectable = null;
            _previousSelectable = null;
        }
    }
    
    protected virtual void OnEnable()
    {
        
        ResetAllSelectables();
        StartCoroutine(SelectFirstAvailableSelectableCar());
    }
    
    
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        foreach (Transform child in transform)
        {
            if (!child.TryGetComponent(out Selectable selectable)) continue;
            if (!selectables.Contains(selectable))
            {
                selectables.Add(selectable);
            }
        }
        selectables.RemoveAll(menu => menu == null);
    }
#endif
    
    
    #region Page Management // ---------------------------------------------------------------------
    
    
    
    protected void StoreOriginalTransforms(Selectable selectable)
    {
        _selectableOriginalScales[selectable] = selectable.transform.localScale;
        _selectableOriginalRotations[selectable] = selectable.transform.localRotation.eulerAngles;
        _selectableOriginalPositions[selectable] = selectable.transform.localPosition;
    }
    
    protected void StoreOriginalState(Selectable selectable)
    {
        _selectableOriginalState[selectable] = selectable.interactable;
    }
    
    protected virtual void SetupSelectable(Selectable selectable)
    {
        var eventTrigger = selectable.GetComponent<EventTrigger>() ?? selectable.gameObject.AddComponent<EventTrigger>();
        
        AddEventTriggerEntry(eventTrigger, EventTriggerType.Select, OnSelect);
        AddEventTriggerEntry(eventTrigger, EventTriggerType.Deselect, OnDeselect);
        AddEventTriggerEntry(eventTrigger, EventTriggerType.PointerEnter, OnPointerEnter);
        AddEventTriggerEntry(eventTrigger, EventTriggerType.PointerExit, OnPointerExit);
    }

    private void AddEventTriggerEntry(EventTrigger eventTrigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback = new EventTrigger.TriggerEvent();
        entry.callback.AddListener(callback);
        eventTrigger.triggers.Add(entry);
    }

    public void ResetAllSelectables() 
    {
        if (selectables.Count == 0) return;
        
        foreach (Selectable selectable in selectables)
        {
            selectable.transform.localScale = _selectableOriginalScales[selectable];
            selectable.transform.localRotation = Quaternion.Euler(_selectableOriginalRotations[selectable]);
            selectable.transform.localPosition = _selectableOriginalPositions[selectable];
            selectable.interactable = _selectableOriginalState[selectable];
        }
    }

    public IEnumerator SelectFirstAvailableSelectableCar()
    {
        yield return new WaitForSeconds(0.3f); // small delay so the selectables will have time to initialize
        SelectFirstAvailableSelectable();
    }
    
    public void SelectFirstAvailableSelectable()
    {
        if (selectables.Count == 0 || !selectFirstSelectableOnEnable) return;
        
        // Try to select previous selectable if it's valid
        if (_previousSelectable && _previousSelectable.isActiveAndEnabled)
        {
            EventSystem.current.SetSelectedGameObject(_previousSelectable.gameObject);
            return;
        }
    
        // Look for first active selectable starting from index 0
        for (var i = 0; i < selectables.Count; i++)
        {
            if (!selectables[i].gameObject.activeSelf) continue;
            EventSystem.current.SetSelectedGameObject(selectables[i].gameObject);
            return;
        }
    }


    public void DisableAllSelectables()
    {
        foreach (Selectable selectable in selectables)
        {
            selectable.interactable = false;
        }
    }
    
    public virtual void ResetSelectablesState()
    {
        foreach (Selectable selectable in selectables)
        {
            selectable.interactable = _selectableOriginalState[selectable];
        }
    }

    public virtual void ResetAllSelectablesSize()
    {
            foreach (Selectable selectable in selectables)
            {
                selectable.transform.localScale = _selectableOriginalScales[selectable];
            }
    }
    

    
    
    
    #endregion Page Management // ---------------------------------------------------------------------
    
    
    #region Events // ---------------------------------------------------------------------
    
    protected virtual void OnSelect(BaseEventData eventData)
    {
        
        if (!eventData.selectedObject.activeSelf) return;
        
        _currentSelectable = eventData.selectedObject.GetComponent<Selectable>();
        if (!_currentSelectable || !_currentSelectable.interactable) return;

        if (scaleOnSelect && !scaleExclusions.Contains(_currentSelectable))
        {
            PlayScaleAnimation(eventData.selectedObject.transform, true);
        }
        
        if (rotateOnSelect && !rotateExclusions.Contains(_currentSelectable))
        {
            PlayRotateAnimation(eventData.selectedObject.transform, true);
        }
        
        if (shakeOnSelect && !shakeExclusions.Contains(_currentSelectable))
        {
            PlayShakeAnimation(eventData.selectedObject.transform);
        }
        

        SoundManager.Instance?.PlaySoundFX("ButtonSelect");
    }

    protected virtual void OnDeselect(BaseEventData eventData)
    {
        
        if (!eventData.selectedObject.activeSelf || _currentSelectable == null || !_currentSelectable.interactable) return;
        
        if (scaleOnSelect && !scaleExclusions.Contains(_currentSelectable))
        {
            PlayScaleAnimation(eventData.selectedObject.transform, false);
        }

        if (rotateOnSelect && !rotateExclusions.Contains(_currentSelectable))
        {
            PlayRotateAnimation(eventData.selectedObject.transform, false);
        }
        
        if (shakeOnSelect && shakeOnDeselect && !shakeExclusions.Contains(_currentSelectable))
        {
            PlayShakeAnimation(eventData.selectedObject.transform);
        }
        
        _previousSelectable = _currentSelectable;
        _currentSelectable = null;
    }

    private void OnPointerEnter(BaseEventData eventData)
    {
        
        if (eventData is PointerEventData pointerEventData)
        {
            pointerEventData.selectedObject = pointerEventData.pointerEnter;
        }
    }
    
    private void OnPointerExit(BaseEventData eventData)
    {
        
        if (eventData is PointerEventData pointerEventData)
        {
            pointerEventData.selectedObject = null;
        }
    }
    
    public void OnNavigate(Vector2 input)
    {

        if (EventSystem.current.currentSelectedGameObject) return;
        SelectFirstAvailableSelectable();
    }
    
    public virtual void OnActiveSceneChanged(Scene currentScene, Scene nextScene)
    {


    }
    
    #endregion Events // ---------------------------------------------------------------------
    
    
    #region Animations // ---------------------------------------------------------------------
    
    private void PlayScaleAnimation(Transform target, bool scaleUp)
    {
        Vector3 endScale = scaleUp ? _selectableOriginalScales[_currentSelectable] * scaleMultiplier : _selectableOriginalScales[_currentSelectable];
        Tween.Scale(target, endScale, scaleDuration, scaleEase, useUnscaledTime: true);
    }

    private void PlayRotateAnimation(Transform target, bool rotateToTarget)
    {
        Vector3 endRotation = rotateToTarget ? rotationStrength : _selectableOriginalRotations[_currentSelectable];
        Tween.LocalRotation(target, endRotation, rotationDuration, rotationEase, useUnscaledTime: true);
    }

    private void PlayShakeAnimation(Transform target)
    {
        Tween.ShakeLocalPosition(target, shakeAxis, shakeDuration, shakeFrequency, easeBetweenShakes: shakeEase, useUnscaledTime: true);
    }
    
    #endregion Animations // ---------------------------------------------------------------------

    
}