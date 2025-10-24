using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScreenManager : MonoBehaviour
{
    public Button startButton;

    void Start()
    {
        startButton.onClick.AddListener(LoadMainMenu);
        Debug.Log("Стартовая заставка загружена");
    }

    void LoadMainMenu()
    {
        Debug.Log("Загружаем главное меню...");
        SceneManager.LoadScene("MenuScene"); 
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            LoadMainMenu();
        }
    }
}