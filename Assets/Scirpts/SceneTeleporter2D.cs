using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using CustomAttribute;
public class SceneTeleporter2D : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool goToNextLevel = false;
    [SerializeField] private SceneField sceneToLoad;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (goToNextLevel)
            {
                GoToNextLevel();
            }
            GoToSelectedLevel();
            
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
}
