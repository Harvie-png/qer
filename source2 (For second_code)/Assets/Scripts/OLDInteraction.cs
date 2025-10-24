using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;

    private GameObject currentInteractable;

    void Update()
    {
        // Проверяем нажатие E
        if (Input.GetKeyDown(interactKey))
        {
            if (currentInteractable != null)
            {
                Debug.Log("Взаимодействие с: " + currentInteractable.name);

                // ВЗАИМОДЕЙСТВИЕ С ОЛД
                if (currentInteractable.name.Contains("Player"))
                {
                    GameManager gameManager = FindObjectOfType<GameManager>();
                    if (gameManager != null)
                    {
                        gameManager.ShowTaskDialogue();
                    }
                    else
                    {
                        Debug.LogError("GameManager не найден!");
                    }
                }

                
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем тег Interactable
        if (other.CompareTag("Interactable"))
        {
            currentInteractable = other.gameObject;
            Debug.Log("Можно взаимодействовать: " + other.name);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Interactable"))
        {
            currentInteractable = null;
            Debug.Log("Выход из зоны взаимодействия");
        }
    }
}