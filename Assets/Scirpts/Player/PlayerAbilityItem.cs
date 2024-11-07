using UnityEngine;

public class PlayerAbilityItem : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private PlayerAbilities ability;
    private bool _triggered;
    
    
    private  void OnTriggerEnter2D(Collider2D collision)
    {
        if (_triggered) return;
        if (collision.CompareTag("Player"))
        {
            _triggered = true;
            PlayerController2D.Instance.ReceiveAbility(ability);
            Destroy(gameObject);
        }
    }

}
