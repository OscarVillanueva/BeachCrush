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
        scoreValue.text = PlayerPrefs.GetInt("score", 0).ToString();

        if (PlayerPrefs.GetInt("challenge", 0) == 1)
        {
            if (PlayerPrefs.GetInt("won") == 0)
                matchResultText.text = "You lose";
        }
        else matchResultText.text = "Awesome";
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
