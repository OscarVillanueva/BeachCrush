using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text movesTexts;
    [SerializeField] private TMP_Text scoreTexts;
    [SerializeField] private TMP_Text challengeValue;
    [SerializeField] private Image challengeImage;

    private int lookingFor = 33;
    private int moveCounter;
    private int score;

    public static GUIManager sharedInstance;

    public int Score
    {
        get => score;
        set
        {
            int combo = BoardManager.sharedInstance ? BoardManager.sharedInstance.Combo : 1;
            score = value + combo;
            scoreTexts.text = "Score: " + score;
        }
    }

    public int MoveCounter
    {
        get => moveCounter;
        set
        {
            moveCounter = value;
            movesTexts.text = "Moves: " + moveCounter;

            if (moveCounter <= 0)
            {
                moveCounter = 0;
                StartCoroutine(GameOver());
            }
        }
    }

    private void Awake()
    {
        if (!sharedInstance) sharedInstance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Score = 0;
        MoveCounter = 30;

        if (PlayerPrefs.GetInt("challenge", 0) != 1)
        {
            challengeImage.enabled = false;
            challengeValue.enabled = false;
        }
        else
        {
            challengeValue.text = lookingFor.ToString();
        }

    }

    public void SetChallengeIcon(Sprite icon)
    {
        challengeImage.sprite = icon;
    }

    public void SetChallengeValue(int value)
    {
        lookingFor = lookingFor - value;

        if (lookingFor > 0) challengeValue.text = lookingFor.ToString();
        else
        {
            challengeValue.text = "0";
            StartCoroutine(GameOver());
        }

    }

    private IEnumerator GameOver()
    {
        // Esperamos a que termine de mover los caramelos en pantalla
        yield return new WaitUntil(() => !BoardManager.sharedInstance.IsShifting);

        // Damos otro tiempo para que el usuario vea los resultados en pantalla
        yield return new WaitForSeconds(1.0f);

        PlayerPrefs.SetInt("won", moveCounter > 0 ? 1 : 0);

        // Guardamos el score antes de movernos de pantalla
        PlayerPrefs.SetInt("score", Score);
        SceneManager.LoadScene("GameOverScene");
    }
}
