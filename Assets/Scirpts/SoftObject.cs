using UnityEngine;
using System.Collections;


[RequireComponent(typeof(PlatformEffector2D))]
[RequireComponent(typeof(Collider2D))]
public class SoftObject : MonoBehaviour {

    [SerializeField] private float dropDownCooldown = 0.4f;
    private SpriteRenderer  _spriteRenderer;
    private PlatformEffector2D _effector2;
    private Collider2D _collider2d;
    private bool _triggered;

    private void Start() {
        _collider2d = GetComponent<Collider2D>();
        _effector2 = GetComponent<PlatformEffector2D>();
        _triggered = false;
    }

    public void StartDropDownCooldown()
    {
        if (_triggered) return;
        _triggered = true;
        StartCoroutine(DropDownCooldown());
    }

    private IEnumerator DropDownCooldown( ) {

        _collider2d.enabled = false;
        _effector2.enabled = false;
        float time = dropDownCooldown;
        
        while (time > 0) {
            time -= Time.deltaTime;
            yield return null;
        }
        _collider2d.enabled = true;
        _effector2.enabled = true;
        _triggered = false;
    }
    // private void OnValidate() {
    //     
    //     effector = GetComponent<PlatformEffector2D>();
    //     effector.useColliderMask = true;
    //     effector.colliderMask = LayerMask.GetMask("Everything");
    //     effector.rotationalOffset = 0;
    //     effector.useOneWay = true;
    //     effector.useOneWayGrouping = false;
    //     effector.surfaceArc = 180;
    //     effector.useSideFriction = false;
    //     effector.useSideBounce = false;
    //     effector.sideArc = 1;
    //     
    //     _spriteRenderer = GetComponent<SpriteRenderer>();
    //     _spriteRenderer.sortingOrder = -1;
    //     
    //     _collider2d = GetComponent<Collider2D>();
    //     _collider2d.usedByEffector = true;
    // }
}
