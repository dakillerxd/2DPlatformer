using UnityEngine;
using System.Collections;
using VInspector;

public class Portal : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform connectedPortal;
    [SerializeField] private bool keepMomentum = true;
    [SerializeField] GameObject portalPrefab;
    private float cooldownTime;
    private bool canTeleport;

    private void Start() {
        
        canTeleport  = true;
    }
    
    
    private void OnTriggerEnter2D(Collider2D collision) {
        
        if (!CanTeleport()) return;

        switch (collision.gameObject.tag) {
            case "Player":
                
                PlayerController player = collision.GetComponentInParent<PlayerController>();
                StartCooldown();
                StartCooldownForConnectedPortal();
                player.Teleport(connectedPortal.transform.position, keepMomentum);
                SoundManager.Instance?.PlaySoundFX("Teleport", 0.1f);
                break;
        }
    }

    private void StartCooldown(float cooldownDuration = 1f) {

        StartCoroutine(Cooldown(cooldownDuration));
        
    }

    private IEnumerator Cooldown(float cooldownDuration) {
        
        canTeleport = false;
        cooldownTime = cooldownDuration;

        while (cooldownTime > 0) {
            cooldownTime -= Time.deltaTime;
            yield return null;
        }
        
        canTeleport = true;
    }

    private void StartCooldownForConnectedPortal() {
        
        if (!connectedPortal) return;
        connectedPortal.gameObject.GetComponent<Portal>().StartCooldown();
    }
    private bool CanTeleport() {
        return  canTeleport && connectedPortal;
    }


    [Button] private void CreateConnectedPortal() {
        if (!connectedPortal) {
            GameObject newPortal = Instantiate(portalPrefab, transform.position, Quaternion.identity);
            connectedPortal = newPortal.transform;
            newPortal.GetComponent<Portal>().connectedPortal = transform;
            return;
        }
        Debug.Log("Connected portal already exists");
    }
    
}
