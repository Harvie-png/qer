using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Database Connection")]
    private SQLiteConnection db;
    private long currentSessionId;

    [Header("Egg System")]
    public GameObject eggFoundPanel;
    public TMP_Text eggFoundText;
    private List<long> foundEggIds;

    [Header("Task Dialogue UI")]
    public GameObject taskPanel;
    public Button[] taskIcons = new Button[9];
    public TMP_Text taskNameText;
    public TMP_Text taskDescriptionText;
    public TMP_InputField answerInput;
    public Button checkButton;
    public Button backButton;
    public Button hintButton;
    public Button viewImageButton;
    public Image taskImage;
    public GameObject hintPanel;
    public TMP_Text hintText;
    public Button closeImageButton;

    [Header("Additional UI")]
    public GameObject feedbackPanel; // для уведомлений о правильном/неправильном ответе
    public TMP_Text feedbackText;

    [Header("Game Settings")]
    public int currentLevelId = 0;

    // Данные из БД
    private List<Task> allTasks;
    private List<long> completedTaskIds;
    private Task currentTask;

    [Header("Menu Button")]
    public Button menuButton;

    // Классы БД 
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

    [Table("egg")]
    public class Egg
    {
        [PrimaryKey] public long id { get; set; }
        public long level_id { get; set; }
        public string name { get; set; }
    }

    [Table("egg_progress")]
    public class EggProgress
    {
        public long session_id { get; set; }
        public long egg_id { get; set; }
    }

    void Start()
    {
        InitializeDatabase();
        LoadGameData(); // Здесь определяется currentLevelId из БД

        // Проверяем не находимся ли мы уже на правильной сцене
        string currentScene = SceneManager.GetActiveScene().name;
        string targetScene = "Level" + currentLevelId;

        // Если мы НЕ на той сцене, куда должны быть - загружаем правильную сцену
        if (currentScene != targetScene)
        {
            Debug.Log($"Переносим игрока с {currentScene} на {targetScene} (последний пройденный уровень: {currentLevelId})");
            SceneManager.LoadScene(targetScene);
            return; // Прерываем выполнение, т.к. сцена перезагружается
        }

        // Если мы уже на правильной сцене - продолжаем инициализацию
        SetupUI();

        // Автоопределение уровня из имени сцены (на всякий случай)
        if (currentScene.StartsWith("Level"))
        {
            string levelStr = currentScene.Replace("Level", "");
            if (int.TryParse(levelStr, out int sceneLevel))
            {
                currentLevelId = sceneLevel;
                SetCurrentTask(currentLevelId);
                ShowFeedback($"Уровень {currentLevelId} загружен!", Color.blue);
            }
        }

        // Скрываем UI при старте
        eggFoundPanel.SetActive(false);
        taskPanel.SetActive(false);
        if (taskImage != null) taskImage.gameObject.SetActive(false);
        if (hintPanel != null) hintPanel.SetActive(false);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        if (closeImageButton != null) closeImageButton.gameObject.SetActive(false);

        Debug.Log($"Игра запущена на уровне {currentLevelId}, сцена: {currentScene}");
    }

    void InitializeDatabase() //Подключается к БД, если она крашится - выводит дебаг
    {
        try
        {
            db = new SQLiteConnection("escapeRoomBase.sqlite", SQLiteOpenFlags.ReadWrite);
            currentSessionId = MainMenuUIManager.CurrentSessionId;
            Debug.Log($"БД подключена. Сессия: {currentSessionId}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка подключения БД: {e.Message}");
        }
    }

    void LoadGameData()
    {
        // Загружаем все задачи
        allTasks = db.Table<Task>().OrderBy(t => t.level_id).ToList();
        Debug.Log($"Загружено задач: {allTasks.Count}");

        // Загружаем выполненные задачи
        completedTaskIds = db.Table<TaskProgress>()
            .Where(tp => tp.session_id == currentSessionId)
            .Select(tp => tp.task_id)
            .ToList();
        Debug.Log($"Выполнено задач: {completedTaskIds.Count}");

        LoadCurrentLevel();
        LoadEggData();
        SetCurrentTask(currentLevelId);
    }

    void LoadCurrentLevel()
    {
        try
        {
            // Находим ВСЕ пройденные задачи текущей сессии
            var completedTasks = db.Table<TaskProgress>()
                .Where(tp => tp.session_id == currentSessionId)
                .Select(tp => tp.task_id)
                .ToList();

            if (completedTasks.Count > 0)
            {
                // Находим задачу с МАКСИМАЛЬНЫМ level_id среди пройденных
                var maxLevelTask = allTasks
                    .Where(t => completedTasks.Contains(t.id))
                    .OrderByDescending(t => t.level_id)
                    .FirstOrDefault();

                if (maxLevelTask != null)
                {
                    currentLevelId = (int)maxLevelTask.level_id + 1;
                    if (currentLevelId >= allTasks.Count)
                    {
                        currentLevelId = allTasks.Count - 1; // Остаемся на последнем уровне
                        Debug.Log($"Достигнут максимальный уровень: {currentLevelId}");
                    }
                    Debug.Log($"Загружен последний пройденный уровень: {currentLevelId}");
                }
                else
                {
                    currentLevelId = 0;
                }
            }
            else
            {
                currentLevelId = 0; // Ни одной задачи не пройдено
                Debug.Log("Нет пройденных задач, начинаем с уровня 0");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка загрузки уровня: {e.Message}");
            currentLevelId = 0;
        }
    }

    void SetupUI()
    {
        // Настраиваем кнопки основной панели
        checkButton.onClick.AddListener(CheckAnswer);
        backButton.onClick.AddListener(HideTaskDialogue);
        hintButton.onClick.AddListener(ShowHint);
        viewImageButton.onClick.AddListener(ViewImage);
        menuButton.onClick.AddListener(ReturnToMainMenu);

        // Настраиваем иконки задач
        for (int i = 0; i < taskIcons.Length; i++)
        {
            int taskIndex = i;
            taskIcons[i].onClick.AddListener(() => SelectTask(taskIndex));
        }

        // Настраиваем кнопки закрытия для HintPanel
        if (hintPanel != null)
        {
            Button[] hintButtons = hintPanel.GetComponentsInChildren<Button>();
            foreach (Button btn in hintButtons)
            {
                if (btn.name.Contains("Close") || btn.name.Contains("Back"))
                {
                    btn.onClick.AddListener(CloseHint);
                    Debug.Log("Найдена кнопка закрытия подсказки: " + btn.name);
                    break;
                }
            }
        }


        // Настраиваем кнопки закрытия для EggFoundPanel
        if (eggFoundPanel != null)
        {
            Button[] eggButtons = eggFoundPanel.GetComponentsInChildren<Button>();
            foreach (Button btn in eggButtons)
            {
                if (btn.name.Contains("Close") || btn.name.Contains("Back") || btn.name.Contains("OK"))
                {
                    btn.onClick.AddListener(CloseEggPanel);
                    Debug.Log("Найдена кнопка закрытия пасхалки: " + btn.name);
                    break;
                }
            }
        }

        // Настраиваем кнопки закрытия для FeedbackPanel (если есть)
        if (feedbackPanel != null)
        {
            Button[] feedbackButtons = feedbackPanel.GetComponentsInChildren<Button>();
            foreach (Button btn in feedbackButtons)
            {
                if (btn.name.Contains("Close") || btn.name.Contains("OK"))
                {
                    btn.onClick.AddListener(HideFeedback);
                    Debug.Log("Найдена кнопка закрытия фидбэка: " + btn.name);
                    break;
                }
            }
        }
        if (taskImage != null)
        {
            Button closeImageBtn = taskImage.GetComponentInChildren<Button>();
            if (closeImageBtn != null)
            {
                closeImageBtn.onClick.AddListener(CloseImage);
                Debug.Log("Кнопка закрытия изображения подключена");
            }
        }

        if (closeImageButton != null)
        {
            closeImageButton.onClick.AddListener(CloseImage);
            Debug.Log("Кнопка закрытия изображения подключена");
        }
        else
        {
            Debug.LogWarning("Кнопка закрытия изображения не найдена!");
        }

        // Обновляем цвета иконок
        UpdateTaskIconsColors();

        Debug.Log("Настройка UI завершена");
    }

    void SetCurrentTask(int levelId) //Поиск заадчи  по ИД
    {
        currentTask = allTasks.FirstOrDefault(t => t.level_id == levelId);
        if (currentTask != null)
        {
            currentLevelId = levelId;
            UpdateTaskUI();
            Debug.Log($"Текущая задача: уровень {levelId}");
        }
    }

    void UpdateTaskUI() //Отрисовка UI с условиями
    {
        if (currentTask != null)
        {
            taskNameText.text = currentTask.name;
            taskDescriptionText.text = currentTask.description;
            answerInput.text = "";
            hintText.text = currentTask.hint;

            // Показываем/скрываем кнопку просмотра изображения
            viewImageButton.gameObject.SetActive(currentTask.picture != null && currentTask.picture.Length > 0);

            // Обновляем цвета иконок
            UpdateTaskIconsColors();
        }
    }

    void UpdateTaskIconsColors() //Иконки
    {
        for (int i = 0; i < taskIcons.Length; i++)
        {
            if (i < allTasks.Count)
            {
                var task = allTasks[i];
                bool isCompleted = completedTaskIds.Contains(task.id);
                bool isCurrent = task.level_id == currentLevelId;

                Image iconImage = taskIcons[i].GetComponent<Image>();

                if (isCurrent)
                {
                    iconImage.color = Color.yellow; // текущая задача
                }
                else if (isCompleted)
                {
                    iconImage.color = Color.green; // выполнена
                }
                else
                {
                    iconImage.color = new Color(0, 0.7f, 1f); // голубая - не выполнена
                }
            }
        }
    }

    void SelectTask(int taskIndex)
    {
        if (taskIndex < allTasks.Count)
        {
            SetCurrentTask((int)allTasks[taskIndex].level_id);
        }
    }

    // === ОСНОВНЫЕ МЕТОДЫ ===

    public void ShowTaskDialogue()
    {
        taskPanel.SetActive(true);
        UpdateTaskUI();
        Debug.Log("Открыто диалоговое окно задач");
    }

    void HideTaskDialogue()
    {
        taskPanel.SetActive(false);
        Debug.Log("Диалоговое окно закрыто");
    }

    void CheckAnswer()
    {
        string userAnswer = answerInput.text.Trim().ToLower();
        string correctAnswer = currentTask.answer.Trim().ToLower();

        if (userAnswer == correctAnswer)
        {
            Debug.Log("Правильный ответ!");
            ShowFeedback("Правильно! Задание выполнено.", Color.green);

            // Сохраняем прогресс
            SaveTaskProgress();

            // Обновляем UI
            UpdateTaskIconsColors();
            answerInput.text = "";
        }
        else
        {
            Debug.Log("Неправильный ответ! Ожидается: " + correctAnswer);
            // Можно добавить визуальный фидбэк ошибки
            ShowFeedback("Неправильно! Попробуйте еще раз.", Color.red);
        }
    }

    public void ShowFeedback(string message, Color color)
    {
        // Если feedbackPanel еще не создан, просто выводим в консоль
        if (feedbackPanel == null)
        {
            Debug.Log("Feedback: " + message);
            return;
        }

        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }

        feedbackPanel.SetActive(true);

        // Автоскрытие через 3 секунды
        CancelInvoke("HideFeedback"); // Отменяем предыдущие вызовы
        Invoke("HideFeedback", 3f);
    }

    void HideFeedback()
    {
        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);
    }
    

    void SaveTaskProgress()
    {
        try
        {
            // Проверяем, не выполнена ли уже задача
            if (!completedTaskIds.Contains(currentTask.id))
            {
                db.Insert(new TaskProgress()
                {
                    session_id = currentSessionId,
                    task_id = currentTask.id
                });

                completedTaskIds.Add(currentTask.id);
                Debug.Log("Прогресс сохранен в БД");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка сохранения прогресса: {e.Message}");
        }
    }

    void ShowHint()
    {
        if (hintPanel != null)
        {
            hintPanel.SetActive(true);
        }
    }

    void ViewImage()
    {
        if (currentTask.picture != null && currentTask.picture.Length > 0)
        {
            try
            {
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(currentTask.picture);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                taskImage.sprite = sprite;
                taskImage.gameObject.SetActive(true);
                if (closeImageButton != null) closeImageButton.gameObject.SetActive(true);
                Debug.Log("Изображение задачи загружено");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка загрузки изображения: {e.Message}");
            }
        }
    }

    // Метод для закрытия изображения (добавить на кнопку закрытия)
    public void CloseImage()
    {
        if (taskImage != null) taskImage.gameObject.SetActive(false);
        if (closeImageButton != null) closeImageButton.gameObject.SetActive(false);
    }

    // Метод для закрытия подсказки (добавить на кнопку закрытия)
    public void CloseHint()
    {
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (db != null)
            db.Close();
    }
    public void ShowEggFound(string eggText)
    {
        if (eggFoundPanel != null && eggFoundText != null)
        {
            eggFoundText.text = eggText;
            eggFoundPanel.SetActive(true);
            Debug.Log("Пасхалка найдена: " + eggText);
        }
    }

    public void CloseEggPanel()
    {
        if (eggFoundPanel != null)
            eggFoundPanel.SetActive(false);
    }

    public bool IsLevelCompleted(int levelId)
    {
        if (levelId == -1)
        {
            Debug.LogError($"Кто-то запрашивает проверку уровня -1! StackTrace: {System.Environment.StackTrace}");
        }
        // ЗАЩИТА: если данные еще не загружены
        if (allTasks == null || completedTaskIds == null)
        {
            Debug.LogWarning("Данные еще не загружены из БД! Проверка отложена.");
            return false;
        }

        // Находим задачу для этого уровня
        var levelTask = allTasks.FirstOrDefault(t => t.level_id == levelId);
        if (levelTask != null)
        {
            bool isCompleted = completedTaskIds.Contains(levelTask.id);
            Debug.Log($"Уровень {levelId} (задача {levelTask.id}) пройден: {isCompleted}");
            return isCompleted;
        }

        Debug.LogWarning($"Задача для уровня {levelId} не найдена");
        return false;
    }

    public bool IsEggCollected(long eggId)
    {
        // Двойная проверка что БД готова
        if (db == null || foundEggIds == null)
        {
            Debug.LogWarning($"Egg {eggId}: Данные еще не загружены (db: {db != null}, foundEggIds: {foundEggIds != null})");
            return false;
        }

        bool collected = foundEggIds.Contains(eggId);
        Debug.Log($"Egg {eggId}: Проверка в БД - {collected} (Всего найдено: {foundEggIds.Count})");
        return collected;
    }

    void LoadEggData()
    {
        try
        {
            foundEggIds = db.Table<EggProgress>()
                .Where(ep => ep.session_id == currentSessionId)
                .Select(ep => ep.egg_id)
                .ToList();
            Debug.Log($"Загружено пасхалок: {foundEggIds.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка загрузки пасхалок: {e.Message}");
            foundEggIds = new List<long>();
        }
    }
    void ReturnToMainMenu()
    {
        Debug.Log("Возвращаемся в главное меню");

        // Сохраняем прогресс перед выходом
        if (db != null)
            db.Close();

        SceneManager.LoadScene("MenuScene"); // Имя твоей сцены меню
    }
    public List<long> GetFoundEggIds()
    {
        return foundEggIds ?? new List<long>();
    }
    public string SaveEggProgress(long eggId)
    {
        Debug.Log($"Egg {eggId}: Attempting to save to DB...");
        try
        {
            if (!foundEggIds.Contains(eggId))
            {
                // Получаем описание пасхалки из таблицы egg
                var eggData = db.Table<Egg>().FirstOrDefault(e => e.id == eggId);
                string eggDescription = eggData?.name ?? $"Пасхалка {eggId} найдена!";

                Debug.Log($"Egg {eggId}: Description from DB - {eggDescription}");

                // Сохраняем прогресс в egg_progress
                db.Insert(new EggProgress()
                {
                    session_id = currentSessionId,
                    egg_id = eggId
                });

                foundEggIds.Add(eggId);
                Debug.Log($"Пасхалка {eggId} сохранена в БД");

                // Возвращаем описание из БД вместо стандартного текста
                return $"{eggDescription}\n\nВсего собрано: {foundEggIds.Count}/4";
            }

            Debug.Log($"Egg {eggId}: Already in DB, skipping save");
            return "Эта пасхалка уже собрана!";
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка сохранения пасхалки: {e.Message}");
            return "Ошибка сохранения пасхалки";
        }
    }

    public void LoadLevel(int levelId)
    {
        currentLevelId = levelId;
        Debug.Log($"Загружаем уровень {levelId}");
        string sceneName = "Level" + levelId;
        SceneManager.LoadScene(sceneName);
    }
}