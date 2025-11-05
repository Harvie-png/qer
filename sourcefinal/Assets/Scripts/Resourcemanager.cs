using UnityEngine;
using System.Linq;
using SQLite;
public class ResourceManager : MonoBehaviour
{
    private SQLiteConnection db;
    public static ResourceManager Instance { get; private set; }

    // Кэш для загруженных ресурсов
    private AudioClip currentMusic;
    private Sprite currentPlayerSprite;
    private Sprite currentOLDSprite;
    private Sprite currentBossSprite;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
            LoadCurrentResources(); // Загружаем ресурсы при старте
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeDatabase()
    {
        try
        {
            db = new SQLiteConnection("escapeRoomBase.sqlite", SQLiteOpenFlags.ReadWrite);
            Debug.Log("ResourceManager: БД подключена");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка подключения БД: {e.Message}");
        }
    }

    public void LoadCurrentResources()
    {
        LoadCurrentMusic();
        LoadCurrentTextures();
        ApplyResourcesToScene(); // Применяем ресурсы к текущей сцене
    }

    public AudioClip LoadCurrentMusic()
    {
        var settings = db.Table<Setting>().FirstOrDefault();
        if (settings == null)
        {
            Debug.LogWarning("Настройки не найдены, используем музыку по умолчанию");
            return Resources.Load<AudioClip>("Music/1");
        }

        var musicPackage = db.Table<MusicPackage>()
            .FirstOrDefault(m => m.id == settings.current_music_package);

        if (musicPackage != null)
        {
            string path = $"Music/{musicPackage.name}"; // Используем имя как путь
            AudioClip clip = Resources.Load<AudioClip>(path);

            if (clip != null)
            {
                currentMusic = clip;
                Debug.Log($"Музыка загружена: {musicPackage.name}");

                // Воспроизводим музыку
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                    audioSource = gameObject.AddComponent<AudioSource>();

                audioSource.clip = clip;
                audioSource.loop = true;
                audioSource.Play();
            }
            else
            {
                Debug.LogError($"Музыка не найдена по пути: {path}");
            }
            return clip;
        }

        return null;
    }

    // В методе LoadCurrentTextures
    public void LoadCurrentTextures()
    {
        var settings = db.Table<Setting>().FirstOrDefault();
        if (settings == null) return;

        var texturePackage = db.Table<TexturePackage>()
            .FirstOrDefault(t => t.id == settings.current_texture_package);

        if (texturePackage != null)
        {
            string folderPath = $"Textures/{texturePackage.name}";

            // Загружаем спрайты по твоим названиям
            currentPlayerSprite = Resources.Load<Sprite>($"{folderPath}/player{texturePackage.name}");
            currentOLDSprite = Resources.Load<Sprite>($"{folderPath}/old{texturePackage.name}");
            currentBossSprite = Resources.Load<Sprite>($"{folderPath}/oldbad{texturePackage.name}");

            Debug.Log($"Текстуры загружены из: {folderPath}");
            Debug.Log($"Player: player{texturePackage.name} - {currentPlayerSprite != null}");
            Debug.Log($"OLD: old{texturePackage.name} - {currentOLDSprite != null}");
            Debug.Log($"Boss: oldbad{texturePackage.name} - {currentBossSprite != null}");
        }
    }

    public void ApplyResourcesToScene()
    {
        // Применяем текстуры ко всем объектам на сцене по имени
        ApplyPlayerTexture();
        ApplyOLDTexture();
        ApplyBossTexture();
    }

    void ApplyPlayerTexture()
    {
        if (currentPlayerSprite != null)
        {
            // Ищем объект с точным именем "Player"
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.sprite = currentPlayerSprite;
                    Debug.Log("Текстура игрока применена");
                }
            }
            else
            {
                Debug.LogWarning("Объект 'Player' не найден на сцене");
            }
        }
    }
    void ApplyOLDTexture()
    {
        if (currentOLDSprite != null)
        {
            // Ищем объект с точным именем "OLD"
            GameObject old = GameObject.Find("OLD");
            if (old != null)
            {
                SpriteRenderer renderer = old.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.sprite = currentOLDSprite;
                    Debug.Log("Текстура OLD применена");
                }
            }
            else
            {
                Debug.LogWarning("Объект 'OLD' не найден на сцене");
            }
        }
    }
    void ApplyBossTexture()
    {
        if (currentBossSprite != null)
        {
            // Ищем объект с точным именем "Bad"
            GameObject bad = GameObject.Find("Bad");
            if (bad != null)
            {
                SpriteRenderer renderer = bad.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.sprite = currentBossSprite;
                    Debug.Log("Текстура босса применена");
                }
            }
            else
            {
                Debug.LogWarning("Объект 'Bad' не найден на сцене");
            }
        }
    }

    // Геттеры для других скриптов
    public Sprite GetPlayerSprite() => currentPlayerSprite;
    public Sprite GetOLDSprite() => currentOLDSprite;
    public Sprite GetBossSprite() => currentBossSprite;
    public AudioClip GetCurrentMusic() => currentMusic;

    void OnDestroy()
    {
        if (db != null)
            db.Close();
    }

    // Классы БД
    [Table("setting")]
    public class Setting
    {
        [PrimaryKey] public long id { get; set; }
        public long current_music_package { get; set; }
        public long current_texture_package { get; set; }
        public long debug_mode { get; set; }
    }

    [Table("music_package")]
    public class MusicPackage
    {
        [PrimaryKey] public long id { get; set; }
        public string name { get; set; }
        public string file_path { get; set; }
    }

    [Table("texture_package")]
    public class TexturePackage
    {
        [PrimaryKey] public long id { get; set; }
        public string name { get; set; }
        public string folder_path { get; set; }
    }
}