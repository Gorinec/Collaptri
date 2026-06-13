using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LineChecker : MonoBehaviour
{
    [Header("Line Settings")]
    public int minLineLength = 5; 
    public int scorePerLine = 100;
    public UIManager uiManager;

    private SandGrid sandGrid;
    private LineDestructionEffect lineFX;
    private bool isChecking = false;

    void Start()
    {
        sandGrid = FindFirstObjectByType<SandGrid>();
        lineFX = FindFirstObjectByType<LineDestructionEffect>();
    }

    public void CheckLinesOnce()
    {
        if (!isChecking) StartCoroutine(CheckLinesCoroutine());
    }

    System.Collections.IEnumerator CheckLinesCoroutine()
    {
        isChecking = true;
        // Даем песку время окончательно успокоиться
        yield return new WaitForSeconds(0.4f);

        if (sandGrid == null) { isChecking = false; yield break; }
        if (lineFX == null) lineFX = FindFirstObjectByType<LineDestructionEffect>();

        var sandCells = sandGrid.GetAllSand();
        if (sandCells == null || sandCells.Count == 0) { isChecking = false; yield break; }

        List<List<GameObject>> allLinesToClear = new List<List<GameObject>>();

        // Группируем по Y (горизонтальные линии)
        var rows = sandCells.GroupBy(cell => cell.Key.y).OrderBy(g => g.Key);

        foreach (var row in rows)
        {
            // Берем только уникальные X в ряду (на случай если песок наложился)
            var cellsInRow = row.OrderBy(c => c.Key.x).ToList();
            
            List<GameObject> currentLine = new List<GameObject>();
            Color? lastColor = null;
            int lastX = -999;

            foreach (var cell in cellsInRow)
            {
                GameObject sand = cell.Value;
                if (sand == null) continue;

                Color sandColor = sand.GetComponent<SpriteRenderer>().color;
                int currentX = cell.Key.x;

                // Проверяем: тот же цвет И идет подряд (X+1)
                bool sameColor = lastColor.HasValue && IsSameColor(sandColor, lastColor.Value);
                bool isAdjacent = (currentX == lastX + 1);

                if (sameColor && isAdjacent)
                {
                    currentLine.Add(sand);
                }
                else
                {
                    // Если накопили достаточно - сохраняем линию
                    if (currentLine.Count >= minLineLength)
                    {
                        allLinesToClear.Add(new List<GameObject>(currentLine));
                    }
                    // Начинаем новую цепочку
                    currentLine = new List<GameObject> { sand };
                    lastColor = sandColor;
                }
                lastX = currentX;
            }

            // Проверка хвоста ряда
            if (currentLine.Count >= minLineLength)
            {
                allLinesToClear.Add(new List<GameObject>(currentLine));
            }
        }

        // Выполнение удаления
        if (allLinesToClear.Count > 0)
        {
            if (AudioManager.Instance != null && AudioManager.Instance.lineClearSound != null)
                AudioManager.Instance.PlaySound(AudioManager.Instance.lineClearSound);

            int totalBlocks = 0;
            foreach (var line in allLinesToClear)
            {
                totalBlocks += line.Count;
                int rowY = Mathf.RoundToInt(line[0].transform.position.y / sandGrid.cellSize);
                
                if (lineFX != null) 
                    StartCoroutine(lineFX.PlayLineDestroyEffect(line, rowY));
                else
                    foreach (var b in line) sandGrid.RemoveSand(b);
            }

            if (uiManager != null) uiManager.AddScore(totalBlocks * scorePerLine);
            
            // Ждем завершения фазы "Подсветки" прежде чем сжимать песок
            yield return new WaitForSeconds(0.3f);
            sandGrid.CompactSand();
        }

        isChecking = false;
    }

    private bool IsSameColor(Color a, Color b)
    {
        // Увеличенный допуск для надежности (0.1 вместо 0.05)
        return Mathf.Abs(a.r - b.r) < 0.1f && 
               Mathf.Abs(a.g - b.g) < 0.1f && 
               Mathf.Abs(a.b - b.b) < 0.1f;
    }
}
