using System.Linq;
using UnityEngine;

public class GhostFrightened : GhostBehavior
{
    public SpriteRenderer body;
    public SpriteRenderer eyes;
    public SpriteRenderer blue;
    public SpriteRenderer white;

    private bool eaten;

    public override void Enable(float duration)
    {
        base.Enable(duration);

        body.enabled = false;
        eyes.enabled = false;
        blue.enabled = true;
        white.enabled = false;

        Invoke(nameof(Flash), duration / 2f);
    }

    public override void Disable()
    {
        base.Disable();

        body.enabled = true;
        eyes.enabled = true;
        blue.enabled = false;
        white.enabled = false;
    }

    private void Eaten()
    {
        eaten = true;
        ghost.SetPosition(ghost.home.inside.position);
        ghost.home.Enable(duration);

        body.enabled = false;
        eyes.enabled = true;
        blue.enabled = false;
        white.enabled = false;
    }

    private void Flash()
    {
        if (!eaten)
        {
            blue.enabled = false;
            white.enabled = true;
            white.GetComponent<AnimatedSprite>().Restart();
        }
    }

    private void OnEnable()
    {
        blue.GetComponent<AnimatedSprite>().Restart();
        ghost.movement.speedMultiplier = 0.5f;
        eaten = false;
    }

    private void OnDisable()
    {
        ghost.movement.speedMultiplier = 1f;
        eaten = false;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled) return;

        var node = other.GetComponent<Node>();
        if (node == null) return;

        // Prevent immediate U-turn
        Vector2 reverseDir = -ghost.movement.direction;
        var options = node.availableDirections
                          .Where(d => d != reverseDir)
                          .ToList();

        if (options.Count == 0)
            options = node.availableDirections.ToList();

        // Choose the one that maximizes distance from Pac-Man
        Vector2 best = options[0];
        float bestDist = float.MinValue;
        foreach (var dir in options)
        {
            Vector3 testPos = transform.position + new Vector3(dir.x, dir.y);
            float dist = (ghost.target.position - testPos).sqrMagnitude;
            if (dist > bestDist)
            {
                bestDist = dist;
                best = dir;
            }
        }

        ghost.movement.SetDirection(best);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pacman"))
        {
            if (enabled)
            {
                Eaten();
            }
        }
    }

}
