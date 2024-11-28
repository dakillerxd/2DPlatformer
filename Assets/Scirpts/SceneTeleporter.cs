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

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (other.CompareTag("Player")) {
            Transform parentTransform = other.transform.root;
            StartCoroutine(PlayAnimationAndTeleport(parentTransform));
        }
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
        SoundManager.Instance?.PlaySoundFX("Teleport", 0.1f);
        StartCoroutine(VFXManager.Instance?.LerpChromaticAberration(true, 2.5f));
        StartCoroutine(VFXManager.Instance?.LerpLensDistortion(true, 2f));
        PlayerController.Instance.PlayAnimation("TeleportIn");
        PlayerController.Instance.SetPlayerState(PlayerState.Frozen);
        
        // Wait until the animation enters the state
        while (!PlayerController.Instance.animator.GetCurrentAnimatorStateInfo(0).IsName("Anim_PlayerTeleportIn")) {
            MoveObjectToMiddle(objectTransform);
            yield return null;
        }

        // Wait until the animation finishes
        while (PlayerController.Instance.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f) {
            yield return null;
        }
        
        if (goToNextLevel)
        {
            GoToNextLevel();
        }
        else
        {
            GoToSelectedLevel();
        }
    }

    private void MoveObjectToMiddle(Transform objectTransform, float moveSpeed = 1f) {
        Vector3 targetPosition = transform.position;
        Vector3 currentPosition = objectTransform.position;
        objectTransform.position = Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime * moveSpeed);
    }
}