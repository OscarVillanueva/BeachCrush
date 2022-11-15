using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text movesTexts;
    [SerializeField] private TMP_Text scoreTexts;

    private int moveCounter;
    private int score;

    public static GUIManager sharedInstance;

    public int Score
    {
        get => score;
        set
        {
            score = value;
            scoreTexts.text = "Score: " + score;
        }
    }

    public int MoveCounter
    {
        get => moveCounter;
        set
        {
            moveCounter = value;
            movesTexts.text = "Moves: " + score;
        }
    }

    private void Awake()
    {
        if (!sharedInstance) sharedInstance = this;
    }

    private void Start()
    {
        Score = 0;
        MoveCounter = 30;
    }
}
