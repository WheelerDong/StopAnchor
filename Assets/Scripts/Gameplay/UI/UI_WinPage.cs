using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_WinPage : MonoBehaviour
{
    [SerializeField] private Button nextLevelButton;

    private void Awake()
    {
        nextLevelButton.onClick.AddListener(OnNextLevelButtonClick);
    }

    private void OnNextLevelButtonClick()
    {
        
    }
}
