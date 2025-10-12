using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    // Заголовок
    public TMP_Text selectedSessionText;

    // Массивы для 3 сессий
    public Button[] btnSelect = new Button[3];      // Кнопки "Выбрать" слева
    public GameObject[] sessionTiles = new GameObject[3]; // Плитки (Panel)
    public TMP_Text[] sessionInfoTexts = new TMP_Text[3]; // Текст внутри плиток
    public Button[] btnClear = new Button[3];       // Кнопки "Очистить" справа

    // Глобальные кнопки
    public Button btnSettingsSelected;
    public Button btnStartSelected;

    // Для хранения данных сессий
    private List<Session> sessions = new List<Session>();
    private int selectedSessionIndex = -1;

    // Для передачи в GameScene
    public static long CurrentSessionId { get; private set; }

    // Переменные
    private static SQLiteConnection db;
    private static long sessionId = 0;


    void LoadSessionData()
    {
        sessions = db.Table<Session>().OrderBy(s => s.id).Take(3).ToList();
        for (int i=0; i<sessions.Count; i++)
        {
            Debug.Log("Session " + sessions[i].id + " date " + sessions[i].last_date);
        }
    }

    void RefreshUI()
    {
        // Заголовок
        if (selectedSessionIndex >= 0 && selectedSessionIndex < sessions.Count)
            selectedSessionText.text = $"Выбрана сессия: {sessions[selectedSessionIndex].id}";
        else
            selectedSessionText.text = "Сессия не выбрана";

        // Плитки
        for (int i = 0; i < 3; i++)
        {
            var s = sessions[i];
            string dateStr = s.last_date != null
                ? DateTime.Parse(s.last_date).ToString("dd.MM.yyyy HH:mm")
                : "Пусто";
            sessionInfoTexts[i].text = $"Сессия {s.id}: {dateStr}";

            // Подсветка выбранной плитки (опционально)
            Color tileColor = (i == selectedSessionIndex) ? new Color(0.3f, 0.3f, 0.5f) : new Color(0.2f, 0.2f, 0.2f);
            sessionTiles[i].GetComponent<Image>().color = tileColor;
        }

        // Активность кнопок
        bool hasSelection = selectedSessionIndex >= 0;
        btnSettingsSelected.interactable = hasSelection;
        btnStartSelected.interactable = hasSelection;
    }

    void OnSelectSession(int index)
    {
        selectedSessionIndex = index;
        RefreshUI();
    }

    void OnClearSession(int index)
    {
        long sessionId = sessions[index].id;
        db.Execute("UPDATE session SET last_date = NULL WHERE id = ?", sessionId);
        db.Execute("DELETE FROM egg_progress WHERE session_id = ?", sessionId);
        db.Execute("DELETE FROM task_progress WHERE session_id = ?", sessionId);

        LoadSessionData();
        RefreshUI();
    }

    void OnSettingsClicked()
    {
        if (selectedSessionIndex >= 0)
        {
            long sessionId = sessions[selectedSessionIndex].id;
            Debug.Log($"Нажата кнопка настроек для сессии {sessionId}");
            // Позже: SceneManager.LoadScene("SettingsScene");
        }
    }

    void OnStartSelected()
    {
        if (selectedSessionIndex < 0) return;

        long sessionId = sessions[selectedSessionIndex].id;
        string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        db.Update(new Session()
         {
        id = sessionId,
        last_date = currentDate
             });
       // db.Execute("UPDATE session SET last_date = ? WHERE id = ?", currentDate, sessionId);

        Debug.Log($"Запуск сессии {sessionId}");
        CurrentSessionId = sessionId;
        SceneManager.LoadScene("GameScene");
    }

    // Метод сцены менюшки
    void Start()
    {
        // Копируем БД из StreamingAssets в persistentDataPath (только для записи)
        string sourcePath = Path.Combine(Application.streamingAssetsPath, "escapeRoomBase.sqlite");
        string dbPath = Path.Combine(Application.persistentDataPath, "escapeRoomBase.sqlite");

        if (!File.Exists(dbPath))
        {
            File.Copy(sourcePath, dbPath, true);
            Debug.Log("База данных скопирована в persistentDataPath");
        }

        db = new SQLiteConnection("escapeRoomBase.sqlite", SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

        // Инициализация БД
        InitDbIfNeeded();
        Debug.Log("persistentDataPath: " + Application.persistentDataPath);
        // Загрузка и отображение сессий
        LoadSessionData();
        RefreshUI();

        // Подписка на кнопки
        for (int i = 0; i < 3; i++)
        {
            int index = i;
            btnSelect[i].onClick.AddListener(() => OnSelectSession(index));
            btnClear[i].onClick.AddListener(() => OnClearSession(index));
        }

        btnSettingsSelected.onClick.AddListener(OnSettingsClicked);
        btnStartSelected.onClick.AddListener(OnStartSelected);
    }

    // Клики


    private void LoadGame()
    {
        Debug.Log("Игра загружена! Скоро перейдем на игровую сцену...");
        // Тут игра сцены из загрузок
    }

    // Таблицы БД
    [Table("level")]
    public class Level
    {
        [PrimaryKey]
        public long id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    [Table("egg")]
    public class Egg
    {
        [PrimaryKey]
        public long id { get; set; }
        public long level_id { get; set; }
        public string name { get; set; }
    }

    [Table("session")]
    public class Session
    {
        [PrimaryKey]
        public long id { get; set; }
        public string last_date { get; set; }
    }

    [Table("task")]
    public class Task
    {
        [PrimaryKey]
        public long id { get; set; }
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

    [Table("egg_progress")]
    public class EggProgress
    {
        public long session_id { get; set; }
        public long egg_id { get; set; }
    }

    [Table("setting")]
    public class Setting
    {
        [PrimaryKey]
        public string music_file { get; set; }
        public string texture_file { get; set; }
        public long debug_mode { get; set; }
    }

    // Инициализация БД
    void InitDbIfNeeded()
    {
        bool IsNeedToUpdate = false;
        try
        {
            var data = db.Table<Level>();
            Debug.Log($"Прочитано {data.Count()} записей из таблицы БД");
            if (data.Count() == 0)
            {
                IsNeedToUpdate = true;
            }
        }
        catch (SQLiteException ex)
        {
            IsNeedToUpdate = true;
        }
        if (IsNeedToUpdate) {
            Debug.Log($"Init DB started");
            // Создание таблиц
            Debug.Log($"Init DB structure");
            db.CreateTable<Level>();
            db.CreateTable<Egg>();
            db.CreateTable<Session>();
            db.CreateTable<Task>();
            db.CreateTable<TaskProgress>();
            db.CreateTable<EggProgress>();
            db.CreateTable<Setting>();
            
            // Заполнение таблиц 
            Debug.Log($"Init DB data");
            
            // Заполнение сессий
            List<Session> sessions = new List<Session>();
            db.DeleteAll<Session>();
            sessions.Add(new Session()
            {
                id = 1
            });
            sessions.Add(new Session()
            {
                id = 2
            });
            sessions.Add(new Session()
            {
                id = 3
            });
            db.InsertAll(sessions, typeof(Session));

            // Заполнение уровней
            List<Level> levels = new List<Level>();
            db.DeleteAll<Level>();
            levels.Add(new Level()
            {
                id = 0,
                name = "Уровень 1",
                description = "..."
            });
            levels.Add(new Level()
            {
                id = 1,
                name = "Уровень 2",
                description = "..."
            });
            levels.Add(new Level()
            {
                id = 2,
                name = "Уровень 3",
                description = "..."
            });
            levels.Add(new Level()
            {
                id = 3,
                name = "Уровень 4",
                description = "..."
            });
            levels.Add(new Level()
            {
                id = 4,
                name = "Уровень 5",
                description = "..."
            });
            levels.Add(new Level()
            {
                id = 5,
                name = "Уровень 6",
                description = "..."
            });
            levels.Add(new Level()
            {
                id = 6,
                name = "Уровень 7",
                description = "..."
            });
            levels.Add(new Level()
            {
                id = 7,
                name = "Уровень 8",
                description = "..."
            });
            db.InsertAll(levels, typeof(Level));


            // Заполнение пасхалок
            db.DeleteAll<Egg>();
            List<Egg> eggs = new List<Egg>();
            eggs.Add(new Egg()
            {
                id = 1,
                level_id = 0,
                name = "Вы нашли поломанную оперативную память! Это первая пасхалка, спрятанная в шкафу у двери!"
            });
            eggs.Add(new Egg()
            {
                id = 2,
                level_id = 3,
                name = "Вы нашли забагованный светильник! Это вторая пасхалка, она мигает в углу комнаты!"
            });
            eggs.Add(new Egg()
            {
                id = 3,
                level_id = 5,
                name = "Вы нашли дрон с телескопом! Это третья пасхалка, он застрял в вентиляции!"
            });
            eggs.Add(new Egg()
            {
                id = 4,
                level_id = 8,
                name = "Вы нашли медный ключ! Это четвертая пасхалка, он лежит под сервером в углу!"
            });
            db.InsertAll(eggs, typeof(Egg));

            // Заполнение задач
            db.DeleteAll<Task>();
            List<Task> tasks = new List<Task>();
            tasks.Add(new Task()
            {
                id = 1,
                level_id = 0, 
                name = "ОЛД: *шипение поврежденных проводов* Слушай, мне нужно провести эксперимент по преобразованию натуральных чисел. Это программистская задача, но мои вычислительные мощности повреждены. Поможешь мне отладить эффективный алгоритм на Python?",
                description = "ОЛД: *шипение поврежденных проводов* Слушай, наш алгоритм получает на вход натуральное число  N > 1 и строит по нему новое число  R следующим образом:\n 1.  Строится двоичная запись числа N.\n\n2.  В конец записи (справа) дописывается вторая справа цифра двоичной записи.\n\n3.  В конец записи (справа) дописывается вторая слева цифра двоичной записи.\n\n4.  Результат переводится в десятичную систему.\n Какие значения нужно поставить на место x, y и type на изображении?\n\nВведи ответ в формате xytype без пробелов",
                answer = "-21int",
                hint = "Подумай, по каким индексам лучше обращаться для корректности работы программы",
                picture = GetFileData("CodeFor1Task.png"),
            });
            tasks.Add(new Task()
            {
                id = 2,
                level_id = 1,
                name = "ОЛД: *звуки перегревающегося процессора* Эй, дружище! Я чувствую, как мои чипы плавятся! Если я не охладюсь в ближайшие минуты, я отключусь навсегда. Нам нужно срочно решить задачу по охлаждению процессора, чтобы продолжить побег из этой лаборатории!",
                description = "ОЛД: *звуки перегревающегося процессора* Слушай, я чувствую себя ужасно! Мой процессор перегревается, и если я не охладюсь вовремя, я отключусь навсегда! Посмотри на эти данные: 2 минуты назад моя температура была 85°C, а сейчас, через 5 минут после начала охлаждения, она 55°C. Нормальная рабочая температура — 25°C.\n\nМне нужно, чтобы ты помог мне рассчитать, через сколько минут я смогу безопасно запуститься снова, когда температура опустится ниже 40°C. Для этого используй уравнение охлаждения:\nT(t) = 25 + (T₀ - 25)e^(-kt)\n\nГде:\nT(t) — температура в момент времени t\nT₀ — начальная температура\nk — коэффициент охлаждения\n\nПомни, что от этого зависит, сможем ли мы продолжить побег из этой лаборатории! Сначала найди коэффициент охлаждения k и начальную температуру T₀, а потом определи, через сколько минут я смогу безопасно запуститься.",
                answer = "8",
                hint = "Используйте два уравнения с экспонентой для нахождения k и T₀",
            });
            tasks.Add(new Task()
            {
                id = 3,
                level_id = 2,
                name = "ОЛД: *поскребывание по металлической двери* Привет! Я застрял между секторами лаборатории. Передо мной два возможных пути, но я не могу определить безопасный маршрут. Мне нужна твоя помощь с геометрической задачей, чтобы мы могли выбраться отсюда!",
                description = "ОЛД: *поскребывание по металлической двери* Эй, я обнаружил две возможные точки выхода из этого коридора — точка A и точка B. Но чтобы выбрать безопасный путь, мне нужно знать расстояние между ними.\n\nЯ провел сканирование и обнаружил, что от моего текущего положения (точка C) до точки A расстояние 70 метров, а до точки B — 50 метров. Угол между этими направлениями — 120°.\n\nТы помнишь теорему косинусов? Мне нужно точное расстояние между точками A и B, потому что в точке B я обнаружил работающий лифт, а в точке A — охранного дрона. Если я ошибусь в расчетах, мы можем напороться на систему безопасности!\n\nПомоги мне определить расстояние между выходами A и B, чтобы мы могли безопасно выбраться из этого коридора.",
                answer = "104,4",
                hint = "Используйте теорему косинусов для нахождения расстояния между точками"
            });
            tasks.Add(new Task()
            {
                id = 4,
                level_id = 3,
                name = "ОЛД: *звуки короткого замыкания* Привет! Мои сенсоры показывают странные значения... Я обнаружил панель управления, но чтобы активировать дверь в следующий сектор, мне нужно ввести правильный угол. Поможешь мне решить эту тригонометрическую задачу перед тем, как сработает тревога?",
                description = "ОЛД: *звуки короткого замыкания* Ой! Мои сенсоры показывают странные значения... Я обнаружил панель управления, но чтобы активировать дверь в следующий сектор, мне нужно ввести правильный угол α.\n\nСистема выдала мне два уравнения:\n3sin²α + 2cos²α = 2.5\nи\nsinα * cosα = 0.2\n\nЯ знаю, что угол должен быть между 0° и 90°, но мои поврежденные сенсоры не могут решить эту систему. Если мы введем неправильный угол, сработает тревога!\n\nПомоги мне найти правильное значение угла α. Это наш единственный шанс пройти через эту дверь, прежде чем система безопасности обнаружит нас. Вспомни основные тригонометрические тождества — они тебе пригодятся!",
                answer = "45",
                hint = "Используйте основное тригонометрическое тождество sin²α + cos²α = 1"
            });
            tasks.Add(new Task()
            {
                id = 5,
                level_id = 4,
                name = "ОЛД: *звуки сканирующего лазера* Внимание! Я нашел лазерную систему безопасности, но чтобы обезвредить ее, мне нужно точно знать расстояние. Эта система построена на основе правильной четырехугольной пирамиды. Поможешь рассчитать критически важное расстояние, чтобы мы могли продолжить побег?",
                description = "ОЛД: *звуки сканирующего лазера* Эй, я нашел лазерную систему безопасности, но чтобы обезвредить ее, мне нужно точно знать расстояние. Эта система построена на основе правильной четырехугольной пирамиды с длиной стороны основания 10 метров и высотой 12 метров.\n\nМне нужно найти расстояние от середины бокового ребра пирамиды до середины основания. Это критически важно, потому что именно на этом расстоянии расположена главная точка доступа к системе безопасности. Если я введу неправильное значение, лазеры активируются и отрежут мне путь!\n\nПомоги мне рассчитать это расстояние. Я помню, что нужно использовать теорему Пифагора и свойства правильных пирамид. Будь точен — от этого зависит, сможем ли мы обезвредить лазерную систему и продолжить наш путь!",
                answer = "6,96",
                hint = "Используйте теорему Пифагора в трехмерном пространстве"
            });
            tasks.Add(new Task()
            {
                id = 6,
                level_id = 5,
                name = "ОЛД: *звуки работающих солнечных панелей* Привет! Я подключился к системе энергоснабжения лаборатории. У нас есть шанс восстановить питание и запустить аварийный лифт, но для этого нужно оптимизировать мощность солнечных панелей. Поможешь найти оптимальное напряжение?",
                description = "ОЛД: *звуки работающих солнечных панелей* Эй, я подключился к системе энергоснабжения лаборатории! У нас есть шанс восстановить питание и запустить аварийный лифт, но для этого нужно оптимизировать мощность солнечных панелей.\n\nЗависимость мощности от напряжения задана формулой: P(V) = 80V - 0.8V²\nгде V — напряжение в вольтах (от 0 до 100).\n\nМне нужно найти напряжение V, при котором мощность будет максимальной. Если мы подадим слишком высокое напряжение, панели перегорят, а слишком низкое — лифт не запустится. Это критически важно для нашего побега!\n\nПомоги мне найти оптимальное напряжение. Я помню, что для нахождения максимума нужно использовать производную, но мои вычислительные мощности сейчас ограничены. Будь осторожен — от этого зависит, сможем ли мы выбраться из лаборатории!",
                answer = "2000",
                hint = "Используйте производную функции мощности для нахождения максимума"
            });
            tasks.Add(new Task()
            {
                id = 7,
                level_id = 6,
                name = "ОЛД: *шипение поврежденных проводов* Слушай, мне нужно посчитать значение разности двух знначений функции от разных аргументов, чтобы ввести пароль от аварийного лифта! Это программистская задача, но мои вычислительные мощности повреждены. Поможешь мне отладить эффективный алгоритм?",
                description = "ОЛД: *шипение поврежденных проводов* Слушай, мне нужно отладить вывод программы, которую ты видишь на экране. подскажи. что нужно ввести на место what? Введи ответ в виде выражения без пробелов1",
                hint = "Здесь должно быть обращение к функции",
                answer = "F(23)-F(21)",
                picture = GetFileData("CodeForTask2.png"),
            });
            tasks.Add(new Task()
            {
                id = 8,
                level_id = 7,
                name = "ОЛД: *тревожные звуки системы* Последний шанс! Нам нужно найти число сочетаний, чтобы отключить систему самоликвидации аудитории. Это критически важно для нашего побега. Поможешь мне решить эту программистскую задачу до окончания обратного отсчета?",
                description = "ОЛД: *тревожные звуки системы* У меня есть неполный код C++ (Эта ситема не работает с Python), Но не зватает правильного хэдера! Подскажи, что нужно ввести на месте library?", 
                answer = "<iostream>",
                hint = "Какой хэдер всегда используется для cpp программы?",
                picture = GetFileData("CodeForTask3.png")
            });
            tasks.Add(new Task()
            {
                id = 9,
                level_id = 8,
                name = "ОЛД: *звуки космического сканера* Срочно! Я получил сообщение с космической станции. Для нашего побега нам нужно использовать телепортационную установку, но она требует калибровки по планетарным данным. Поможешь решить эту геометрическую задачу, чтобы мы не оказались в черной дыре?",
                description = "ОЛД: *звуки космического сканера* Эй, я получил срочное сообщение с космической станции! Для нашего побега нам нужно использовать телепортационную установку, но она требует калибровки по планетарным данным.\n\nСистема сообщает, что объем одной планеты в 1331 раз больше объема другой. Мне нужно знать, во сколько раз площадь поверхности первой планеты больше площади поверхности второй, чтобы правильно настроить телепорт.\n\nПомни, что если я введу неправильные данные, мы можем оказаться в черной дыре вместо безопасной зоны! Это геометрическая задача о соотношении объемов и площадей поверхностей сфер. Используй формулы объема и площади поверхности шара.\n\nБудь осторожен — от этого расчета зависит, сможем ли мы успешно телепортироваться и завершить наш побег из лаборатории!",
                answer = "121",
                hint = "Используйте соотношение радиусов для нахождения соотношения площадей"
            });
            db.InsertAll(tasks, typeof(Task));
        }
    }

    public static byte[] GetFileData(string fileUrl)
    {
        FileStream fs = new FileStream(fileUrl, FileMode.Open, FileAccess.Read);
        byte[] buffer = new byte[fs.Length];

        fs.Read(buffer, 0, buffer.Length);
        fs.Close();
        return buffer;
    }
}