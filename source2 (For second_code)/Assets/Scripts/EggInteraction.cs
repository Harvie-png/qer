using UnityEngine;

public class EggInteraction : MonoBehaviour
{
    [Header("Egg Settings")]
    public long eggId = 1; // ID пасхалки в БД

    private GameManager gameManager;
    private bool isCollected = false;

    void Start()
    {
        StartCoroutine(InitializeEggWithDelay());
    }

    System.Collections.IEnumerator InitializeEggWithDelay()
    {
        // Ждем пока GameManager проинициализирует БД
        yield return new WaitForSeconds(0.5f);

        gameManager = FindObjectOfType<GameManager>();
        Debug.Log($"Egg {eggId}: GameManager found - {gameManager != null}");

        // Проверяем в БД, собрана ли уже эта пасхалка
        if (gameManager != null)
        {
            isCollected = gameManager.IsEggCollected(eggId);
            Debug.Log($"Egg {eggId}: Is collected from DB - {isCollected}");

            if (isCollected)
            {
                gameObject.SetActive(false); // Скрываем если уже собрана
                Debug.Log($"Пасхалка {eggId} уже собрана, скрываем");
            }
        }
        else
        {
            Debug.LogError($"Egg {eggId}: GameManager not found after delay!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Interactable") && !isCollected && gameManager != null)
        {
            CollectEgg();
        }
    }

    void CollectEgg()
    {
        isCollected = true;

        // Сохраняем в БД и показываем сообщение
        string eggText = gameManager.SaveEggProgress(eggId);
        gameManager.ShowEggFound(eggText);

        // Скрываем объект после сбора
        gameObject.SetActive(false);
        Debug.Log($"Пасхалка {eggId} собрана и сохранена");
    }
}