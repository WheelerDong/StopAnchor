using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_LevelsPage : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private UI_LevelManager levelManager;

    private void Awake()
    {
        backButton.onClick.AddListener((() =>
        {
            GameUIManager.Instance.ShowMainPage();
        }));
        levelManager.InitLevelButtons();
    }

    private void Start()
    {
        //LevelLibrary lib = GameFlowManager.Instance.LevelLibrary;
        
    }
}
