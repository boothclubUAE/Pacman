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

    private Vector2 nextDirection = Vector2.zero;

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
        // Step 1: Check for input and queue it as nextDirection
        foreach (var kvp in keyToDirection)
        {
            if (Input.GetKeyDown(kvp.Key))
            {
                nextDirection = kvp.Value;
            }
        }

        // Step 2: Try to apply the queued direction if valid
        if (nextDirection != Vector2.zero && !movement.Occupied(nextDirection))
        {
            movement.SetDirection(nextDirection, forced: true);
            nextDirection = Vector2.zero;
        }

        // Step 3: Rotate Pacman sprite
        if (movement.direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(movement.direction.y, movement.direction.x);
            transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
        }
    }

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
        nextDirection = Vector2.zero;
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
