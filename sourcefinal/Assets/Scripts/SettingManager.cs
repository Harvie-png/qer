using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using SQLite;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button[] musicButtons = new Button[5];
    public Button[] textureButtons = new Button[5];
    public Toggle developerToggle;
    public Button saveButton;
    public Button backButton;

    [Header("Selection Visuals")]
    public Color selectedColor = Color.green;
    public Color normalColor = Color.white;

    // Текущие настройки
    private int selectedMusicPackage = 1;
    private int selectedTexturePackage = 1;
    private bool developerMode = false;

    private SQLiteConnection db;

    void Start()
    {
        InitializeDatabase();
        LoadSettings();
        SetupUI();
        UpdateSelectionVisuals();
    }

    void InitializeDatabase()
    {
        try
        {
            db = new SQLiteConnection("escapeRoomBase.sqlite", SQLiteOpenFlags.ReadWrite);
            Debug.Log("SettingsManager: БД подключена");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка подключения БД: {e.Message}");
        }
    }

    void LoadSettings()
    {
        try
        {
            var settings = db.Table<Setting>().FirstOrDefault();
            if (settings != null)
            {
                developerMode = settings.debug_mode == 1;
                Debug.Log($"Загружены настройки: музыка={selectedMusicPackage}, текстуры={selectedTexturePackage}, режим разработчика={developerMode}");
            }
            else
            {
                Debug.Log("Настройки не найдены, используем значения по умолчанию");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка загрузки настроек: {e.Message}");
        }
    }

    void SetupUI()
    {
        // Подписываем кнопки музыки
        for (int i = 0; i < musicButtons.Length; i++)
        {
            int musicIndex = i + 1;
            musicButtons[i].onClick.AddListener(() => SelectMusicPackage(musicIndex));
        }

        // Подписываем кнопки текстур
        for (int i = 0; i < textureButtons.Length; i++)
        {
            int textureIndex = i + 1;
            textureButtons[i].onClick.AddListener(() => SelectTexturePackage(textureIndex));
        }

        // Подписываем toggle и кнопки
        if (developerToggle != null)
        {
            developerToggle.isOn = developerMode;
            developerToggle.onValueChanged.AddListener(OnDeveloperModeChanged);
        }

        saveButton.onClick.AddListener(SaveSettings);
        backButton.onClick.AddListener(ReturnToMenu);

        Debug.Log("UI настроек инициализирован");
    }

    void SelectMusicPackage(int packageId)
    {
        selectedMusicPackage = packageId;
        UpdateSelectionVisuals();
        Debug.Log($"Выбран пакет музыки: {packageId}");
    }

    void SelectTexturePackage(int packageId)
    {
        selectedTexturePackage = packageId;
        UpdateSelectionVisuals();
        Debug.Log($"Выбран пакет текстур: {packageId}");
    }

    void OnDeveloperModeChanged(bool isOn)
    {
        developerMode = isOn;
        Debug.Log($"Режим разработчика: {(isOn ? "включен" : "выключен")}");

        // Показываем предупреждение при включении
        if (isOn)
        {
            ShowDeveloperModeWarning();
        }
    }

    void ShowDeveloperModeWarning()
    {
        // Можно добавить всплывающее окно с предупреждением
        Debug.LogWarning("ВНИМАНИЕ: Режим разработчика включен! Все уровни будут доступны.");

        // Показываем сообщение в UI (если есть куда)
        if (GameObject.FindObjectOfType<GameManager>() != null)
        {
            GameObject.FindObjectOfType<GameManager>().ShowFeedback(
                "РЕЖИМ РАЗРАБОТЧИКА АКТИВЕН!\nВсе уровни разблокированы.",
                Color.yellow
            );
        }
    }

    void UpdateSelectionVisuals()
    {
        // Обновляем визуал кнопок музыки
        for (int i = 0; i < musicButtons.Length; i++)
        {
            int musicIndex = i + 1;
            Image buttonImage = musicButtons[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = (musicIndex == selectedMusicPackage) ? selectedColor : normalColor;
            }
        }

        // Обновляем визуал кнопок текстур
        for (int i = 0; i < textureButtons.Length; i++)
        {
            int textureIndex = i + 1;
            Image buttonImage = textureButtons[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = (textureIndex == selectedTexturePackage) ? selectedColor : normalColor;
            }
        }
    }

    void SaveSettings()
    {
        try
        {
            var settings = db.Table<Setting>().FirstOrDefault();
            if (settings == null) settings = new Setting();

            settings.debug_mode = developerMode ? 1 : 0;

            db.InsertOrReplace(settings);
            Debug.Log("Настройки сохранены");

            // ОБНОВЛЯЕМ РЕСУРСЫ ПОСЛЕ СОХРАНЕНИЯ
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.LoadCurrentResources();
            }

            ShowSaveConfirmation();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка сохранения: {e.Message}");
        }
    }

    void UnlockAllLevels()
    {
        try
        {
            long sessionId = MainMenuUIManager.CurrentSessionId;

            // Получаем все задачи из БД
            var allTasks = db.Table<Task>().ToList();

            // Добавляем прогресс для всех задач
            foreach (var task in allTasks)
            {
                // Проверяем, не добавлена ли уже эта задача
                var existingProgress = db.Table<TaskProgress>()
                    .FirstOrDefault(tp => tp.session_id == sessionId && tp.task_id == task.id);

                if (existingProgress == null)
                {
                    db.Insert(new TaskProgress()
                    {
                        session_id = sessionId,
                        task_id = task.id
                    });
                    Debug.Log($"Разблокирована задача: {task.id} (уровень {task.level_id})");
                }
            }

            Debug.Log($"Все уровни разблокированы для сессии {sessionId}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка разблокировки уровней: {e.Message}");
        }
    }

    void ShowSaveConfirmation()
    {
        TextMeshProUGUI saveButtonText = saveButton.GetComponentInChildren<TextMeshProUGUI>();
        if (saveButtonText != null)
        {
            string originalText = saveButtonText.text;
            saveButtonText.text = "СОХРАНЕНО!";
            Invoke(nameof(RestoreSaveButtonText), 2f);
        }
    }

    void RestoreSaveButtonText()
    {
        TextMeshProUGUI saveButtonText = saveButton.GetComponentInChildren<TextMeshProUGUI>();
        if (saveButtonText != null)
        {
            saveButtonText.text = "Сохранить";
        }
    }

    void ReturnToMenu()
    {
        Debug.Log("Возврат в главное меню");

        if (db != null)
            db.Close();

        SceneManager.LoadScene("MenuScene");
    }

    void OnDestroy()
    {
        if (db != null)
            db.Close();
    }

    // Классы БД
    [Table("setting")]
    public class Setting
    {
        [PrimaryKey]
        public long id { get; set; } = 1;
        public long debug_mode { get; set; } = 0;
    }

    [Table("task")]
    public class Task
    {
        [PrimaryKey] public long id { get; set; }
        public long level_id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string answer { get; set; }
        public byte[] picture { get; set; }
        public string hint { get; set; }
    }

    [Table("task_progress")]
    public class TaskProgress
    {
        public long session_id { get; set; }
        public long task_id { get; set; }
    }
}