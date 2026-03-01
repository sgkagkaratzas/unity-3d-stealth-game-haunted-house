using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Enemy
{
    public class SearchNode : MonoBehaviour
    {
        [Tooltip("Drag the neighboring SearchNode GameObjects here to connect them.")]
        public List<SearchNode> neighbors = new List<SearchNode>();

        // This draws colored lines in the Unity Scene View so you can easily see your node network!
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position, 0.2f);

            if (neighbors != null)
            {
                Gizmos.color = Color.yellow;
                foreach (var neighbor in neighbors)
                {
                    if (neighbor != null)
                        Gizmos.DrawLine(transform.position, neighbor.transform.position);
                }
            }
        }
    }
}
