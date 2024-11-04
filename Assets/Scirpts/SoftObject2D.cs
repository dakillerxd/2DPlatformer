using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// This script is used to handle soft object, can be jumped on from below.
/// </summary>
[RequireComponent(typeof(PlatformEffector2D))]
[RequireComponent(typeof(Collider2D))]
public class SoftObject2D : MonoBehaviour {

    [SerializeField] private float dropDownCooldown = 0.4f;
    private SpriteRenderer  spriteRenderer;
    private PlatformEffector2D effector;
    private Collider2D collider2d;

    private void Start() {
        collider2d = GetComponent<Collider2D>();
    }

    public void StartDropDownCooldown()
    {
        StartCoroutine(DropDownCooldown());
    }

    private IEnumerator DropDownCooldown( ) {

        collider2d.enabled = false;
        float time = dropDownCooldown;
        
        while (time > 0) {
            time -= Time.deltaTime;
            yield return null;
        }
        collider2d.enabled = true;
    }
    private void OnValidate() {
        
        // effector = GetComponent<PlatformEffector2D>();
        // effector.useColliderMask = true;
        // effector.colliderMask = LayerMask.GetMask("Everything");
        // effector.rotationalOffset = 0;
        // effector.useOneWay = true;
        // effector.useOneWayGrouping = false;
        // effector.surfaceArc = 180;
        // effector.useSideFriction = false;
        // effector.useSideBounce = false;
        // effector.sideArc = 1;
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = -1;
        
        collider2d = GetComponent<Collider2D>();
        collider2d.usedByEffector = true;
    }
}
