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

            StartCoroutine(PlayAnimationAndTeleport(PlayerController.Instance.GetComponent<Rigidbody2D>()));

        }
    }

    private void GoToSelectedLevel() {
        if (sceneToLoad != null) {
            SceneManager.LoadScene(sceneToLoad.SceneName);
        }
    }
    
    private void GoToNextLevel() {
        
        if (SceneManager.GetActiveScene().buildIndex + 1 >= SceneManager.sceneCountInBuildSettings) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
        
    }
    
    private IEnumerator PlayAnimationAndTeleport(Rigidbody2D objectRigidbody) {

        activated = true;
        SoundManager.Instance?.PlaySoundFX("Teleport", 0.1f);
        PlayerController.Instance.PlayAnimation("TeleportIn");
        PlayerController.Instance.SetPlayerState(PlayerState.Frozen);
        

        // Wait until the animation enters the state
        while (!PlayerController.Instance.animator.GetCurrentAnimatorStateInfo(0).IsName("Anim_PlayerTeleportIn")) {
            MoveObjectToMiddle(objectRigidbody);
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


    private void MoveObjectToMiddle(Rigidbody2D objectRigidbody, float moveSpeed = 1f) {

        Vector2 targetPosition = transform.position;
        Vector2 currentPosition = objectRigidbody.position;
        Vector2 direction = (targetPosition - currentPosition).normalized;
        float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);


        if (distanceToTarget < 0.1f) //  Stop when close to the target
        {
            objectRigidbody.linearVelocity = Vector2.zero;
            objectRigidbody.position = targetPosition;
            return;
        }

        // Gradually slow down as we get closer to the target
        float speedMultiplier = Mathf.Min(distanceToTarget, 1f);
    
        // Set the velocity based on direction and speed
        objectRigidbody.linearVelocity = speedMultiplier * moveSpeed * direction;
    }
}
