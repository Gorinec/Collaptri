using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// Restored SwipeController logic from the beginning of the dialogue.
/// Simple swipes: X for move, Y-Down for fast fall, Y-Up or Tap for rotate.
/// </summary>
public class SwipeInputHandler : MonoBehaviour
{
    [Header("Swipe Settings")]
    public float swipeThreshold = 50f;
    public float fastFallThreshold = 80f;

    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private bool isTouching = false;
    private Tetromino currentTetromino;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        if (currentTetromino == null)
        {
            currentTetromino = FindFirstObjectByType<Tetromino>();
        }

        if (currentTetromino == null) return;

        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                startTouchPosition = touch.screenPosition;
                isTouching = true;
            }
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended && isTouching)
            {
                endTouchPosition = touch.screenPosition;
                DetectSwipe();
                isTouching = false;
            }
        }
    }

    void DetectSwipe()
    {
        Vector2 swipeDelta = endTouchPosition - startTouchPosition;

        // If movement is mostly horizontal
        if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
        {
            if (Mathf.Abs(swipeDelta.x) > swipeThreshold)
            {
                if (swipeDelta.x > 0)
                {
                    currentTetromino.Move(1);
                }
                else
                {
                    currentTetromino.Move(-1);
                }
                Invoke(nameof(StopMove), 0.15f);
            }
            else
            {
                // Simple tap/short swipe -> Rotate
                currentTetromino.Rotate();
            }
        }
        // If movement is mostly vertical
        else
        {
            if (Mathf.Abs(swipeDelta.y) > fastFallThreshold)
            {
                if (swipeDelta.y < 0)
                {
                    currentTetromino.FastFall(true);
                    Invoke(nameof(StopFastFall), 0.25f);
                }
                else
                {
                    // Swipe up -> Rotate
                    currentTetromino.Rotate();
                }
            }
            else if (Mathf.Abs(swipeDelta.y) > swipeThreshold)
            {
                // Short vertical swipe -> Rotate
                currentTetromino.Rotate();
            }
            else
            {
                // Tap -> Rotate
                currentTetromino.Rotate();
            }
        }
    }

    void StopMove()
    {
        if (currentTetromino != null) currentTetromino.StopMove();
    }

    void StopFastFall()
    {
        if (currentTetromino != null) currentTetromino.FastFall(false);
    }
}
