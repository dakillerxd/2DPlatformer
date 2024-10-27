using UnityEngine;


public class ScoreManager : MonoBehaviour
{
 
    public static ScoreManager Instance { get; private set; }


    // [Range(0, 100)] public int scorePercentageToWin = 70;
    public int currentScore = 0; 



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


    public void UpdateScore(int score)
    {
        currentScore += score;
        UIManager.Instance?.UpdateScoreUI();
        GameManager.Instance?.CheckGameOver();
    }


    public void ResetScore()
    {
        currentScore = 0;
        UIManager.Instance?.UpdateScoreUI();
    }
}
