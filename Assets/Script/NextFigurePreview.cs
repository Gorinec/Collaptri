using UnityEngine;
using UnityEngine.UI;

public class NextFigurePreview : MonoBehaviour
{
    [Header("References")]
    public Spawner spawner;
    public Image previewImage;

    [Header("Figure Sprites")]
    public Sprite[] figureSprites;

    private void Start()
    {
        if (spawner == null)
            spawner = FindFirstObjectByType<Spawner>();

        if (previewImage == null)
            previewImage = GetComponent<Image>();
    }

    private void Update()
    {
        if (spawner != null && previewImage != null && figureSprites != null && figureSprites.Length > 0)
        {
            int nextIndex = spawner.GetNextFigureIndex();
            Color nextColor = spawner.GetNextFigureColor();

            if (nextIndex >= 0 && nextIndex < figureSprites.Length && figureSprites[nextIndex] != null)
            {
                previewImage.sprite = figureSprites[nextIndex];
                previewImage.color = nextColor;
                previewImage.preserveAspect = true;
            }
        }
    }
}
