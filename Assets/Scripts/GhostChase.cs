using System.Linq;
using UnityEngine;

public class GhostChase : GhostBehavior
{
    private void OnDisable()
    {
        ghost.scatter.Enable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled || ghost.frightened.enabled) 
            return;

        var node = other.GetComponent<Node>();
        if (node == null) 
            return;

        // 1) Block immediate reversal
        Vector2 reverseDir = -ghost.movement.direction;
        var options = node.availableDirections
                          .Where(d => d != reverseDir)
                          .ToList();

        // 2) If no other choice, allow reversing
        if (options.Count == 0)
            options = node.availableDirections.ToList();

        // 3) Pick the one that minimizes squared distance to Pac-Man
        Vector2 best = options[0];
        float bestDist = float.MaxValue;
        foreach (var dir in options)
        {
            Vector3 testPos = transform.position + new Vector3(dir.x, dir.y);
            float dist = (ghost.target.position - testPos).sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = dir;
            }
        }

        ghost.movement.SetDirection(best);
    }
}
