using UnityEngine;

public class BlinkingObject : MonoBehaviour
{
    [Header("Colors")]
    public Color color1 = Color.yellow;
    public Color color2 = Color.black;

    [Header("Blink Settings")]
    public float blinkSpeed = 0.5f; // время между сменой цветов

    private SpriteRenderer spriteRenderer;
    private float timer;
    private bool isColor1 = true;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Нет SpriteRenderer на объекте!");
            return;
        }

        spriteRenderer.color = color1; // начальный цвет
    }

    void Update()
    {
        if (spriteRenderer == null) return;

        timer += Time.deltaTime;

        if (timer >= blinkSpeed)
        {
            // Меняем цвет
            if (isColor1)
            {
                spriteRenderer.color = color2;
            }
            else
            {
                spriteRenderer.color = color1;
            }

            isColor1 = !isColor1;
            timer = 0f;
        }
    }
}