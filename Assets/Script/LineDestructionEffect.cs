using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineDestructionEffect : MonoBehaviour
{
    [Header("Settings")]
    public float flashDuration = 0.3f;
    public float highlightDuration = 0.25f; // New: time to pulse/glow
    public float dissolveDuration = 1.0f;    // Slowed down from 0.5f
    public Color neonColor = new Color(0, 255/255f, 255/255f, 1); // Cyan Neon
    public int particlesPerBlock = 12;

    private SandGrid sandGrid;
    private ParticleSystem _psInstance;

    private void Awake()
    {
        sandGrid = FindFirstObjectByType<SandGrid>();
        SetupDefaultParticleSystem();
    }

    private void SetupDefaultParticleSystem()
    {
        GameObject psObj = new GameObject("LineDestructionParticles");
        psObj.transform.SetParent(transform);
        _psInstance = psObj.AddComponent<ParticleSystem>();

        var main = _psInstance.main;
        main.startLifetime = 1.2f; // Longer particles
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.0f, 3.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
        main.gravityModifier = 0.25f; // Slower fall
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;

        var emission = _psInstance.emission;
        emission.enabled = false;

        var velocity = _psInstance.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-4f, 4f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.5f, 1.5f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f); // Fix: all axes same mode

        var sizeOverLifetime = _psInstance.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, 0.0f);

        var colorOverLifetime = _psInstance.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(neonColor, 0.6f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = grad;

        var renderer = psObj.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 1000;
    }

    public IEnumerator PlayLineDestroyEffect(List<GameObject> blocksInLine, int lineIndex)
    {
        if (sandGrid == null) sandGrid = FindFirstObjectByType<SandGrid>();
        if (sandGrid == null || blocksInLine == null || blocksInLine.Count == 0) yield break;

        float worldY = lineIndex * sandGrid.cellSize;

        // 1. Bright Neon Flash over the whole line
        StartCoroutine(FlashNeonLineCoroutine(worldY));

        // Create ghosts immediately to preserve state before SandGrid removes them
        List<BlockGhost> ghosts = new List<BlockGhost>();
        foreach (var block in blocksInLine)
        {
            if (block == null) continue;
            SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
            
            GameObject gObj = new GameObject("FX_Ghost");
            gObj.transform.position = block.transform.position;
            gObj.transform.localScale = block.transform.localScale;
            
            SpriteRenderer gSr = gObj.AddComponent<SpriteRenderer>();
            gSr.sprite = sr.sprite;
            gSr.color = sr.color;
            gSr.sortingOrder = sr.sortingOrder + 10;
            
            ghosts.Add(new BlockGhost { obj = gObj, sr = gSr, initialColor = sr.color, initialScale = block.transform.localScale });
            
            sandGrid.RemoveSand(block);
        }

        // 2. HIGHLIGHT Phase: Pulse blocks white and scale up slightly to show they gathered
        float elapsed = 0;
        while (elapsed < highlightDuration)
        {
            float t = elapsed / highlightDuration;
            float curve = Mathf.Sin(t * Mathf.PI); // Pulse 0 -> 1 -> 0
            
            foreach (var g in ghosts)
            {
                if (g.obj == null) continue;
                g.obj.transform.localScale = g.initialScale * (1f + curve * 0.25f);
                g.sr.color = Color.Lerp(g.initialColor, Color.white, curve * 0.8f);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. DISSOLVE Phase: Fade out slowly and emit particles
        elapsed = 0;
        bool emitted = false;
        while (elapsed < dissolveDuration)
        {
            float t = elapsed / dissolveDuration;
            
            if (!emitted && t > 0.1f)
            {
                foreach (var g in ghosts) EmitParticlesAt(g.obj.transform.position, g.initialColor);
                emitted = true;
            }

            foreach (var g in ghosts)
            {
                if (g.obj == null) continue;
                g.obj.transform.localScale = Vector3.Lerp(g.initialScale * 1.25f, Vector3.zero, t);
                g.sr.color = new Color(g.initialColor.r, g.initialColor.g, g.initialColor.b, 1f - t);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (var g in ghosts) if (g.obj != null) Destroy(g.obj);
    }

    private struct BlockGhost
    {
        public GameObject obj;
        public SpriteRenderer sr;
        public Color initialColor;
        public Vector3 initialScale;
    }

    private IEnumerator FlashNeonLineCoroutine(float y)
{
        GameObject flash = new GameObject("NeonFlash");
        flash.transform.position = new Vector3(0, y, -0.5f);
        float width = (sandGrid.maxGridX - sandGrid.minGridX + 2) * sandGrid.cellSize;
        flash.transform.localScale = new Vector3(width, 0.02f, 1f);

        SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
        sr.sprite = GetSquareSprite();
        sr.color = neonColor;
        sr.sortingOrder = 1100;

        float elapsed = 0;
        while (elapsed < flashDuration)
        {
            float t = elapsed / flashDuration;
            flash.transform.localScale = new Vector3(width, Mathf.Lerp(0.02f, 0.8f, t), 1f);
            sr.color = new Color(neonColor.r, neonColor.g, neonColor.b, 1f - t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(flash);
    }

    private void EmitParticlesAt(Vector3 pos, Color color)
{
        if (_psInstance == null) return;
        var emitParams = new ParticleSystem.EmitParams { position = pos, startColor = color };
        _psInstance.Emit(emitParams, particlesPerBlock);
    }

    private Sprite _cachedSquare;
    private Sprite GetSquareSprite()
    {
        if (_cachedSquare != null) return _cachedSquare;
        Texture2D tex = new Texture2D(2, 2);
        for (int x = 0; x < 2; x++) for (int y = 0; y < 2; y++) tex.SetPixel(x, y, Color.white);
        tex.Apply();
        return _cachedSquare = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 100);
    }
}
