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


    protected override void Start()
    {
        base.Start();
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


    
    private void StartLevelTitleEffect(float duration, string title, float startDelay = 0) {
        
        if (levelTitleText) {
            
            // Set the text and initial alpha
            levelTitleText.text = title;
            levelTitleText.alpha = 0f;

            Sequence.Create(useUnscaledTime: true)
                .ChainDelay(startDelay) // Delay if specified
                .Chain(Tween.Alpha(levelTitleText, startValue: 0f, endValue: 1f, duration: duration * 0.5f, ease: Ease.OutSine)) // Fade in
                .ChainDelay(duration * 0.2f) // Delay to make text readable
                .Chain(Tween.Alpha(levelTitleText, startValue: 1f, endValue: 0f, duration: duration * 0.5f, ease: Ease.InSine)); // Fade out
        }
    }
    


    public override void OnActiveSceneChanged(Scene currentScene, Scene nextScene)
    {
        if (nextScene.name == "ShowcaseLevel")
        {
            UpdateAbilitiesUI();
            StartLevelTitleEffect(2, "Showcase Level", 2);
        } else {
            UpdateAbilitiesUI();
            StartLevelTitleEffect(2, SceneManager.GetActiveScene().name.Replace("Level", "Level ").Trim());
        }
    }
    
}
