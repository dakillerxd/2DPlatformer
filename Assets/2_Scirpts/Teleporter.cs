using UnityEngine;
using UnityEngine.SceneManagement;
using CustomAttribute;
using System.Collections;
using VInspector;


public class Teleporter : MonoBehaviour 
{
    [Header("Settings")]
    [SerializeField] private TeleportType teleportType = TeleportType.NextScene;
    [SerializeField] private bool activated = false;
    [SerializeField] [Min(0f)] private float pullDuration = 1.5f;
    
    [Header("Selected Scene Teleport")]
    [SerializeField] private SceneField sceneToLoad;
    
    [Header("Teleporter")]
    [SerializeField] private Teleporter connectedTeleporter;
    
    [Header("Player Abilities")]
    [SerializeField] private bool receiveAbilityOnUse;
    [EnableIf(nameof(receiveAbilityOnUse))]
    [SerializeField] private bool doubleJump;
    [SerializeField] private bool wallSlide;
    [SerializeField] private bool wallJump;
    [SerializeField] private bool dash;
    [EndIf]
    
    [Header("References")]
    [SerializeField] public Animator animator;

    private void Start()
    {
        // Validate settings
        if (teleportType == TeleportType.ConnectedTeleporter && connectedTeleporter == null)
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
        StartCoroutine(StartTeleportation(parentTransform));
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
                SaveManager.Instance.SaveInt("SavedCheckpoint", 0);
                SceneManager.LoadScene(sceneToLoad.SceneName);
                break;
                
            case TeleportType.ConnectedTeleporter:
                CheckpointManager.Instance?.SetStartTeleporter(connectedTeleporter);
                PlayerController.Instance?.RespawnFromTeleporter();
                activated = false;
                break;
        }
    }
    
    public void PlayTeleporterEffects()
    {
        
        VFXManager.Instance?.PlayAnimationTrigger(animator, "Spawn");
        StartCoroutine(VFXManager.Instance?.LerpColorAdjustments(true, 0.2f));
        StartCoroutine(VFXManager.Instance?.LerpChromaticAberration(false, 1.5f));
        StartCoroutine(VFXManager.Instance?.LerpLensDistortion(false, 1.5f));
        
        if (!receiveAbilityOnUse) return;
        if (doubleJump) PlayerController.Instance?.ReceiveAbility(PlayerAbilities.DoubleJump, false);
        if (wallSlide) PlayerController.Instance?.ReceiveAbility(PlayerAbilities.WallSlide, false);
        if (wallJump) PlayerController.Instance?.ReceiveAbility(PlayerAbilities.WallJump, false);
        if (dash) PlayerController.Instance?.ReceiveAbility(PlayerAbilities.Dash, false);
    }
    
    private IEnumerator StartTeleportation(Transform objectTransform) 
    {
        activated = true;
        
        Rigidbody2D rb = objectTransform.GetComponentInChildren<Rigidbody2D>();
        PlayerController player = objectTransform.GetComponentInParent<PlayerController>();
        
        if (!rb) {
            Debug.LogError("No Rigidbody2D found!");
            yield break;
        }
        
        player.TeleportOutOfLevel();
        VFXManager.Instance?.PlayAnimationTrigger(animator, "Teleport"); // Teleporter animation

        
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
        

        ExecuteTeleport();
    }
}