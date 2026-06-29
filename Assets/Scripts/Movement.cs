using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    public float speedMultiplier = 1f;
    public Vector2 initialDirection;
    public LayerMask obstacleLayer;

    [Header("Tunnel Ports")]
    public Transform leftPortal;
    public Transform rightPortal;

    [HideInInspector] public Rigidbody2D rb;
    public Vector2 direction { get; private set; }
    public Vector2 nextDirection { get; private set; }
    public Vector3 startingPosition { get; private set; }

    public bool canMove = false;
    public bool isPlayerControlled = false;

    // Only used by Pacman if isPlayerControlled is true
    public List<Vector2> nextDirections = new List<Vector2>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startingPosition = transform.position;
    }

    private void Start()
    {
        ResetState();
    }

    public void ResetPosition()
    {
        transform.position = startingPosition;
    }

    public void ResetState()
    {
        speedMultiplier = 1f;
        direction = initialDirection;
        nextDirection = Vector2.zero;

        if (isPlayerControlled)
            nextDirections.Clear();

        transform.position = startingPosition;
        rb.isKinematic = false;
        enabled = true;
    }

    private void Update()
    {
        if (!canMove) return;

        if (isPlayerControlled)
        {
            foreach (Vector2 dir in nextDirections)
            {
                if (!Occupied(dir))
                {
                    SetDirection(dir, forced: true);
                    break;
                }
            }
        }
        else
        {
            if (nextDirection != Vector2.zero)
                SetDirection(nextDirection);
        }
    }

    private void FixedUpdate()
    {
        if (!canMove) return;

        Vector2 pos = rb.position;
        float effectiveSpeed = speed * speedMultiplier * (1 + (GameManager.Instance.Round * 0.25f));
        Vector2 move = effectiveSpeed * Time.fixedDeltaTime * direction;
        Vector2 newPos = pos + move;

        if (leftPortal != null && rightPortal != null)
        {
            if (newPos.x < leftPortal.position.x)
            {
                Vector3 dest = rightPortal.position;
                dest.z = transform.position.z;
                rb.position = dest;
                transform.position = dest;
                return;
            }

            if (newPos.x > rightPortal.position.x)
            {
                Vector3 dest = leftPortal.position;
                dest.z = transform.position.z;
                rb.position = dest;
                transform.position = dest;
                return;
            }
        }

        rb.MovePosition(newPos);
    }

    public void SetDirection(Vector2 dir, bool forced = false)
    {
        if (!canMove)
        {
            nextDirection = dir;
            return;
        }

        if (forced || !Occupied(dir))
        {
            direction = dir;
            nextDirection = Vector2.zero;
        }
        else
        {
            nextDirection = dir;
        }
    }

    public bool Occupied(Vector2 dir)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.75f,
                                             0f, dir, 1.5f, obstacleLayer);
        return hit.collider != null;
    }
}
