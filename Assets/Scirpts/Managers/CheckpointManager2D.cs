using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VInspector;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
public class CheckpointManager2D : MonoBehaviour, ISerializationCallbackReceiver
{
    public static CheckpointManager2D Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] public Vector2 playerSpawnPoint;
    [SerializeField] public GameObject activeCheckpoint;
    [SerializeField] private List<GameObject> checkpointList = new List<GameObject>();
    
    [Header("Effects")]
    [SerializeField] private Color disabledCheckpointColor = Color.white;
    [SerializeField] private Color activeCheckpointColor = Color.green;
    [SerializeField] private ParticleSystem activateVfx;
    [SerializeField] private ParticleSystem deactivateVfx;

    [Header("References")]
    [SerializeField] private GameObject checkpointPrefab;



    
    private void Awake() {

       if (Instance != null && Instance != this) {

            Destroy(gameObject);

       } else {

            Instance = this;
       }
    }

    private void Start() {

        if (activeCheckpoint) {ActivateCheckpoint(activeCheckpoint);}
        FindAllCheckpoints();
    }


    public void SetSpawnPoint(Vector2 newSpawnPoint) {

        playerSpawnPoint = newSpawnPoint;
        Debug.Log("Set spawn point to: " + playerSpawnPoint);
    }

    
    [Button] private void AddCheckpoint() {
        GameObject newCheckpoint = Instantiate(checkpointPrefab, transform.position, Quaternion.identity);
        checkpointList.Add(newCheckpoint);
        SeCheckpointColor(newCheckpoint, disabledCheckpointColor);
        newCheckpoint.name = "Checkpoint " + checkpointList.Count;
        RenameCheckpoints();
        
    }
    
    private void RenameCheckpoints()
    {
        // Remove any null elements from the list
        checkpointList.RemoveAll(w => w == null);
    
        // Rename all checkpoints to match their current position
        for (int i = 0; i < checkpointList.Count; i++)
        {
            checkpointList[i].name = "Checkpoint " + i;
        }
    }
    
    
    [Button] private void FindAllCheckpoints() {

        // Find all the checkpoints in the scene
        checkpointList = GameObject.FindGameObjectsWithTag("Checkpoint").ToList();
        Debug.Log($"Found {checkpointList.Count} checkpoints in the scene.");

        // Set the color for each checkpoint
        foreach (GameObject checkpoint in checkpointList) {

            SeCheckpointColor(checkpoint, disabledCheckpointColor);
        }
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
    
    
    private void SeCheckpointColor(GameObject checkpoint, Color color) {

        checkpoint.GetComponent<SpriteRenderer>().color = color;
    }
    
    
    private void SpawnParticleEffect(ParticleSystem effect, Vector3 position, Quaternion rotation, Transform parent) {

        if (effect == null) return; // If no effect when selected in the inspector then do nothing

        Instantiate(effect, position, rotation, parent);
        
    }
    
#if UNITY_EDITOR
    private List<GameObject> previousList = new List<GameObject>();

    public void OnBeforeSerialize()
    {
        if (Application.isPlaying) return;
        
        // Find objects that were in the previous list but not in the current list
        foreach (var prevCheckpoint in previousList)
        {
            if (prevCheckpoint != null && !checkpointList.Contains(prevCheckpoint))
            {
                // Schedule destruction for next frame
                EditorApplication.delayCall += () =>
                {
                    if (prevCheckpoint != null)
                    {
                        Undo.RecordObject(this, "Delete Checkpoint");
                        DestroyImmediate(prevCheckpoint);
                        EditorUtility.SetDirty(this);
                        EditorSceneManager.MarkSceneDirty(gameObject.scene);
                    }
                };
            }
        }

        // Update previous list
        previousList = new List<GameObject>(checkpointList);
    }

    public void OnAfterDeserialize()
    {
        // Not needed
    }

    // This handles when objects are deleted in the hierarchy
    [InitializeOnLoadMethod]
    private static void RegisterHierarchyChangeCallback()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    private static void OnHierarchyChanged()
    {
        if (Application.isPlaying) return;

        var managers = FindObjectsOfType<CheckpointManager2D>();
        foreach (var manager in managers)
        {
            if (manager != null && manager.checkpointList != null)
            {
                if (manager.checkpointList.RemoveAll(checkpoint => checkpoint == null) > 0)
                {
                    manager.RenameCheckpoints();
                    EditorUtility.SetDirty(manager);
                    EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
                }
            }
        }
    }
#endif
}
