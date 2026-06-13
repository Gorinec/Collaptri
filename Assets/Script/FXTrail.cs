using UnityEngine;

/// <summary>
/// Helper component for visual effects that move and fade independently.
/// </summary>
public class FXTrail : MonoBehaviour 
{
    public Vector2 velocity;
    public float duration;
    private float elapsed;
    private SpriteRenderer sr;
    private Color startCol;

    void Start() 
    { 
        sr = GetComponent<SpriteRenderer>(); 
        if (sr != null) startCol = sr.color; 
        Destroy(gameObject, duration); 
    }

    void Update() 
    {
        // Simple manual physics
        velocity += Vector2.down * 9.81f * Time.deltaTime;
        transform.position += (Vector3)velocity * Time.deltaTime;
        
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        
        if (sr != null)
        {
            sr.color = new Color(startCol.r, startCol.g, startCol.b, startCol.a * (1 - t));
        }
    }
}
