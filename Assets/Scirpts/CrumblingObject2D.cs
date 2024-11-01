using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// This script is used to handle the crumbling behavior of an object after collision.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CrumblingObject2D : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float timeUntilBroken = 3f;
    [SerializeField] private float breakDuration = 3f;
    [SerializeField] [Range(0f, 1f)] private float brokenAlpha;

    [Header("State")]
    private bool isBroken = false;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D coll;

    private void Start()
    {
        if (!coll) { coll = GetComponent<Collider2D>(); }
        if (!spriteRenderer) { spriteRenderer = GetComponent<SpriteRenderer>(); }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (isBroken) return; 
        StartCoroutine(CountDownToBreak());
    }

    private IEnumerator CountDownToBreak() {
        
        float time = timeUntilBroken;
        
        while (time > 0) {
            time -= Time.deltaTime;
            yield return null;
        }
        Break();
    }
    
    private IEnumerator CountDownToUnBreak() {
        
        float time = breakDuration;
        
        while (time > 0) {
            time -= Time.deltaTime;
            yield return null;
        }
        UnBreak();
    }
    private void Break() {
        isBroken = true;
        coll.enabled = false;
        spriteRenderer.color = new Color(1f, 1f, 1f, brokenAlpha);;
        StartCoroutine(CountDownToUnBreak());
    }

    private void UnBreak() {
        isBroken = false;
        coll.enabled = true;
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);;
    }

}
