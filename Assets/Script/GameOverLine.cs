using UnityEngine;

public class GameOverLine : MonoBehaviour
{
    public float blinkSpeed = 0.5f;
    public bool isActive = true;

    private SpriteRenderer sr;
    private float timer;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = gameObject.AddComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!isActive) return;

        // Мигание
        timer += Time.deltaTime;
        float alpha = (Mathf.Sin(timer * blinkSpeed * Mathf.PI * 2) + 1) / 2;
        Color color = sr.color;
        color.a = alpha * 0.8f + 0.2f; // От 0.2 до 1.0
        sr.color = color;
    }

    public void Activate()
    {
        isActive = true;
    }

    public void Deactivate()
    {
        isActive = false;
        Color color = sr.color;
        color.a = 0;
        sr.color = color;
    }
}