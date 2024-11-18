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
            PlayerController.Instance.ReceiveAbility(ability);
            CameraController.Instance.ShakeCamera(3f, 5f,2,2);
            Destroy(gameObject);
        }
    }

}
