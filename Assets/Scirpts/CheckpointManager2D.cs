using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CustomAttribute;
public class CheckpointManager2D : MonoBehaviour
{
    public static CheckpointManager2D Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] [ReadOnly] public Vector2 playerSpawnPoint;
    [SerializeField] [ReadOnly] public GameObject activeCheckpoint;
    [SerializeField] [ReadOnly] private GameObject[] checkpointList;
    
    [Header("Effects")]
    [SerializeField] private Color disabledCheckpointColor = Color.white;
    [SerializeField] private Color activeCheckpointColor = Color.green;
    [SerializeField] private ParticleSystem activateVfx;
    [SerializeField] private ParticleSystem deactivateVfx;



    
    private void Awake() {

       if (Instance != null && Instance != this) {

            Destroy(gameObject);

       } else {

            Instance = this;
       }

    }

    private void Start() {

        FindAllCheckpoints();
    }

    private void FindAllCheckpoints() {

        // Find all the checkpoints in the scene
        checkpointList = GameObject.FindGameObjectsWithTag("Checkpoint");
        checkpointList = checkpointList.ToArray();
        Debug.Log($"Found {checkpointList.Length} checkpoints in the scene.");

        // Set the color for each checkpoint
        foreach (GameObject checkpoint in checkpointList) {

            SeCheckpointColor(checkpoint, disabledCheckpointColor);
        }
    }


    public void SetSpawnPoint(Vector2 newSpawnPoint) {

        playerSpawnPoint = newSpawnPoint;
        Debug.Log("Set spawn point to: " + playerSpawnPoint);
    }



    public void ActivateCheckpoint(GameObject checkpoint) {

        if (activeCheckpoint == checkpoint) return; // If the active checkpoint is the same as the new one do nothing

        // CameraController2D.Instance?.ShakeCamera(0.4f,0.2f);
        DeactivateLastCheckpoint();
        activeCheckpoint = checkpoint;
        SpawnParticleEffect(activateVfx, activeCheckpoint.transform.position, activeCheckpoint.transform.rotation, activeCheckpoint.transform);
        SeCheckpointColor(activeCheckpoint, activeCheckpointColor);
        SoundManager.Instance?.PlaySoundFX("Checkpoint Set");
    }


    private void DeactivateLastCheckpoint() {

        if (!activeCheckpoint) return; // If there is no active checkpoint then do nothing


        SpawnParticleEffect(deactivateVfx, activeCheckpoint.transform.position, activeCheckpoint.transform.rotation, activeCheckpoint.transform);
        SeCheckpointColor(activeCheckpoint, disabledCheckpointColor);
        activeCheckpoint = null;
        
    }


    private void SpawnParticleEffect(ParticleSystem effect, Vector3 position, Quaternion rotation, Transform parent) {

        if (effect == null) return; // If no effect when selected in the inspector then do nothing

        Instantiate(effect, position, rotation, parent);
        
    }

    
    private void SeCheckpointColor(GameObject checkpoint, Color color) {

        checkpoint.GetComponent<SpriteRenderer>().color = color;
    }
}
