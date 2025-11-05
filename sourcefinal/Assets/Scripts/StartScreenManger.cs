using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScreenManager : MonoBehaviour
{
    public Button startButton;

    void Start()
    {
        Debug.Log("=== START SCREEN MANAGER START ===");

        // Проверяем что кнопка существует
        if (startButton == null)
        {
            Debug.LogError("START BUTTON IS NULL!");
            return;
        }
        Debug.Log("Start button found: " + startButton.name);

        // Сброс устройств
        Debug.Log("Resetting input devices...");
        if (Keyboard.current != null)
        {
            InputSystem.ResetDevice(Keyboard.current);
            Debug.Log("Keyboard reset");
        }
        if (Mouse.current != null)
        {
            InputSystem.ResetDevice(Mouse.current);
            Debug.Log("Mouse reset");
        }

        // Подписываемся на кнопку
        startButton.onClick.AddListener(LoadMainMenu);
        Debug.Log("Listener added to button");

        Debug.Log("=== START SCREEN SETUP COMPLETE ===");
    }

    void Update()
    {
        // Проверяем ввод каждый кадр
        if (Time.frameCount < 5)
        {
            Debug.Log($"Frame {Time.frameCount} - skipping input");
            return;
        }

        if (Input.anyKeyDown)
        {
            Debug.Log($"Key pressed: {Input.inputString}");
            LoadMainMenu();
        }
    }

    void LoadMainMenu()
    {
        Debug.Log("=== LOAD MAIN MENU CALLED ===");
        SceneManager.LoadScene("MenuScene");
    }
}