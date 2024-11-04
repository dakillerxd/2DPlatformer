using UnityEngine;
using UnityEngine.SceneManagement;
using CustomAttribute;
using System.Collections;
using VInspector;

public class SceneTeleporter2D : MonoBehaviour 
{
    [Header("Settings")]
    [SerializeField] private bool activated = false;
    [SerializeField] private bool goToNextLevel = true;
    [DisableIf("goToNextLevel")][SerializeField] private SceneField sceneToLoad;
    
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (other.CompareTag("Player")) {

            StartCoroutine(PlayAnimationAndTeleport());
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
    
    private IEnumerator PlayAnimationAndTeleport()
    {

        activated = true;
        SoundManager.Instance?.PlaySoundFX("Teleport", 0.1f);
        PlayerController2D.Instance.PlayAnimation("TeleportIn");
        PlayerController2D.Instance.SetPlayerState(PlayerState.Frozen);

        // Wait until the animation enters the state
        while (!PlayerController2D.Instance.animator.GetCurrentAnimatorStateInfo(0).IsName("Anim_PlayerTeleportIn"))
        {
            yield return null;
        }

        // Wait until the animation finishes
        while (PlayerController2D.Instance.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
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
}
