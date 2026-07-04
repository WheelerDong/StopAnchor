using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Setting : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        resumeButton.onClick.AddListener(OnResumeButtonClick);
        tutorialButton.onClick.AddListener(OnTutorialButtonClick);
        quitButton.onClick.AddListener(OnQuitButtonClick);
    }

    private void OnResumeButtonClick()
    {
        GameplayManager.Instance.Resume();
        gameObject.SetActive(false);
    }
    private void OnTutorialButtonClick()
    {
        
    }
    private void OnQuitButtonClick()
    {
        GameFlowManager.Instance.GoIntoLobby();
    }
}
