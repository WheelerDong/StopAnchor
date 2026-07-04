using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MainPage : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button levelsButton;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        playButton.onClick.AddListener(OnPlayButtonClick);
        levelsButton.onClick.AddListener(onLevelsButtonClick);
        exitButton.onClick.AddListener(onExitButtonClick);
    }

    private void OnPlayButtonClick()
    {
        GameFlowManager.Instance.PlayCurrentLevel();
    }
    private void onLevelsButtonClick()
    {
        GameUIManager.Instance.ShowLevelsPage();
    }
    private void onExitButtonClick()
    {
        GameFlowManager.Instance.QuitGame();
    }
}
