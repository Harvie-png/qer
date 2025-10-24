using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.VisualScripting.Dependencies.Sqlite;

public class FinalLevelManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject statsPanel;
    public TMP_Text congratulationsText;
    public TMP_Text eggsStatsText;
    public TMP_Text missingEggsText;
    public TMP_Text websiteText;
    public Button closeButton;

    [Header("Secret Panel")]
    public GameObject secretPanel;
    public TMP_Text secretPanelText;
    public Button secretCloseButton;

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
        LoadEggProgress();
        SetupUI();
        ShowStatsPanel();
    }

    void LoadEggProgress()
    {
        // Пробуем получить через GameManager
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            foundEggIds = gameManager.GetFoundEggIds();
        }
        else
        {
            // Запасной вариант
            LoadEggsDirectly();
        }

        Debug.Log($"Загружено пасхалок: {foundEggIds?.Count ?? 0}");
    }

    void LoadEggsDirectly()
    {
        try
        {
            using (var db = new SQLiteConnection("escapeRoomBase.sqlite", SQLiteOpenFlags.ReadWrite))
            {
                long sessionId = MainMenuUIManager.CurrentSessionId;
                foundEggIds = db.Table<EggProgress>()
                    .Where(ep => ep.session_id == sessionId)
                    .Select(ep => ep.egg_id)
                    .ToList();
            }
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

        websiteText.text = "Посетите наш сайт: www.example.com"; // Замени на свой
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
            using (var db = new SQLiteConnection("escapeRoomBase.sqlite", SQLiteOpenFlags.ReadWrite))
            {
                var egg = db.Table<Egg>().FirstOrDefault(e => e.level_id == levelId);
                if (egg != null)
                    return foundEggIds.Contains(egg.id);
            }
        }
        catch { }
        return false;
    }

    void HideStatsPanel()
    {
        statsPanel.SetActive(false);
    }

    void HideSecretPanel()
    {
        secretPanel.SetActive(false);
    }

    // Вызывается из OLDInteraction
    public void InteractWithOLD()
    {
        if (foundEggIds.Count >= 4)
        {
            LoadSecretScene();
        }
        else
        {
            ShowNotAllEggsMessage();
        }
    }

    void LoadSecretScene()
    {
        Debug.Log("Загружаем секретный уровень!");
        SceneManager.LoadScene("SecretScene");
    }

    void ShowNotAllEggsMessage()
    {
        int missingCount = 4 - foundEggIds.Count;
        secretPanelText.text = $"ОЛД: *мигает красным* \nТы собрал только {foundEggIds.Count} из 4 пасхалок!\nВернись и найди все секреты!";
        secretPanel.SetActive(true);
    }
}