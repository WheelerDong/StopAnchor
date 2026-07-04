using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Anchor : MonoBehaviour
{
    [SerializeField] private GameObject validGameObject;
    [SerializeField] private GameObject invalidGameObject;


    private void Awake()
    {
        SetActive(true);
    }
    
    public void SetActive(bool active)
    {
        validGameObject.SetActive(active);
        invalidGameObject.SetActive(!active);
    }
}
