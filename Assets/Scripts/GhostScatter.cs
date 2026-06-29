using System.Linq;
using UnityEngine;

public class GhostScatter : GhostBehavior
{
    private void OnDisable()
    {
        ghost.chase.Enable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var node = other.GetComponent<Node>();
        if (node == null || !enabled || ghost.frightened.enabled)
            return;

        // 1) Exclude the direct reverse of current movement
        Vector2 reverse = -ghost.movement.direction;
        var options = node.availableDirections
                          .Where(d => d != reverse)
                          .ToList();

        // 2) If that leaves no options, allow reversal
        if (options.Count == 0)
            options = node.availableDirections.ToList();

        // 3) Pick one at random
        Vector2 chosen = options[Random.Range(0, options.Count)];
        ghost.movement.SetDirection(chosen);
    }
}
