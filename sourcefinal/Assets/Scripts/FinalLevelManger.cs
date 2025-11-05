using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using SQLite;

public class FinalLevelManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject statsPanel;
    public TMP_Text congratulationsText;
    public TMP_Text eggsStatsText;
    public TMP_Text missingEggsText;
    public TMP_Text websiteText;
    public Button closeButton;
    public Button menuButton;

    [Header("Secret Panel")]
    public GameObject secretPanel;
    public TMP_Text secretPanelText;
    public Button secretCloseButton;

    [Header("OLD Interaction")]
    public GameObject oldObject; // Ссылка на ОЛД объект на сцене

    // Поле класса для БД соединения
    private SQLiteConnection db;

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

    private List<long> foundEggIds;

    void Start()
    {
        InitializeDatabase();
        LoadEggProgress();
        SetupUI();
        ShowStatsPanel();
    }

    void InitializeDatabase()
    {
        try
        {
            db = new SQLiteConnection("escapeRoomBase.sqlite", SQLiteOpenFlags.ReadWrite);
            Debug.Log($"БД подключена в FinalLevelManager");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка подключения БД: {e.Message}");
        }
    }

    void LoadEggProgress()
    {
        // Пробуем получить через GameManager
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            foundEggIds = gameManager.GetFoundEggIds();
            Debug.Log($"Загружено пасхалок через GameManager: {foundEggIds.Count}");
        }
        else
        {
            // Запасной вариант - загружаем напрямую из БД
            LoadEggsDirectly();
        }
    }

    void LoadEggsDirectly()
    {
        try
        {
            long sessionId = MainMenuUIManager.CurrentSessionId;
            foundEggIds = db.Table<EggProgress>()
                .Where(ep => ep.session_id == sessionId)
                .Select(ep => ep.egg_id)
                .ToList();
            Debug.Log($"Загружено пасхалок напрямую из БД: {foundEggIds.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка загрузки пасхалок: {e.Message}");
            foundEggIds = new List<long>();
        }
    }

    void SetupUI()
    {
        closeButton.onClick.AddListener(HideStatsPanel);
        menuButton.onClick.AddListener(ReturnToMainMenu);
        secretCloseButton.onClick.AddListener(HideSecretPanel);
        secretPanel.SetActive(false);
    }

    void ShowStatsPanel()
    {
        congratulationsText.text = "Поздравляем с завершением игры!";
        eggsStatsText.text = $"Собрано пасхалок: {foundEggIds.Count}/4";

        if (foundEggIds.Count < 4)
        {
            var missingEggs = GetMissingEggs();
            missingEggsText.text = $"Пропущенные пасхалки на уровнях: {string.Join(", ", missingEggs)}";
        }
        else
        {
            missingEggsText.text = "Все пасхалки собраны! Отличный результат!";
        }

        websiteText.text = "Посетите наш сайт: www.MatInfo.ru";
        statsPanel.SetActive(true);
    }

    List<int> GetMissingEggs()
    {
        var missingLevels = new List<int>();
        int[] eggLevels = { 0, 3, 5, 8 };

        foreach (int level in eggLevels)
        {
            if (!IsEggCollectedOnLevel(level))
                missingLevels.Add(level);
        }
        return missingLevels;
    }

    bool IsEggCollectedOnLevel(int levelId)
    {
        try
        {
            var egg = db.Table<Egg>().FirstOrDefault(e => e.level_id == levelId);
            if (egg != null)
                return foundEggIds.Contains(egg.id);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка проверки пасхалки уровня {levelId}: {e.Message}");
        }
        return false;
    }

    void HideStatsPanel()
    {
        statsPanel.SetActive(false);
    }

    void ReturnToMainMenu()
    {
        Debug.Log("Возвращаемся в главное меню");

        if (db != null)
            db.Close();

        SceneManager.LoadScene("MenuScene");
    }

    void HideSecretPanel()
    {
        secretPanel.SetActive(false);
    }

    // Вызывается из OLDInteraction на уровне 9
    public void InteractWithOLD()
    {
        Debug.Log($"Взаимодействие с ОЛД. Собрано пасхалок: {foundEggIds.Count}/4");

        if (foundEggIds.Count >= 4)
        {
            // Все пасхалки собраны - переходим на секретный уровень
            LoadSecretLevel();
        }
        else
        {
            // Не все пасхалки собраны - показываем сообщение
            ShowNotAllEggsMessage();
        }
    }

    void LoadSecretLevel()
    {
        Debug.Log("Все пасхалки собраны! Загружаем секретный уровень...");

        // Закрываем БД перед сменой сцены
        if (db != null)
            db.Close();

        // Загружаем сцену Level10
        SceneManager.LoadScene("Level10");
    }

    void ShowNotAllEggsMessage()
    {
        int missingCount = 4 - foundEggIds.Count;
        secretPanelText.text = $"ОЛД: *мигает красным* \nТы собрал только {foundEggIds.Count} из 4 пасхалок!\nВернись и найди все секреты!\n\nПропущенные уровни: {string.Join(", ", GetMissingEggs())}";
        secretPanel.SetActive(true);

        Debug.Log($"Показано сообщение о недостающих пасхалках: {missingCount}");
    }

    void OnDestroy()
    {
        if (db != null)
            db.Close();
    }
}