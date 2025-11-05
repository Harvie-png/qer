using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // ДОБАВЬ эту строку

public class CutsceneManager : MonoBehaviour
{
    [Header("Cutscene UI")]
    public GameObject cutscenePanel;
    public TMP_Text dialogueText;
    public TMP_Text pressAnyKeyText;

    [Header("Menu Button")] // ДОБАВЬ этот раздел
    public Button menuButton;

    [Header("OLD Reference")]
    public Transform oldTransform;

    [Header("UI Offset")]
    public Vector3 panelOffset = new Vector3(2f, 2f, 0f);

    [Header("Dialogue")]
    public string[] dialogues = {
        "ОЛД: *системные голоса смешиваются* Наконец-то ты здесь...",
        "ОЛД: Все эти пасхалки были тестами... тестами твоей готовности.",
        "ОЛД: Но теперь пришло время настоящего испытания!",
        "ОЛД: Покажи, на что ты способен!"
    };

    private int currentDialogue = 0;
    private bool cutsceneActive = true;
    private float textBlinkTimer = 0f;
    private Camera mainCamera;
    private RectTransform panelRect;
    private bool canProceed = false;
    private PlayerController playerController;
    private BossOLD bossOLD;
    private PlayerCombat playerCombat;

    void Start()
    {
        mainCamera = Camera.main;

        // Находим все нужные компоненты заранее
        playerController = FindObjectOfType<PlayerController>();
        bossOLD = FindObjectOfType<BossOLD>();
        playerCombat = FindObjectOfType<PlayerCombat>();

        if (cutscenePanel != null)
        {
            panelRect = cutscenePanel.GetComponent<RectTransform>();
            cutscenePanel.SetActive(false);
        }

        // ДОБАВЬ инициализацию кнопки меню
        if (menuButton != null)
        {
            menuButton.onClick.AddListener(ReturnToMainMenu);
            menuButton.gameObject.SetActive(true); // Убедись что кнопка активна
            Debug.Log("Кнопка меню инициализирована");
        }
        else
        {
            Debug.LogError("Кнопка меню не назначена в инспекторе!");
        }

        Debug.Log("CutsceneManager инициализирован");
        Debug.Log($"Найдены: PlayerController={playerController != null}, BossOLD={bossOLD != null}, PlayerCombat={playerCombat != null}");

        // Запускаем кат-сцену с задержкой
        Invoke("StartCutscene", 0.5f);
    }

    void StartCutscene()
    {
        Debug.Log("=== НАЧАЛО КАТ-СЦЕНЫ ===");

        if (cutscenePanel != null)
            cutscenePanel.SetActive(true);

        // Отключаем управление и босса вместо Time.timeScale = 0
        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("PlayerController отключен");
        }

        if (bossOLD != null)
        {
            bossOLD.enabled = false;
            Debug.Log("BossOLD отключен");
        }

        if (playerCombat != null)
        {
            playerCombat.enabled = false;
            Debug.Log("PlayerCombat отключен");
        }

        // ДОБАВЬ: скрываем кнопку меню во время кат-сцены
        if (menuButton != null)
            menuButton.gameObject.SetActive(false);

        currentDialogue = 0;

        if (dialogueText != null)
            dialogueText.text = dialogues[currentDialogue];

        // Включаем подсказку через секунду
        Invoke("EnableProceed", 1f);

        Debug.Log("Первый диалог показан");
    }

    void EnableProceed()
    {
        canProceed = true;
        if (pressAnyKeyText != null)
            pressAnyKeyText.enabled = true;
        Debug.Log("canProceed = true - теперь можно нажимать E");
    }

    void Update()
    {
        if (!cutsceneActive) return;

        // Обновляем позицию панели относительно ОЛДа
        UpdatePanelPosition();

        // Мигание текста "Нажмите любую клавишу"
        if (canProceed)
        {
            textBlinkTimer += Time.deltaTime;
            if (textBlinkTimer >= 0.8f)
            {
                if (pressAnyKeyText != null)
                    pressAnyKeyText.enabled = !pressAnyKeyText.enabled;
                textBlinkTimer = 0f;
            }
        }

        // ОТЛАДКА: логируем нажатие E
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"E нажата! canProceed = {canProceed}, cutsceneActive = {cutsceneActive}");
        }

        // Проверка нажатия E
        if (canProceed && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Обработка нажатия E - переход к следующему диалогу");
            ShowNextDialogue();
        }
    }

    void UpdatePanelPosition()
    {
        if (oldTransform != null && cutscenePanel != null && mainCamera != null && panelRect != null)
        {
            Vector3 worldPosition = oldTransform.position + panelOffset;
            Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            panelRect.position = screenPosition;
        }
    }

    void ShowNextDialogue()
    {
        Debug.Log($"ShowNextDialogue вызван. Текущий диалог: {currentDialogue}");

        canProceed = false;
        if (pressAnyKeyText != null)
            pressAnyKeyText.enabled = false;

        currentDialogue++;

        if (currentDialogue < dialogues.Length)
        {
            if (dialogueText != null)
                dialogueText.text = dialogues[currentDialogue];

            Debug.Log($"Показываем диалог {currentDialogue}: {dialogues[currentDialogue]}");

            // Снова разрешаем переход через секунду
            Invoke("EnableProceed", 1f);
        }
        else
        {
            Debug.Log("Все диалоги завершены, завершаем кат-сцену");
            EndCutscene();
        }
    }

    void EndCutscene()
    {
        Debug.Log("=== ЗАВЕРШЕНИЕ КАТ-СЦЕНЫ ===");

        cutsceneActive = false;
        canProceed = false;

        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);

        // Включаем все обратно
        if (playerController != null)
        {
            playerController.enabled = true;
            Debug.Log("PlayerController включен");
        }

        if (bossOLD != null)
        {
            bossOLD.enabled = true;
            Debug.Log("BossOLD включен");
            bossOLD.StartBossFight();
        }
        else
        {
            Debug.LogError("BossOLD не найден!");
        }

        if (playerCombat != null)
        {
            playerCombat.enabled = true;
            Debug.Log("PlayerCombat включен");
        }

        // ДОБАВЬ: показываем кнопку меню после кат-сцены
        if (menuButton != null)
            menuButton.gameObject.SetActive(true);

        Debug.Log("Кат-сцена завершена, начинаем бой!");
    }

    // ДОБАВЬ метод для возврата в меню
    void ReturnToMainMenu()
    {
        Debug.Log("Возвращаемся в главное меню с уровня 10");
        SceneManager.LoadScene("MenuScene");
    }

    // Метод для принудительного завершения кат-сцены (на всякий случай)
    public void ForceEndCutscene()
    {
        if (cutsceneActive)
        {
            Debug.Log("Принудительное завершение кат-сцены");
            EndCutscene();
        }
    }

    public void RestartCutscene()
    {
        // Сбрасываем переменные
        currentDialogue = 0;
        cutsceneActive = true;
        canProceed = false;

        // Скрываем кнопку меню при рестарте
        if (menuButton != null)
            menuButton.gameObject.SetActive(false);

        // Запускаем кат-сцену заново
        Invoke("StartCutscene", 0.5f);
    }
}