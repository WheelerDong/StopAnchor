using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_LevelElement : MonoBehaviour
{
    private Button button;
    public int levelIndex = 0;

    
    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButonClick);
    }

    private void OnButonClick()
    {
        GameFlowManager.Instance.PlayLevel(levelIndex);
    }
}
