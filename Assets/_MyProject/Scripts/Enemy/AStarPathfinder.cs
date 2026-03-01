using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MyGame.Enemy
{
    public static class AStarPathfinder
    {
        public static List<SearchNode> FindPath(SearchNode start, SearchNode target)
        {
            if (start == target) return new List<SearchNode> { target };

            List<SearchNode> openSet = new List<SearchNode> { start };
            HashSet<SearchNode> closedSet = new HashSet<SearchNode>();

            Dictionary<SearchNode, SearchNode> cameFrom = new Dictionary<SearchNode, SearchNode>();

            // gScore: Cost from start along best known path
            Dictionary<SearchNode, float> gScore = new Dictionary<SearchNode, float> { { start, 0 } };

            // fScore: Estimated total cost from start to goal
            Dictionary<SearchNode, float> fScore = new Dictionary<SearchNode, float> { { start, Heuristic(start, target) } };

            while (openSet.Count > 0)
            {
                // Get the node with the lowest fScore
                SearchNode current = openSet.OrderBy(n => fScore.ContainsKey(n) ? fScore[n] : float.MaxValue).First();

                if (current == target)
                    return ReconstructPath(cameFrom, current);

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (SearchNode neighbor in current.neighbors)
                {
                    if (neighbor == null || closedSet.Contains(neighbor)) continue;

                    float tentativeGScore = gScore[current] + Vector3.Distance(current.transform.position, neighbor.transform.position);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                    else if (tentativeGScore >= (gScore.ContainsKey(neighbor) ? gScore[neighbor] : float.MaxValue))
                    {
                        continue; // This is not a better path
                    }

                    // This path is the best until now. Record it!
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, target);
                }
            }

            return null; // No path found
        }

        // The Heuristic function: straight-line distance to the target
        private static float Heuristic(SearchNode a, SearchNode b)
        {
            return Vector3.Distance(a.transform.position, b.transform.position);
        }

        private static List<SearchNode> ReconstructPath(Dictionary<SearchNode, SearchNode> cameFrom, SearchNode current)
        {
            List<SearchNode> path = new List<SearchNode> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }
    }
}
