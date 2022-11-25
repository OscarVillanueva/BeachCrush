using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreBoardController : MonoBehaviour
{

    [SerializeField] private TMP_Text challengeScore;
    [SerializeField] private TMP_Text regularScore;

    // Start is called before the first frame update
    void Start()
    {
        challengeScore.text = PlayerPrefs.GetInt("ChallengeMaxScore", 0).ToString();
        regularScore.text = PlayerPrefs.GetInt("RegularMaxScore", 0).ToString();
    }

    public void GoBack()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
