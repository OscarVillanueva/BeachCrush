using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void Play()
    {
        PlayerPrefs.SetInt("challenge", 0);
        SceneManager.LoadScene("GameScene");
    }

    public void PlayWithChallenge()
    {
        PlayerPrefs.SetInt("challenge", 1);
        SceneManager.LoadScene("GameScene");
    }

    public void SeeScoreBoard()
    {
        SceneManager.LoadScene("ScoreBoardScene");
    }
}
