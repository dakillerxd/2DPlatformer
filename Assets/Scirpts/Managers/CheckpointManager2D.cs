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
    [SerializeField] public Checkpoint2D activeCheckpoint;
    [SerializeField] private List<Checkpoint2D> checkpointList = new List<Checkpoint2D>();
    
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
        checkpointList.Add(newCheckpoint.GetComponent<Checkpoint2D>());
        SeCheckpointColor(newCheckpoint.GetComponent<Checkpoint2D>(), disabledCheckpointColor);
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
        checkpointList = FindObjectsByType<Checkpoint2D>(FindObjectsSortMode.None).ToList();
        Debug.Log($"Found {checkpointList.Count} checkpoints in the scene.");

        // Set the color for each checkpoint
        foreach (Checkpoint2D checkpoint in checkpointList) {

            SeCheckpointColor(checkpoint, disabledCheckpointColor);
        }
    }
    

    public void ActivateCheckpoint(Checkpoint2D checkpoint) {

        if (activeCheckpoint == checkpoint) return; // If the active checkpoint is the same as the new one do nothing
        
        DeactivateLastCheckpoint();
        activeCheckpoint = checkpoint;
        SpawnParticleEffect(activateVfx, activeCheckpoint.transform.position, activeCheckpoint.transform.rotation, activeCheckpoint.transform);
        SeCheckpointColor(activeCheckpoint, activeCheckpointColor);
        SoundManager.Instance?.PlaySoundFX("Checkpoint Set");
    }

    public void DeactivateCheckpoint(Checkpoint2D checkpoint)
    {
        if (activeCheckpoint  == checkpoint) DeactivateLastCheckpoint();
    }
    private void DeactivateLastCheckpoint() {

        if (!activeCheckpoint) return; // If there is no active checkpoint then do nothing
        
        SpawnParticleEffect(deactivateVfx, activeCheckpoint.transform.position, activeCheckpoint.transform.rotation, activeCheckpoint.transform);
        SeCheckpointColor(activeCheckpoint, disabledCheckpointColor);
        activeCheckpoint = null;
        
    }
    
    
    private void SeCheckpointColor(Checkpoint2D checkpoint, Color color) {

        checkpoint.spriteRenderer.color = color;
    }
    
    
    private void SpawnParticleEffect(ParticleSystem effect, Vector3 position, Quaternion rotation, Transform parent) {

        if (effect == null) return; // If no effect when selected in the inspector then do nothing

        Instantiate(effect, position, rotation, parent);
        
    }
    
#if UNITY_EDITOR
    private List<Checkpoint2D> previousList = new List<Checkpoint2D>();
    private bool isProcessingDestroy = false;

    public void OnBeforeSerialize()
    {
        if (Application.isPlaying || isProcessingDestroy) return;
        
        // Find objects that were in the previous list but not in the current list
        foreach (var prevCheckpoint in previousList)
        {
            if (prevCheckpoint != null && !checkpointList.Contains(prevCheckpoint))
            {
                // Schedule destruction for next frame
                EditorApplication.delayCall += () =>
                {
                    isProcessingDestroy = true;
                    if (prevCheckpoint != null)
                    {
                        Undo.RecordObject(this, "Delete Checkpoint");
                        DestroyImmediate(prevCheckpoint.gameObject);
                        EditorUtility.SetDirty(this);
                        EditorSceneManager.MarkSceneDirty(gameObject.scene);
                    }
                    isProcessingDestroy = false;
                };
            }
        }

        // Update previous list
        previousList = new List<Checkpoint2D>(checkpointList);
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

        var manager = FindAnyObjectByType<CheckpointManager2D>();
        if (manager != null && manager.checkpointList != null)
        {
            // Check both for null component and null gameObject
            if (manager.checkpointList.RemoveAll(checkpoint => 
                checkpoint == null || checkpoint.gameObject == null) > 0)
            {
                manager.RenameCheckpoints();
                EditorUtility.SetDirty(manager);
                EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
            }
        }
    }
#endif
}
