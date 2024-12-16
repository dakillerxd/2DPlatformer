using System;
using System.Collections;
using UnityEngine;

public class Spike : MonoBehaviour
{
    public Vector2 pushForce = new Vector2(5, 5);
    public int damage = 1;
    private bool _triggered;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || _triggered) return;
        _triggered = true;
        Vector2 spikeNormal = collision.GetContact(0).normal;
        var player = collision.gameObject.GetComponent<PlayerController>();
        player.Push(-spikeNormal * pushForce);
        player.DamageHealth(damage, true, gameObject.name);
        StartCoroutine(ResetTrigger());
    }
    

    private IEnumerator ResetTrigger()
    {
        yield return new WaitForSeconds(0.2f);
        _triggered = false;
    }
}
