using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverController : MonoBehaviour
{

    [SerializeField] private TMP_Text scoreValue;
    [SerializeField] private TMP_Text matchResultText;

    private void Start()
    {
        int maxScore;
        int score = PlayerPrefs.GetInt("score", 0);
        scoreValue.text = score.ToString();

        if (PlayerPrefs.GetInt("challenge", 0) == 1)
        {
            if (PlayerPrefs.GetInt("won") == 0)
                matchResultText.text = "You lose";
            else
            {
                maxScore = PlayerPrefs.GetInt("ChallengeMaxScore", 0);

                if (score > maxScore) PlayerPrefs.SetInt("ChallengeMaxScore", score);
            }

        }
        else
        {
            matchResultText.text = "Awesome";

            maxScore = PlayerPrefs.GetInt("RegularMaxScore", 0);

            if (score > maxScore) PlayerPrefs.SetInt("RegularMaxScore", score);
        }

    }

    public void Replay()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
