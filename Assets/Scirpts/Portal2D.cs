using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using CustomAttribute;
using System.Collections;
public class Portal2D : MonoBehaviour
{

    [SerializeField] private Transform connectedPortal;
    private float cooldownTime;
    private bool canTeleport;

    private void Start()
    {
        canTeleport  = true;
    }

    public void StartCooldown(float cooldownDuration = 1f) {

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

    public void StartCooldownForConnectedPortal() {
        
        if (!connectedPortal) return;
        connectedPortal.gameObject.GetComponent<Portal2D>().StartCooldown();
    }
    public bool CanTeleport() {
        return  canTeleport && connectedPortal;
    }
    
    public  Vector2 GetConnectedPortalLocation() {
        return connectedPortal.transform.position;
    }
}
