using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStatistics : MonoBehaviour {
    public Text ScoreText;
    public int Score { get; private set; }

    public void ResetStatistics() {
        Score = 0;
        ScoreText.text = $"{Score}";
        ScoreText.GetComponent<ScoreText>().Pop();
    }

    public void IncreaseScore(int value) {
        Score += value;
        ScoreText.text = $"{Score}";
        ScoreText.GetComponent<ScoreText>().Pop();
    }

    public void DecreaseScore(int value) {
        Score -= value;
        ScoreText.text = $"{Score}";
        ScoreText.GetComponent<ScoreText>().Pop();
    }
}
