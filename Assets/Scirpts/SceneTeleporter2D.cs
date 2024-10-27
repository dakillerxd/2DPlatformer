using UnityEngine;
using UnityEngine.SceneManagement;
using CustomAttribute;
public class SceneTeleporter2D : MonoBehaviour
{

    [SerializeField] private SceneField sceneToLoad;


    public void GoToSelectedLevel() {
        if (sceneToLoad != null) {
            SceneManager.LoadScene(sceneToLoad.SceneName);
        }
    }
}
