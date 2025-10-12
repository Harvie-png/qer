using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneManager : MonoBehaviour
{
    [Header("UI Elements - перетащите из иерархии")]
    public Canvas mainCanvas;
    public TMP_Text levelNameText;
    public TMP_Text taskDescriptionText;
    public TMP_InputField answerInput;
    public Button submitButton;
    public Button hintButton;
    public Button menuButton;
    public Image taskImage;
    public Slider progressSlider;
    public TMP_Text progressText;

    [Header("Utility Panels - перетащите из иерархии")]
    public GameObject hintPanel;
    public GameObject successPanel;
    public GameObject eggFoundPanel;
    public TMP_Text hintText;
    public TMP_Text eggFoundText;

    [Header("Settings")]
    public float successPanelDisplayTime = 2f;
    public float eggPanelDisplayTime = 3f;

    // Приватные переменные для игры
    private SQLiteConnection db;
    private List<Task> currentLevelTasks = new List<Task>();
    private List<Egg> currentLevelEggs = new List<Egg>();
    private int currentTaskIndex = 0;
    private int currentLevelId = 0;
    private long sessionId;

    // Классы для БД
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

    [Table("egg")]
    public class Egg
    {
        [PrimaryKey] public long id { get; set; }
        public long level_id { get; set; }
        public string name { get; set; }
    }

    [Table("task_progress")]
    public class TaskProgress
    {
        public long session_id { get; set; }
        public long task_id { get; set; }
    }

    [Table("egg_progress")]
    public class EggProgress
    {
        public long session_id { get; set; }
        public long egg_id { get; set; }
    }

    void Start()
    {
        Debug.Log("GameSceneManager начал работу");

        // Получаем ID сессии из главного меню
        sessionId = MainMenuUIManager.CurrentSessionId;
        Debug.Log($"Загружена сессия: {sessionId}");

        // Подключение к БД
        string dbPath = Path.Combine(Application.persistentDataPath, "escapeRoomBase.sqlite");
        db = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite);
        Debug.Log("База данных подключена");

        // Начинаем с уровня 0
        LoadLevel(0);

        // Подписка на кнопки
        submitButton.onClick.AddListener(CheckAnswer);
        hintButton.onClick.AddListener(() => ShowHint(true));
        menuButton.onClick.AddListener(ReturnToMenu);

        // Находим кнопку закрытия подсказки
        Button closeHintButton = hintPanel.GetComponentInChildren<Button>();
        if (closeHintButton != null)
        {
            closeHintButton.onClick.AddListener(() => ShowHint(false));
        }

        // Скрываем вспомогательные панели
        if (hintPanel != null) hintPanel.SetActive(false);
        if (successPanel != null) successPanel.SetActive(false);
        if (eggFoundPanel != null) eggFoundPanel.SetActive(false);

        Debug.Log("GameSceneManager инициализирован");
    }

    void LoadLevel(int levelId)
    {
        currentLevelId = levelId;
        currentTaskIndex = 0;

        Debug.Log($"Загружаем уровень {levelId}");

        // Загрузка задач уровня
        try
        {
            currentLevelTasks = db.Query<Task>($"SELECT * FROM task WHERE level_id = {levelId}").ToList();
            Debug.Log($"Найдено задач: {currentLevelTasks.Count} для уровня {levelId}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка загрузки задач: {e.Message}");
            currentLevelTasks = new List<Task>();
        }

        // Загрузка пасхалок уровня
        try
        {
            currentLevelEggs = db.Query<Egg>($"SELECT * FROM egg WHERE level_id = {levelId}").ToList();
            Debug.Log($"Найдено пасхалок: {currentLevelEggs.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка загрузки пасхалок: {e.Message}");
            currentLevelEggs = new List<Egg>();
        }

        // Проверка прогресса - какие задачи уже выполнены
        List<TaskProgress> completedTasks = new List<TaskProgress>();
        try
        {
            completedTasks = db.Query<TaskProgress>($"SELECT * FROM task_progress WHERE session_id = {sessionId}").ToList();
            Debug.Log($"Выполнено задач в сессии: {completedTasks.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка загрузки прогресса: {e.Message}");
        }

        // Пропускаем уже выполненные задачи
        foreach (var completedTask in completedTasks)
        {
            var task = currentLevelTasks.FirstOrDefault(t => t.id == completedTask.task_id);
            if (task != null)
            {
                currentTaskIndex++;
                Debug.Log($"Задача {task.id} уже выполнена, пропускаем");
            }
        }

        if (currentTaskIndex < currentLevelTasks.Count)
        {
            DisplayCurrentTask();
        }
        else if (currentLevelTasks.Count > 0)
        {
            // Все задачи уровня выполнены
            Debug.Log($"Все задачи уровня {levelId} выполнены");
            CompleteLevel();
        }
        else
        {
            // Нет задач для этого уровня
            Debug.Log($"Нет задач для уровня {levelId}");
            CompleteLevel();
        }

        UpdateProgress();
    }

    void DisplayCurrentTask()
    {
        if (currentTaskIndex >= currentLevelTasks.Count)
        {
            Debug.LogError("Индекс задачи вне диапазона");
            return;
        }

        var currentTask = currentLevelTasks[currentTaskIndex];
        Debug.Log($"Отображаем задачу {currentTask.id}: {currentTask.name}");

        levelNameText.text = $"Уровень {currentLevelId} - Задача {currentTaskIndex + 1}";
        taskDescriptionText.text = currentTask.description;

        if (hintText != null)
            hintText.text = currentTask.hint;

        // Загрузка изображения если есть
        if (currentTask.picture != null && currentTask.picture.Length > 0)
        {
            Debug.Log("Загружаем изображение задачи");
            try
            {
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(currentTask.picture);
                taskImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                taskImage.gameObject.SetActive(true);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка загрузки изображения: {e.Message}");
                taskImage.gameObject.SetActive(false);
            }
        }
        else
        {
            taskImage.gameObject.SetActive(false);
        }

        answerInput.text = "";
        // Сбрасываем placeholder
        var placeholder = answerInput.placeholder as TMP_Text;
        if (placeholder != null)
        {
            placeholder.text = "Введите ваш ответ...";
        }
    }

    void CheckAnswer()
    {
        if (currentTaskIndex >= currentLevelTasks.Count) return;

        var currentTask = currentLevelTasks[currentTaskIndex];
        string userAnswer = answerInput.text.Trim().ToLower();
        string correctAnswer = currentTask.answer.Trim().ToLower();

        Debug.Log($"Проверка ответа: '{userAnswer}' vs '{correctAnswer}'");

        if (userAnswer == correctAnswer)
        {
            Debug.Log("Ответ правильный!");

            // Сохраняем прогресс задачи
            try
            {
                db.Execute("INSERT OR IGNORE INTO task_progress (session_id, task_id) VALUES (?, ?)",
                          sessionId, currentTask.id);
                Debug.Log("Прогресс сохранен в БД");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка сохранения прогресса: {e.Message}");
            }

            // Проверка на пасхалку
            CheckForEgg();

            ShowSuccessPanel();
        }
        else
        {
            Debug.Log("Неправильный ответ");
            ShowError();
        }
    }

    void CheckForEgg()
    {
        var egg = currentLevelEggs.FirstOrDefault();
        if (egg != null)
        {
            Debug.Log($"Проверяем пасхалку для уровня {currentLevelId}");

            // Проверяем, не найдена ли уже пасхалка
            var existingEggProgress = new List<EggProgress>();
            try
            {
                existingEggProgress = db.Query<EggProgress>(
                    $"SELECT * FROM egg_progress WHERE session_id = {sessionId} AND egg_id = {egg.id}").ToList();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка проверки пасхалки: {e.Message}");
            }

            if (existingEggProgress.Count == 0)
            {
                // Найдена новая пасхалка
                Debug.Log($"Найдена новая пасхалка: {egg.name}");
                try
                {
                    db.Execute("INSERT INTO egg_progress (session_id, egg_id) VALUES (?, ?)",
                              sessionId, egg.id);
                    ShowEggFoundPanel(egg.name);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Ошибка сохранения пасхалки: {e.Message}");
                }
            }
        }
    }

    void ShowSuccessPanel()
    {
        if (successPanel != null)
        {
            successPanel.transform.SetAsLastSibling();
            successPanel.SetActive(true);
            StartCoroutine(HideSuccessPanel());
        }
    }

    IEnumerator HideSuccessPanel()
    {
        yield return new WaitForSeconds(successPanelDisplayTime);
        if (successPanel != null)
            successPanel.SetActive(false);

        currentTaskIndex++;
        if (currentTaskIndex < currentLevelTasks.Count)
        {
            DisplayCurrentTask();
        }
        else
        {
            CompleteLevel();
        }

        UpdateProgress();
    }

    void ShowEggFoundPanel(string eggText)
    {
        if (eggFoundPanel != null && eggFoundText != null)
        {
            eggFoundText.text = eggText;
            eggFoundPanel.transform.SetAsLastSibling();
            eggFoundPanel.SetActive(true);
            StartCoroutine(HideEggFoundPanel());
        }
    }

    IEnumerator HideEggFoundPanel()
    {
        yield return new WaitForSeconds(eggPanelDisplayTime);
        if (eggFoundPanel != null)
            eggFoundPanel.SetActive(false);
    }

    void ShowHint(bool show)
    {
        if (hintPanel != null)
        {
            hintPanel.SetActive(show);
            if (show)
            {
                hintPanel.transform.SetAsLastSibling();
            }
        }
    }

    void ShowError()
    {
        // Визуальная обратная связь для ошибки
        var placeholder = answerInput.placeholder as TMP_Text;
        if (placeholder != null)
        {
            placeholder.text = "Неправильно! Попробуйте еще раз...";
        }

        // Можно добавить анимацию тряски
        StartCoroutine(ShakeInputField());
    }

    IEnumerator ShakeInputField()
    {
        Vector3 originalPos = answerInput.transform.position;
        float elapsed = 0f;

        while (elapsed < 0.5f)
        {
            float x = originalPos.x + Random.Range(-5f, 5f);
            float y = originalPos.y + Random.Range(-2f, 2f);
            answerInput.transform.position = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        answerInput.transform.position = originalPos;
    }

    void CompleteLevel()
    {
        Debug.Log($"Завершаем уровень {currentLevelId}");

        // Переход к следующему уровню
        int nextLevelId = currentLevelId + 1;

        // Проверяем существование следующего уровня
        var nextLevelTasks = new List<Task>();
        try
        {
            nextLevelTasks = db.Query<Task>($"SELECT * FROM task WHERE level_id = {nextLevelId}").ToList();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка проверки следующего уровня: {e.Message}");
        }

        if (nextLevelTasks.Count > 0)
        {
            Debug.Log($"Переходим на уровень {nextLevelId}");
            LoadLevel(nextLevelId);
        }
        else
        {
            // Игра завершена
            Debug.Log("Игра завершена! Все уровни пройдены.");
            ShowGameCompleted();
        }
    }

    void ShowGameCompleted()
    {
        levelNameText.text = "🎉 ПОБЕДА! 🎉";
        taskDescriptionText.text = "Поздравляем! Вы успешно прошли все уровни и сбежали из лаборатории!\n\nВаши знания и навыки помогли преодолеть все испытания.";
        taskImage.gameObject.SetActive(false);
        answerInput.gameObject.SetActive(false);
        submitButton.gameObject.SetActive(false);
        hintButton.gameObject.SetActive(false);

        progressSlider.value = 1f;
        progressText.text = "Завершено!";
    }

    void UpdateProgress()
    {
        if (currentLevelTasks.Count > 0)
        {
            float progress = (float)currentTaskIndex / currentLevelTasks.Count;
            progressSlider.value = progress;
            progressText.text = $"{currentTaskIndex}/{currentLevelTasks.Count}";
            Debug.Log($"Прогресс обновлен: {currentTaskIndex}/{currentLevelTasks.Count}");
        }
        else
        {
            progressSlider.value = 0f;
            progressText.text = "0/0";
        }
    }

    void ReturnToMenu()
    {
        Debug.Log("Возврат в главное меню");
        if (db != null)
            db.Close();
        SceneManager.LoadScene("MainMenu");
    }

    void OnDestroy()
    {
        if (db != null)
        {
            db.Close();
            Debug.Log("База данных закрыта");
        }
    }
}