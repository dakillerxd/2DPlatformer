using System;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PlayerAbilityItem : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private PlayerAbilities ability;
    [SerializeField] private UnityEvent[] eventsAfterTrigger;
    private bool _triggered;
    
    
    [Header("References")]
    [SerializeField] private TextMeshPro titleText;
    

    private void OnValidate()
    {
        SetTitleText();
    }

    private  void OnTriggerEnter2D(Collider2D collision)
    {
        if (_triggered) return;
        if (collision.CompareTag("Player"))
        {
            _triggered = true;
            PlayerController.Instance.ReceiveAbility(ability);
            CameraController.Instance.ShakeCamera(3f, 5f,2,2);
            foreach (var e in eventsAfterTrigger)
            {
                e.Invoke();
            }
            Destroy(gameObject);
        }
    }


    private void SetTitleText() {
        if (!titleText) return;
        
        switch (ability)
        {
            case PlayerAbilities.DoubleJump:
            titleText.text = "Double Jump";
            break;
            
            case PlayerAbilities.WallSlide:
                titleText.text = "Wall Slide";
            break;
            
            case PlayerAbilities.WallJump:
            titleText.text = "Wall Jump";
            break;
            
            
            case PlayerAbilities.Dash:
            titleText.text = "Dash";
            break;
        }
    }

}
