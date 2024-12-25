using UnityEngine;
using VInspector;

public class CameraManager : MonoBehaviour
{
    
    public static CameraManager Instance { get; private set; }
    
    
    [Header("Settings")] 
    [SerializeField] private Camera activeCamera;
    [SerializeField] private Transform target;
    
    
    [Header("References")]
    [SerializeField] private Transform triggersHolder;
    [SerializeField] private CameraTrigger triggerPrefab;
    
    
    private void Awake() {

        if (Instance != null && Instance != this) {

            Destroy(gameObject);

        } else {
            
            Instance = this; 
        }
    }
    
    
    
    
    
    
    

}
