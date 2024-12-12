using UnityEngine;
using UnityEngine.SceneManagement;
using CustomAttribute;
using System.Collections;
using VInspector;

public enum TeleportType
{
    NextScene,
    SelectedScene,
    SameSceneLocation
}

public class SceneTeleporter : MonoBehaviour 
{
    [Header("Settings")]
    [SerializeField] private TeleportType teleportType = TeleportType.NextScene;
    [SerializeField] private bool activated = false;
    [SerializeField] [Min(0f)] private float pullDuration = 1.5f;
    
    [Header("Selected Scene Teleport")]
    [SerializeField] private SceneField sceneToLoad;
    
    [Header("Same Scene Teleport")]
    [SerializeField] private Transform destinationPoint;
    [SerializeField] [Min(0f)] private float sameSceneTeleportDelay = 0.5f;
    
    [Header("References")]
    [SerializeField] public Animator animator;

    private void Start()
    {
        VFXManager.Instance?.PlayAnimationTrigger(animator, "Spawn");
        
        // Validate settings
        if (teleportType == TeleportType.SameSceneLocation && destinationPoint == null)
        {
            Debug.LogWarning("Destination point is not set for same scene teleportation!");
        }
        else if (teleportType == TeleportType.SelectedScene && sceneToLoad == null)
        {
            Debug.LogWarning("Scene to load is not set for selected scene teleportation!");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || activated) return;
        
        Transform parentTransform = other.transform.root;
        StartCoroutine(PlayAnimationAndTeleport(parentTransform));
    }

    private void ExecuteTeleport() 
    {
        switch (teleportType)
        {
            case TeleportType.NextScene:
                SaveManager.Instance.SaveInt("SavedCheckpoint", 0);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                break;
                
            case TeleportType.SelectedScene:
                if (sceneToLoad != null) {
                    SaveManager.Instance.SaveInt("SavedCheckpoint", 0);
                    SceneManager.LoadScene(sceneToLoad.SceneName);
                }
                break;
                
            case TeleportType.SameSceneLocation:
                if (destinationPoint != null) {
                    PlayerController.Instance.transform.position = destinationPoint.position;
                }
                activated = false; // Reset activation for same-scene teleports
                TeleportOut();
                break;
        }
    }

    private void TeleportOut()
    {
        StartCoroutine(VFXManager.Instance?.LerpChromaticAberration(false, 1.5f));
        StartCoroutine(VFXManager.Instance?.LerpLensDistortion(false, 1.5f));
    }
    
    private IEnumerator PlayAnimationAndTeleport(Transform objectTransform) 
    {
        activated = true;
        Rigidbody2D rb = objectTransform.GetComponentInChildren<Rigidbody2D>();
        
        if (rb == null) {
            Debug.LogError("No Rigidbody2D found on the player or its children!");
            yield break;
        }

        // Store original settings
        RigidbodyType2D oRigidbodyType = rb.bodyType;
        RigidbodyConstraints2D originalConstraints = rb.constraints;
        Vector2 originalVelocity = rb.linearVelocity;

        // Start effects
        VFXManager.Instance?.PlayAnimationTrigger(animator, "Teleport");
        SoundManager.Instance?.PlaySoundFX("Teleport", 0.1f);
        StartCoroutine(VFXManager.Instance?.LerpChromaticAberration(true, 2.5f));
        StartCoroutine(VFXManager.Instance?.LerpLensDistortion(true, 2f));
        CameraController.Instance?.ShakeCamera(2f, 2f, 2, 2);
        PlayerController.Instance.PlayAnimationTrigger("TeleportIn");
        
        // Start pulling
        Vector2 startPos = rb.position;
        Vector2 targetPos = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < pullDuration) {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / pullDuration);
            float smoothT = Mathf.SmoothStep(0, 1, t);
                
            // Calculate new position
            Vector2 newPosition = Vector2.Lerp(startPos, targetPos, smoothT);
                
            // Move the rigidbody
            rb.linearVelocity = Vector2.zero;
            rb.MovePosition(newPosition);
                
            yield return null;
        }
            
        // Wait a moment at the end
        yield return new WaitForSeconds(0.2f);
        
        if (teleportType == TeleportType.SameSceneLocation)
        {
            yield return new WaitForSeconds(sameSceneTeleportDelay);
        }
        
        
        ExecuteTeleport();
    }
}