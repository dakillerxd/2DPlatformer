using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PrimeTween;
using VInspector;

public class MenuPage : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private bool selectFirstSelectableOnEnable = true;
    [SerializeField] private bool rememberLastSelectableOnDisable = false;
    [SerializeField] protected List<Selectable> selectables = new List<Selectable>();
    
    
    [Header("Page Animation Move In")]
    [SerializeField] private bool moveInAnimation = false;
    [ShowIf("moveInAnimation")]
    [SerializeField] private Vector3 moveInFromDirection = new Vector3(0, -700f, 0);
    [SerializeField] private float moveInDuration = 0.3f;
    [SerializeField] private float moveInDelay = 0.2f;
    [SerializeField] private Ease moveInEase = Ease.OutSine;
    [SerializeField] private List<GameObject> moveInObjects= new List<GameObject>();
    [EndIf]
    
    [Header("Selectable Animation Scale")]
    [SerializeField] private bool scaleOnSelect = false;
    [ShowIf("scaleOnSelect")]
    [SerializeField] private float scaleMultiplier = 1.1f;
    [SerializeField] private float scaleDuration = 0.15f;
    [SerializeField] private Ease scaleEase = Ease.InOutBounce;
    [SerializeField] private List<Selectable> scaleExclusions = new List<Selectable>();
    [EndIf]
    
    
    [Header("Selectable Animation Rotation")]
    [SerializeField] private bool rotateOnSelect = false;
    [ShowIf("rotateOnSelect")]
    [SerializeField] private Vector3 rotationStrength = new Vector3(0, 0, 15);
    [SerializeField] private float rotationDuration = 0.15f;
    [SerializeField] private Ease rotationEase = Ease.InOutBounce;
    [SerializeField] private List<Selectable> rotateExclusions = new List<Selectable>();
    [EndIf]
    
    
    [Header("Selectable Animation Shake")]
    [SerializeField] private bool shakeOnSelect = false;
    [ShowIf("shakeOnSelect")]
    [SerializeField] private bool shakeOnDeselect = false;
    [SerializeField] private Vector3 shakeAxis = new Vector3(3, 3, 0);
    [SerializeField] private float shakeFrequency = 10f;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private Ease shakeEase = Ease.Default;
    [SerializeField] private List<Selectable> shakeExclusions = new List<Selectable>();
    [EndIf]
    
    
    private Selectable _currentSelectable;
    private Selectable _previousSelectable;
    protected MenuController _menuController;
    protected MenuCategory _menuCategory;
    private bool _canSelect;
    private readonly Dictionary<GameObject, Vector3> _moveInObjectsOriginalPositions = new Dictionary<GameObject, Vector3>();
    private readonly Dictionary<Selectable, Vector3> _selectableOriginalScales = new Dictionary<Selectable, Vector3>();
    private readonly Dictionary<Selectable, Vector3> _selectableOriginalRotations = new Dictionary<Selectable, Vector3>();
    private readonly Dictionary<Selectable, Vector3> _selectableOriginalPositions = new Dictionary<Selectable, Vector3>();

  
    private void Awake()
    {
        if (selectables.Count == 0) return;
        
        _menuController = GetComponentInParent<MenuController>();
        _menuCategory = GetComponentInParent<MenuCategory>();
        
        foreach (Selectable selectable in selectables)
        {
            SetupSelectable(selectable);
            StoreOriginalTransforms(selectable);
        }
        

        foreach (GameObject obj in moveInObjects)
        {
            _moveInObjectsOriginalPositions[obj] = obj.transform.localPosition;
        }
    }

    private void OnDisable()
    {
        if (rememberLastSelectableOnDisable) return;
        _currentSelectable = null;
        _previousSelectable = null;
        _canSelect = false;
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
    
    
    public virtual void OnPageSelected()
    {
        ResetAllSelectables();
        
        if (moveInAnimation)
        {
            PlayMoveInAnimation();
            
        } else {
            
            StartCoroutine(SetCanSelect(true, 0f));
            
            if (selectFirstSelectableOnEnable) {
                StartCoroutine(SelectFirstAvailableSelectable());
            }
        }
    }
    
    protected void StoreOriginalTransforms(Selectable selectable)
    {
        _selectableOriginalScales[selectable] = selectable.transform.localScale;
        _selectableOriginalRotations[selectable] = selectable.transform.localRotation.eulerAngles;
        _selectableOriginalPositions[selectable] = selectable.transform.localPosition;
    }
    
    protected void SetupSelectable(Selectable selectable)
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

    private void ResetAllSelectables()
    {
        if (selectables.Count == 0) return;
        
        foreach (Selectable selectable in selectables)
        {
            if (!selectable.gameObject.activeSelf) continue;
            selectable.transform.localScale = _selectableOriginalScales[selectable];
            selectable.transform.localRotation = Quaternion.Euler(_selectableOriginalRotations[selectable]);
            selectable.transform.localPosition = _selectableOriginalPositions[selectable];
        }
    }

    private IEnumerator SelectFirstAvailableSelectable(float delay = 0.03f)
    {
        if (selectables.Count == 0) yield return null;
        yield return new WaitForSeconds(delay); // small delay so the selectables will have time to initialize
        
        if (_previousSelectable && _previousSelectable.isActiveAndEnabled)
        {
            EventSystem.current.SetSelectedGameObject(_previousSelectable.gameObject);
        }
        else
        {
            for (int i = 1; i < selectables.Count; i++)
            {
                if (selectables[i].gameObject.activeSelf)
                {
                    EventSystem.current.SetSelectedGameObject(selectables[i].gameObject);
                    break;
                }
            }
        }
        
    }
    
    
    private IEnumerator SetCanSelect(bool state, float delay)
    {
        yield return new WaitForSeconds(delay);
        _canSelect = state;

        foreach (Selectable selectable in selectables)
        {
            
            selectable.interactable = state;
            
        }
    }
    
    
    #endregion Page Management // ---------------------------------------------------------------------
    
    
    #region Events // ---------------------------------------------------------------------
    
    private void OnSelect(BaseEventData eventData)
    {
        if (!_canSelect) return;
        
        if (!eventData.selectedObject.activeSelf) return;
        
        _currentSelectable = eventData.selectedObject.GetComponent<Selectable>();
        if (_currentSelectable == null) return;

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

    private void OnDeselect(BaseEventData eventData)
    {
        if (!_canSelect) return;
        
        if (!eventData.selectedObject.activeSelf || _currentSelectable == null) return;
        
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
        if (!_canSelect) return;
        
        if (eventData is PointerEventData pointerEventData)
        {
            pointerEventData.selectedObject = pointerEventData.pointerEnter;
        }
    }
    
    private void OnPointerExit(BaseEventData eventData)
    {
        if (!_canSelect) return;
        
        if (eventData is PointerEventData pointerEventData)
        {
            pointerEventData.selectedObject = null;
        }
    }
    
    public void OnNavigate(Vector2 input)
    {
        if (!_canSelect) return;
        
        if (EventSystem.current.currentSelectedGameObject != null) return;
        StartCoroutine(SelectFirstAvailableSelectable());
    }
    
    #endregion Events // ---------------------------------------------------------------------
    
    
    #region Animations // ---------------------------------------------------------------------
    
    private void PlayScaleAnimation(Transform target, bool scaleUp)
    {
        Vector3 endScale = scaleUp ? _selectableOriginalScales[_currentSelectable] * scaleMultiplier : _selectableOriginalScales[_currentSelectable];
        Tween.Scale(target, endScale, scaleDuration, scaleEase);
    }

    private void PlayRotateAnimation(Transform target, bool rotateToTarget)
    {
        Vector3 endRotation = rotateToTarget ? rotationStrength : _selectableOriginalRotations[_currentSelectable];
        Tween.LocalRotation(target, endRotation, rotationDuration, rotationEase);
    }

    private void PlayShakeAnimation(Transform target)
    {
        Tween.ShakeLocalPosition(target, shakeAxis, shakeDuration, shakeFrequency, easeBetweenShakes: shakeEase);
    }
    
    private void PlayMoveInAnimation()
    {
        if (moveInObjects.Count == 0) return;
        
        
        StartCoroutine(SetCanSelect(false, 0f));
        _currentSelectable = null;
        _previousSelectable = null;
        EventSystem.current.SetSelectedGameObject(null);
        ResetAllSelectables();
        
        
        for (int i = 0; i < moveInObjects.Count; i++)
        {
            GameObject currentObject = moveInObjects[i];
            currentObject.transform.localPosition = moveInFromDirection;
            Tween.LocalPosition(currentObject.transform, startValue: moveInFromDirection, endValue: _moveInObjectsOriginalPositions[currentObject], moveInDuration, ease: moveInEase, startDelay: i * moveInDelay);
        }
        
        float totalAnimationTime = moveInDuration + (moveInDelay * (moveInObjects.Count - 1));
        StartCoroutine(SetCanSelect(true, totalAnimationTime));
        if (selectFirstSelectableOnEnable) { 
            StartCoroutine(SelectFirstAvailableSelectable(totalAnimationTime + 0.03f)); 
        }
    }
    
    #endregion Animations // ---------------------------------------------------------------------

    
}