using UnityEngine;
using UnityEngine.SceneManagement;
using CustomAttribute;
using System.Collections;
using VInspector;

public class SceneTeleporter : MonoBehaviour 
{
    [Header("Settings")]
    [SerializeField] private bool activated = false;
    [SerializeField] private bool goToNextLevel = true;
    [SerializeField] private SceneField sceneToLoad;
    [SerializeField] [Min(0f)] private float pullDuration = 1.5f;
    
    [Header("References")]
    [SerializeField] public Animator animator;

    private void Start()
    {
        VFXManager.Instance?.PlayAnimationTrigger(animator, "Spawn");
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || activated) return;
        
        Transform parentTransform = other.transform.root;
        StartCoroutine(PlayAnimationAndTeleport(parentTransform));
    }

    private void GoToSelectedLevel() {
        if (sceneToLoad != null) {
            SceneManager.LoadScene(sceneToLoad.SceneName);
        }
    }
    
    private void GoToNextLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    private IEnumerator PlayAnimationAndTeleport(Transform objectTransform) {
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

        // Freeze position at the end
        rb.bodyType = RigidbodyType2D.Kinematic;;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // Wait a moment at the end
        yield return new WaitForSeconds(0.2f);
        
        if (goToNextLevel)
        {
            GoToNextLevel();
        }
        else
        {
            GoToSelectedLevel();
        }

        // Restore original settings (though not strictly necessary since we're changing scenes)
        rb.bodyType = oRigidbodyType;
        rb.constraints = originalConstraints;
        rb.linearVelocity = originalVelocity;
    }
}