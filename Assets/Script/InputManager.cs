using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    private Tetromino currentTetromino;

    private bool moveLeftPressed = false;
    private bool moveRightPressed = false;
    private bool rotatePressed = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (currentTetromino == null)
            currentTetromino = FindFirstObjectByType<Tetromino>();

        if (currentTetromino != null && rotatePressed)
        {
            currentTetromino.Rotate();
            rotatePressed = false;
        }
    }

    public void OnMoveLeft(InputAction.CallbackContext context)
    {
        if (currentTetromino == null) return;
        if (context.performed) { moveLeftPressed = true; currentTetromino.Move(-1); }
        else if (context.canceled) { moveLeftPressed = false; if (!moveRightPressed) currentTetromino.StopMove(); }
    }

    public void OnMoveRight(InputAction.CallbackContext context)
    {
        if (currentTetromino == null) return;
        if (context.performed) { moveRightPressed = true; currentTetromino.Move(1); }
        else if (context.canceled) { moveRightPressed = false; if (!moveLeftPressed) currentTetromino.StopMove(); }
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        if (context.performed) rotatePressed = true;
    }

    public void OnFastFall(InputAction.CallbackContext context)
    {
        if (currentTetromino == null) return;
        if (context.performed) currentTetromino.FastFall(true);
        else if (context.canceled) currentTetromino.FastFall(false);
    }
}
