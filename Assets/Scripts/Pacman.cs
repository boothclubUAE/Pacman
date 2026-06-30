using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Movement))]
public class Pacman : MonoBehaviour
{
    [SerializeField]
    private AnimatedSprite deathSequence;

    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    internal Movement movement;

    // Input buffers
    private Vector2 keyboardDirection = Vector2.zero;
    private Vector2 externalDirection = Vector2.zero;

    private readonly Dictionary<KeyCode, Vector2> keyToDirection = new()
    {
        { KeyCode.UpArrow, Vector2.up },
        { KeyCode.DownArrow, Vector2.down },
        { KeyCode.LeftArrow, Vector2.left },
        { KeyCode.RightArrow, Vector2.right }
    };

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        movement = GetComponent<Movement>();
        movement.isPlayerControlled = true;
    }

    private void Update()
    {
        HandleKeyboardInput();
        HandleMovement();
        HandleRotation();
    }

    // ---------------- INPUT ----------------
    private void HandleKeyboardInput()
    {
        foreach (var kvp in keyToDirection)
        {
            if (Input.GetKeyDown(kvp.Key))
            {
                keyboardDirection = kvp.Value;
            }
        }
    }

    // ---------------- MOVEMENT ----------------
    private void HandleMovement()
    {
        Vector2 chosenDirection = Vector2.zero;

        // external has priority
        if (externalDirection != Vector2.zero)
        {
            chosenDirection = externalDirection;
        }
        else if (keyboardDirection != Vector2.zero)
        {
            chosenDirection = keyboardDirection;
        }

        if (chosenDirection != Vector2.zero && !movement.Occupied(chosenDirection))
        {
            movement.SetDirection(chosenDirection, forced: true);

            keyboardDirection = Vector2.zero;
            externalDirection = Vector2.zero;
        }
    }

    // ---------------- ROTATION ----------------
    private void HandleRotation()
    {
        if (movement.direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(movement.direction.y, movement.direction.x);
            transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
        }
    }

    // ---------------- EXTERNAL CONTROL ----------------
    public void SetExternalDirection(Vector2 direction)
    {
        externalDirection = direction;
    }

    public void ClearExternalDirection()
    {
        externalDirection = Vector2.zero;
    }

    // ---------------- STATES ----------------
    public void Idle()
    {
        enabled = false;
        spriteRenderer.enabled = true;
        circleCollider.enabled = true;
        deathSequence.enabled = false;
        movement.ResetPosition();
        movement.canMove = false;
        gameObject.SetActive(true);
    }

    public void ResetState()
    {
        enabled = true;
        spriteRenderer.enabled = true;
        circleCollider.enabled = true;
        deathSequence.enabled = false;
        movement.canMove = true;
        movement.ResetState();

        keyboardDirection = Vector2.zero;
        externalDirection = Vector2.zero;

        gameObject.SetActive(true);
    }

    public void DeathSequence()
    {
        enabled = false;
        spriteRenderer.enabled = false;
        circleCollider.enabled = false;
        movement.canMove = false;
        movement.enabled = false;
        deathSequence.enabled = true;
        deathSequence.Restart();
    }
}