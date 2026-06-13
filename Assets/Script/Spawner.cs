using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] tetrominoPrefabs;
    public float spawnInterval = 3f;
    public Vector2 spawnPosition = new Vector2(0, 4.5f);

    // Цвета для превью в том же порядке, что и префабы
    public Color[] previewColors;

    private float timer;
    private int nextFigureIndex;
    private Color nextFigureColor;

    void Start()
    {
        timer = 1f;

        // Если цвета не заданы, используем стандартные
        if (previewColors == null || previewColors.Length == 0)
        {
            previewColors = new Color[]
    {
    new Color(1f, 0f, 0f, 1f),     // Ярко-красный
    new Color(0f, 0f, 1f, 1f),     // Ярко-синий
    new Color(0f, 1f, 0f, 1f),     // Ярко-зеленый
    new Color(1f, 1f, 0f, 1f)      // Ярко-желтый
    };
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(); // ← Убрал .music, теперь просто PlayMusic()
            }
            if (PlayerPrefs.GetInt("TutorialShown", 0) == 0)
            {
                enabled = false;
            }
        }

        // Выбираем первую следующую фигуру
        nextFigureIndex = Random.Range(0, tetrominoPrefabs.Length);
        nextFigureColor = previewColors[nextFigureIndex % previewColors.Length];
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0 && FindFirstObjectByType<Tetromino>() == null)
        {
            SpawnNextTetromino();
            timer = spawnInterval;
        }
    }

    void SpawnNextTetromino()
    {
        if (tetrominoPrefabs.Length == 0)
        {
            Debug.LogError("No tetromino prefabs assigned!");
            return;
        }

        int currentIndex = nextFigureIndex;
        Color currentColor = nextFigureColor;

        // Выбираем следующую фигуру
        nextFigureIndex = Random.Range(0, tetrominoPrefabs.Length);
        nextFigureColor = previewColors[nextFigureIndex % previewColors.Length];

        // Спавним текущую фигуру с её цветом
        GameObject newTetromino = Instantiate(tetrominoPrefabs[currentIndex], spawnPosition, Quaternion.identity);
        Tetromino tetro = newTetromino.GetComponent<Tetromino>();
        if (tetro != null)
        {
            tetro.ApplyColor(currentColor);
        }
    }

    public int GetNextFigureIndex()
    {
        return nextFigureIndex;
    }

    public Color GetNextFigureColor()
    {
        return nextFigureColor;
    }
    public void StopSpawning()
    {
        enabled = false;
        Debug.Log("Spawning stopped!");
    }
}