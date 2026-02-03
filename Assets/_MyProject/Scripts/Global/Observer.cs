using UnityEngine;
using MyGame.Player;

namespace MyGame.Global
{
    public class Observer : MonoBehaviour
    {
        public Transform player;
        public GameEnding gameEnding;

        bool m_IsPlayerInRange;

        void Start()
        {
            // Auto-find references to avoid "NullReference" crashes
            PlayerMovement playerScript = FindFirstObjectByType<PlayerMovement>();
            if (playerScript != null) player = playerScript.transform;

            gameEnding = FindFirstObjectByType<GameEnding>();

            if (player == null) Debug.LogError("Observer Error: Can't find Player object.");
            if (gameEnding == null) Debug.LogError("Observer Error: Can't find GameEnding script.");
        }

        void OnTriggerEnter(Collider other)
        {
            // Use root comparison to be safe against child colliders
            if (player != null && other.transform.root == player.root)
            {
                m_IsPlayerInRange = true;
                Debug.Log("Player entered vision cone trigger.");
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (player != null && other.transform.root == player.root)
            {
                m_IsPlayerInRange = false;
                Debug.Log("Running away from vision cone.");
            }
        }

        void Update()
        {
            if (player == null || gameEnding == null) return;

            if (m_IsPlayerInRange)
            {
                // Calculate direction to player's chest (Vector3.up raises the target from feet to chest)
                Vector3 direction = (player.position + Vector3.up) - transform.position;

                Ray ray = new Ray(transform.position, direction);
                RaycastHit raycastHit;

                if (Physics.Raycast(ray, out raycastHit))
                {
                    // Check if we hit the player (or any part of the player)
                    if (raycastHit.collider.transform.root == player.root)
                    {
                        Debug.Log("Caught, calling GameEnding...");
                        gameEnding.CaughtPlayer();
                    }
                }
            }
        }
    }
}