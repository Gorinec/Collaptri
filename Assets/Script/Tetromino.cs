using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles Tetromino behavior. 
/// Fixed to prevent sticking to walls and only break when hitting something from below.
/// </summary>
public class Tetromino : MonoBehaviour
{
    [Header("Settings")]
    public float fallSpeed = 2f;
    public float moveSpeed = 5f;
    public float fastFallMultiplier = 4f;

    [Header("Sand Settings")]
    public GameObject sandPrefab;
    public int sandPerBlock = 5;
    public float sandSpreadRange = 0.2f;
    public bool coloredSand = true;

    private Rigidbody2D rb;
    private bool isFalling = true;
    private bool isFastFalling = false;
    private float moveDirection = 0f;
    private float normalFallSpeed;
    private SandGrid sandGrid;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Ensure no friction to prevent sticking to walls
        PhysicsMaterial2D mat = new PhysicsMaterial2D("TetroMaterial");
        mat.friction = 0f;
        mat.bounciness = 0f;
        rb.sharedMaterial = mat;
    }

    private void Start()
    {
        sandGrid = FindFirstObjectByType<SandGrid>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true; 
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        normalFallSpeed = fallSpeed;
        rb.linearVelocity = new Vector2(0, -normalFallSpeed);
    }

    public void ApplyColor(Color newColor)
    {
        foreach (Transform child in transform)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = newColor;
                BlockColor bc = child.gameObject.GetComponent<BlockColor>() ?? child.gameObject.AddComponent<BlockColor>();
                bc.blockColor = newColor;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isFalling)
        {
            float targetX = moveDirection * moveSpeed;
            float currentFallSpeed = isFastFalling ? normalFallSpeed * fastFallMultiplier : normalFallSpeed;
            rb.linearVelocity = new Vector2(targetX, -currentFallSpeed);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isFalling || collision.gameObject.transform.parent == transform) return;

        // Visual effects for side walls
        string objName = collision.gameObject.name.ToLower();
        if (objName.Contains("left") || objName.Contains("right"))
        {
            TriggerSlideEffect(collision);
        }

        // CRITICAL: Only stop if hit from BELOW
        bool hitFromBelow = false;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // If the other object is pushing us UP, it's below us.
            if (contact.normal.y > 0.4f) 
            {
                hitFromBelow = true;
                break;
            }
        }

        if (hitFromBelow)
        {
            StopAndTurnToSand();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!isFalling) return;
        string objName = collision.gameObject.name.ToLower();
        if (objName.Contains("left") || objName.Contains("right"))
        {
            TriggerSlideEffect(collision);
        }
    }

    private void TriggerSlideEffect(Collision2D collision)
    {
        if (collision.contactCount > 0)
        {
            ContactPoint2D contact = collision.contacts[0];
            SpriteRenderer sr = contact.otherCollider.GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
            CreateEffect(contact.point, sr != null ? sr.color : Color.white, 2, 0.12f, 0.3f);
        }
    }

    private void CreateEffect(Vector3 pos, Color col, int count, float size, float duration)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject p = new GameObject("FX_Particle");
            p.transform.position = new Vector3(pos.x, pos.y, -1.2f);
            p.transform.localScale = Vector3.one * size;
            
            SpriteRenderer psr = p.AddComponent<SpriteRenderer>();
            psr.sprite = GetParticleSprite();
            psr.color = new Color(col.r, col.g, col.b, 1.0f); 
            psr.sortingOrder = 300;

            var trail = p.AddComponent<FXTrail>();
            trail.velocity = new Vector2(Random.Range(-2.5f, 2.5f), Random.Range(1.5f, 4f));
            trail.duration = duration;
        }
    }

    private static Sprite _cachedPart;
    private Sprite GetParticleSprite()
    {
        if (_cachedPart != null) return _cachedPart;
        Texture2D tex = new Texture2D(32, 32);
        for (int x = 0; x < 32; x++) for (int y = 0; y < 32; y++) 
            tex.SetPixel(x, y, Vector2.Distance(new Vector2(x,y), new Vector2(16,16)) < 14 ? Color.white : Color.clear);
        tex.Apply();
        return _cachedPart = Sprite.Create(tex, new Rect(0,0,32,32), new Vector2(0.5f,0.5f), 100);
    }

    private void StopAndTurnToSand()
    {
        if (!isFalling) return;
        isFalling = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (AudioManager.Instance != null && AudioManager.Instance.blockLandSound != null)
            AudioManager.Instance.PlaySound(AudioManager.Instance.blockLandSound);

        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);

        foreach (GameObject child in children)
        {
            if (child == null) continue;
            Color c = child.GetComponent<SpriteRenderer>()?.color ?? Color.white;
            CreateEffect(child.transform.position, c, 4, 0.18f, 0.3f); 
            TransformBlockIntoSand(child);
        }
        Destroy(gameObject);
    }

    private void TransformBlockIntoSand(GameObject block)
    {
        Color c = block.GetComponent<BlockColor>()?.blockColor ?? block.GetComponent<SpriteRenderer>().color;
        for (int i = 0; i < sandPerBlock; i++)
        {
            GameObject s = Instantiate(sandPrefab);
            if (s != null)
            {
                s.GetComponent<SpriteRenderer>().color = c;
                if (sandGrid != null) sandGrid.AddSand(s, block.transform.position + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0));
            }
        }
        Destroy(block);
    }

    public void Move(float direction) => moveDirection = direction;
    public void StopMove() { moveDirection = 0; }
    public void Rotate() { if (isFalling) transform.Rotate(0, 0, -90); }
    public void FastFall(bool fast) => isFastFalling = fast;
}
