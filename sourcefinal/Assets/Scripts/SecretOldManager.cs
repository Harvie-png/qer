using UnityEngine;

public class OLDInteraction : MonoBehaviour
{
    private bool playerInRange = false;

    void Update()
    {
        // Проверяем нажатие E только когда игрок рядом
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            FindObjectOfType<FinalLevelManager>().InteractWithOLD();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Interactable"))
        {
            playerInRange = true;
            Debug.Log("Игрок у ОЛД - нажми E для взаимодействия");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Interactable"))
        {
            playerInRange = false;
            Debug.Log("Игрок отошел от ОЛД");
        }
    }
}