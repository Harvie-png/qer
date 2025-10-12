using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManagerTEST : MonoBehaviour
{
    [Header("TEST UI Elements")]
    public Canvas testCanvas;
    public TMP_Text testText;
    public Button testButton;

    void Start()
    {
        Debug.Log("Test script loaded!");
    }
}