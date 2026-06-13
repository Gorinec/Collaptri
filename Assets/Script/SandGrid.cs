using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public float cellSize = 0.5f;
    public float updateInterval = 0.15f; // ← УВЕЛИЧИЛ (было 0.08)
    public int gridHeight = 40;

    [Header("Game Over Settings")]
    public float gameOverLineWorldY = 2f;

    [Header("Grid Boundaries")]
    public int minGridX = -6;
    public int maxGridX = 6;
    public int minGridY = -10;

    [Header("Smooth Settings")]
    public float smoothTime = 0.2f; // ← УВЕЛИЧИЛ (было 0.1) - движения плавнее и медленнее

    private Dictionary<Vector2Int, GameObject> sandCells = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<GameObject, Coroutine> activeMovements = new Dictionary<GameObject, Coroutine>();
    private float timer;
    private bool gameOverTriggered = false;
    private int gameOverLineGridY;
    private LineChecker lineChecker;

    void Start()
    {
        timer = updateInterval;
        gameOverLineGridY = Mathf.RoundToInt(gameOverLineWorldY / cellSize);
        lineChecker = FindFirstObjectByType<LineChecker>();
    }

    void Update()
    {
        if (gameOverTriggered) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            UpdateSandPhysics();
            timer = updateInterval;
        }
    }

    public void AddSand(GameObject sand, Vector3 worldPosition)
    {
        worldPosition.x = Mathf.Clamp(worldPosition.x, -3.2f, 3.2f);

        if (gameOverTriggered)
        {
            Destroy(sand);
            return;
        }

        Vector2Int gridPos = WorldToGrid(worldPosition);
        gridPos.x = Mathf.Clamp(gridPos.x, minGridX, maxGridX);

        while (sandCells.ContainsKey(gridPos) && gridPos.y < gridHeight)
        {
            gridPos.y += 1;
        }

        if (gridPos.y < gridHeight)
        {
            sandCells[gridPos] = sand;
            sand.transform.position = GridToWorld(gridPos);

            if (lineChecker != null) lineChecker.CheckLinesOnce();
        }
        else
        {
            TriggerGameOver();
            Destroy(sand);
        }
    }

    void CheckAndTriggerGameOver(int gridY)
    {
        if (!gameOverTriggered && gridY >= gameOverLineGridY)
        {
            TriggerGameOver();
        }
    }

    void TriggerGameOver()
    {
        if (gameOverTriggered) return;
        gameOverTriggered = true;

        UIManager ui = FindFirstObjectByType<UIManager>();
        if (ui != null) ui.ShowGameOver();

        Spawner spawner = FindFirstObjectByType<Spawner>();
        if (spawner != null) spawner.enabled = false;
    }

    public Dictionary<Vector2Int, GameObject> GetAllSand()
    {
        return sandCells;
    }

    public void RemoveSand(GameObject sand)
    {
        Vector2Int? posToRemove = null;
        foreach (var cell in sandCells)
        {
            if (cell.Value == sand)
            {
                posToRemove = cell.Key;
                break;
            }
        }

        if (posToRemove.HasValue)
        {
            if (activeMovements.ContainsKey(sand))
            {
                StopCoroutine(activeMovements[sand]);
                activeMovements.Remove(sand);
            }
            sandCells.Remove(posToRemove.Value);
            Destroy(sand);
        }
    }

    void UpdateSandPhysics()
    {
        if (gameOverTriggered) return;

        List<Vector2Int> positions = new List<Vector2Int>(sandCells.Keys);
        positions.Sort((a, b) => a.y.CompareTo(b.y));

        bool sandMoved = false;

        foreach (Vector2Int pos in positions)
        {
            if (!sandCells.ContainsKey(pos)) continue;

            GameObject sand = sandCells[pos];
            if (sand == null) { sandCells.Remove(pos); continue; }

            Vector2Int downPos = new Vector2Int(pos.x, pos.y - 1);
            Vector2Int downLeftPos = new Vector2Int(pos.x - 1, pos.y - 1);
            Vector2Int downRightPos = new Vector2Int(pos.x + 1, pos.y - 1);

            if (downPos.y >= minGridY && !sandCells.ContainsKey(downPos))
            {
                StartSmoothMove(sand, pos, downPos);
                sandMoved = true;
                CheckAndTriggerGameOver(downPos.y);
                continue;
            }

            if (Random.value < 0.5f)
            {
                if (downLeftPos.x >= minGridX && downLeftPos.y >= minGridY && !sandCells.ContainsKey(downLeftPos))
                {
                    StartSmoothMove(sand, pos, downLeftPos);
                    sandMoved = true;
                    CheckAndTriggerGameOver(downLeftPos.y);
                    continue;
                }
                if (downRightPos.x <= maxGridX && downRightPos.y >= minGridY && !sandCells.ContainsKey(downRightPos))
                {
                    StartSmoothMove(sand, pos, downRightPos);
                    sandMoved = true;
                    CheckAndTriggerGameOver(downRightPos.y);
                    continue;
                }
            }
            else
            {
                if (downRightPos.x <= maxGridX && downRightPos.y >= minGridY && !sandCells.ContainsKey(downRightPos))
                {
                    StartSmoothMove(sand, pos, downRightPos);
                    sandMoved = true;
                    CheckAndTriggerGameOver(downRightPos.y);
                    continue;
                }
                if (downLeftPos.x >= minGridX && downLeftPos.y >= minGridY && !sandCells.ContainsKey(downLeftPos))
                {
                    StartSmoothMove(sand, pos, downLeftPos);
                    sandMoved = true;
                    CheckAndTriggerGameOver(downLeftPos.y);
                    continue;
                }
            }
        }

        if (sandMoved && !gameOverTriggered)
        {
            if (lineChecker != null) lineChecker.CheckLinesOnce();
            UpdateSandPhysics();
        }
    }

    void StartSmoothMove(GameObject sand, Vector2Int oldPos, Vector2Int newPos)
    {
        if (!sandCells.ContainsKey(oldPos) || sand == null) return;
        if (activeMovements.ContainsKey(sand))
        {
            StopCoroutine(activeMovements[sand]);
            activeMovements.Remove(sand);
        }
        sandCells.Remove(oldPos);
        sandCells[newPos] = sand;
        activeMovements[sand] = StartCoroutine(SmoothMove(sand, oldPos, newPos));
    }

    IEnumerator SmoothMove(GameObject sand, Vector2Int oldPos, Vector2Int newPos)
    {
        Vector3 startPos = sand.transform.position;
        Vector3 endPos = GridToWorld(newPos);
        float elapsed = 0;
        while (elapsed < smoothTime)
        {
            float t = elapsed / smoothTime;
            t = Mathf.SmoothStep(0, 1, t);
            sand.transform.position = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        sand.transform.position = endPos;
        activeMovements.Remove(sand);
    }

    public void CompactSand()
    {
        UpdateSandPhysics();
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPos.x / cellSize), Mathf.RoundToInt(worldPos.y / cellSize));
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0);
    }
}