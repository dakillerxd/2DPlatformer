using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VInspector;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


public class CheckpointManager : MonoBehaviour, ISerializationCallbackReceiver
{
    public static CheckpointManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] public Vector2 playerSpawnPoint;
    [SerializeField] public SceneTeleporter startTeleporter;
    [SerializeField] public Checkpoint activeCheckpoint;
    [SerializeField] private List<Checkpoint> checkpointList = new List<Checkpoint>();

    [Header("References")]
    [SerializeField] private GameObject checkpointPrefab;
    
    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize() { }

    private void Awake() 
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
        } 
        else 
        {
            Instance = this;
        }
    }

    private void Start() 
    {
        FindAllCheckpoints();
    }

    public void SetSpawnPoint(Vector2 newSpawnPoint) 
    {
        playerSpawnPoint = newSpawnPoint;
        Debug.Log("Set spawn point to: " + playerSpawnPoint);
    }

    public void PlayStartTeleporterAnimation()
    {
        if (!startTeleporter) return;
        
        VFXManager.Instance?.PlayAnimationTrigger(startTeleporter.animator, "In");
    }
     
    [Button] private void AddCheckpoint() 
    {
        GameObject newCheckpoint = Instantiate(checkpointPrefab, transform.position, Quaternion.identity,transform);
        checkpointList.Add(newCheckpoint.GetComponent<Checkpoint>());
        newCheckpoint.name = "Checkpoint " + checkpointList.Count;
        RenameCheckpoints();
        
        #if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = newCheckpoint;
        #endif
    }
    
    private void RenameCheckpoints()
    {
        checkpointList.RemoveAll(w => w == null);
        for (int i = 0; i < checkpointList.Count; i++)
        {
            checkpointList[i].name = "Checkpoint " + i;
        }
    }
    
    
    [Button] private void FindAllCheckpoints() 
    {
        checkpointList = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None).ToList();
        Debug.Log($"Found {checkpointList.Count} checkpoints in the scene.");

        // foreach (Checkpoint checkpoint in checkpointList) 
        // {
        //     checkpoint.SeCheckpointColor(disabledCheckpointColor);
        // }

        RenameCheckpoints();
    }

    public void ActivateCheckpoint(Checkpoint checkpoint) 
    {
        if (activeCheckpoint == checkpoint) return;
        
        DeactivateLastCheckpoint();
        activeCheckpoint = checkpoint;
    }

    public void DeactivateCheckpoint(Checkpoint checkpoint)
    {
        if (activeCheckpoint == checkpoint) DeactivateLastCheckpoint();
    }

    private void DeactivateLastCheckpoint() 
    {
        if (!activeCheckpoint) return;
        activeCheckpoint.DeactivateCheckpoint();
        activeCheckpoint = null;
    }
    
    #if UNITY_EDITOR
    
    private List<Checkpoint> previousList = new List<Checkpoint>();
    private bool isProcessingDestroy = false;

    // Editor-only serialization
    private void EditorOnBeforeSerialize()
    {
        if (Application.isPlaying || isProcessingDestroy) return;
        
        foreach (var prevCheckpoint in previousList)
        {
            if (prevCheckpoint != null && !checkpointList.Contains(prevCheckpoint))
            {
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

        previousList = new List<Checkpoint>(checkpointList);
    }

    [InitializeOnLoadMethod]
    private static void RegisterHierarchyChangeCallback()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    private static void OnHierarchyChanged()
    {
        if (Application.isPlaying) return;

        var manager = FindAnyObjectByType<CheckpointManager>();
        if (manager != null && manager.checkpointList != null)
        {
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