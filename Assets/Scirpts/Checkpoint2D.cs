using UnityEngine;
using VInspector;

public class Checkpoint2D : MonoBehaviour
{

    
    [Button] private void ActivateCheckpoint() {
        
        CheckpointManager2D.Instance.ActivateCheckpoint(gameObject);
    }
}
