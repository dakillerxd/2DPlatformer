using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPageGameplay : MenuPage
{
    
    [Header("Level Title")]
    [SerializeField] private  TMP_Text levelTitleText;
    
    [Header("Abilities")] 
    [SerializeField] private  Color abilityUnlocked = Color.white;
    [SerializeField] private  Color abilityLocked = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private GameObject doubleJump;
    [SerializeField] private GameObject wallSlide;
    [SerializeField] private GameObject wallJump;
    [SerializeField] private GameObject dash;


    private void Start()
    {
        SetupUI();
        UpdateAbilitiesUI();
    }
    

    private void UpdateAbilitiesUI()
    {
        if (PlayerController.Instance)
        {
            doubleJump.SetActive(PlayerController.Instance.doubleJumpAbility);
            wallSlide.SetActive(PlayerController.Instance.wallSlideAbility);
            wallJump.SetActive(PlayerController.Instance.wallJumpAbility);
            dash.SetActive(PlayerController.Instance.dashAbility);
        }
    }

    private void SetupUI() {
        
        if (levelTitleText)
        {
            levelTitleText.CrossFadeAlpha(0, 0, false);
        }
    }
    
    private void StartLevelTitleEffect(float duration, string title) {
        if (!levelTitleText) return;
        StartCoroutine(LevelTitleEffect(duration, title));
    }
    
    private IEnumerator LevelTitleEffect(float duration, string title) {
        
        if (!levelTitleText) yield break;
        levelTitleText.CrossFadeAlpha(0, 0, false);
        levelTitleText.text = title;
        levelTitleText.CrossFadeAlpha(1, 1, false);
        yield return new WaitForSeconds(1);
        levelTitleText.CrossFadeAlpha(0, duration, false);
    }



    public override void OnActiveSceneChanged(Scene currentScene, Scene nextScene)
    {
        if (nextScene.name == "ShowcaseLevel")
        {
            UpdateAbilitiesUI();
            StartLevelTitleEffect(1, "Showcase Level");
        } else {
            UpdateAbilitiesUI();
            StartLevelTitleEffect(1, SceneManager.GetActiveScene().name.Replace("Level", "Level ").Trim());
        }
    }
    
}
