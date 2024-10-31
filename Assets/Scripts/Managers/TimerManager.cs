using UnityEngine;
using CustomAttribute;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    public enum TimerType {
        CountUp,
        CountDown,
        Off,
    }

    [Header("Timer Settings")]
    public TimerType timerType;
    public bool paused = false;
    [ReadOnly] public float currentGameTime = 0f;


    [Header("Count Down Settings")]
    public float gameTime = 300f;
    public float gameTimeHard = 250f;
    public float warningTime = 100f;
    [ReadOnly] public bool warningTimeAnimationPlayed = false;
    [ReadOnly] public bool warningEndTimeAnimationPlayed = false;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void Update() {
        
        if (timerType == TimerType.CountDown) {
            CountTimeDown();
        }
        if (timerType == TimerType.CountUp) {
            CountTimeUp();
        }
    }


    void CountTimeDown() {

        if ((GameManager.Instance.currentGameState == GameStates.GamePlay)  && !paused) {

            currentGameTime -= Time.deltaTime;
            currentGameTime = Mathf.Max(currentGameTime, 0f);
            UIManager.Instance?.UpdateTimeUI();

            if ((currentGameTime <= warningTime) && (!warningTimeAnimationPlayed)) {
                warningTimeAnimationPlayed = true;

            }
            else if (currentGameTime <= 2 && (!warningEndTimeAnimationPlayed)) {
                warningEndTimeAnimationPlayed = true;

            }
        }
        
    }

    void CountTimeUp() {
        if ((GameManager.Instance.currentGameState == GameStates.GamePlay)  && !paused) {

            currentGameTime += Time.deltaTime;
            UIManager.Instance?.UpdateTimeUI();
        }
    }

    public void HandlePause(bool isPaused) {
        paused = isPaused;
    }

    public void PauseTimer() {
        if (paused) { paused = false; }
        else { paused = true; }
    }


    public void ResetTime(bool IsHardMode) {

        if (timerType == TimerType.CountDown) {
            if (IsHardMode) {
                currentGameTime = gameTimeHard;
            }
            else {
                currentGameTime = gameTime;
            }
        }
        else if (timerType == TimerType.CountUp) {
            currentGameTime = 0f;
        }
        
    }

}
