using System;
using UnityEngine;
using VInspector;

public class Checkpoint2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider2D coll;
    [SerializeField] public SpriteRenderer spriteRenderer;

    private void Start()
    {
        if (!coll) coll = GetComponent<Collider2D>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        switch (collision.gameObject.tag) {
            case "Player":
                
                ActivateCheckpoint();
                break;
            case "Enemy":
                DeactivateCheckpoint();
                break;
            default:
                break;
        }
    }

    [Button] private void ActivateCheckpoint() {
        
        CheckpointManager2D.Instance.ActivateCheckpoint(this);
        PlayerController2D.Instance.HealToFullHealth();
    }

    private void DeactivateCheckpoint() {
        CheckpointManager2D.Instance.DeactivateCheckpoint(this);
    }
}
