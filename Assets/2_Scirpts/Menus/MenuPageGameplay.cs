using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using PrimeTween;

public class MenuPageGameplay : MenuPage
{
    
    [Header("UI Elements")]
    [SerializeField] private  TMP_Text levelTitleText;
    [SerializeField] private  Color abilityUnlocked = Color.white;
    [SerializeField] private  Color abilityLocked = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private GameObject doubleJump;
    [SerializeField] private GameObject wallSlide;
    [SerializeField] private GameObject wallJump;
    [SerializeField] private GameObject dash;


    private void Start()
    {
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


    
    private void StartLevelTitleEffect(float duration, string title) {
        
        if (levelTitleText)
        {
            levelTitleText.text = title;
            Tween.Alpha(levelTitleText, startValue: 1, endValue:0, duration, ease: Ease.InOutSine, useUnscaledTime: true);
        }
        
    }
    


    public override void OnActiveSceneChanged(Scene currentScene, Scene nextScene)
    {
        if (nextScene.name == "ShowcaseLevel")
        {
            UpdateAbilitiesUI();
            StartLevelTitleEffect(1, "Showcase Level");
        } else {
            UpdateAbilitiesUI();
            StartLevelTitleEffect(2, SceneManager.GetActiveScene().name.Replace("Level", "Level ").Trim());
        }
    }
    
}
