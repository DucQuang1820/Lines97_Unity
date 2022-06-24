using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIManager : Singleton<UIManager>
{
    [SerializeField] private Text highScoreText = null;
    [SerializeField] private Text currentScoreText = null;
    [SerializeField] private Text timerText = null;

    public void SetHighScoreText(uint score) => highScoreText.text = score.ToString("D5");
    public void SetCurrentScoreText(uint score) => currentScoreText.text = score.ToString("D5");
    public void SetTimerText(string text) => timerText.text = text;

}
