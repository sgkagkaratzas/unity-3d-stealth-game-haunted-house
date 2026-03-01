using UnityEngine;
using MyGame.Player;
using MyGame.Logging;

namespace MyGame.Global
{
    public class Observer : MonoBehaviour
    {
        public Transform player;
        public GameEnding gameEnding;

        bool m_IsPlayerInRange;

        void Start()
        {
            // Auto-locate player script and GameEnding to avoid manual wiring in the scene
            PlayerMovement playerScript = FindFirstObjectByType<PlayerMovement>();
            if (playerScript != null) player = playerScript.transform;

            gameEnding = FindFirstObjectByType<GameEnding>();

            if (player == null) Debug.LogError("Observer Error: Can't find Player object.");
            if (gameEnding == null) Debug.LogError("Observer Error: Can't find GameEnding script.");
        }

        void OnTriggerEnter(Collider other)
        {
            if (player != null && other.transform.root == player.root)
            {
                m_IsPlayerInRange = true;
                Debug.Log("Player entered vision cone trigger.");
                LoggerFactory.GetLogger()?.LogEvent(GlobalGameData.PlayerName, GlobalGameData.GameTimer, "Player entered vision cone trigger");
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (player != null && other.transform.root == player.root)
            {
                m_IsPlayerInRange = false;
                Debug.Log("Running away from vision cone.");
                LoggerFactory.GetLogger()?.LogEvent(GlobalGameData.PlayerName, GlobalGameData.GameTimer, "Player exited vision cone trigger");
            }
        }

        void Update()
        {
            if (player == null || gameEnding == null) return;

            if (m_IsPlayerInRange)
            {
                Vector3 direction = (player.position + Vector3.up) - transform.position;

                Ray ray = new Ray(transform.position, direction);
                RaycastHit raycastHit;

                if (Physics.Raycast(ray, out raycastHit))
                {
                    if (raycastHit.collider.transform.root == player.root)
                    {
                        // Only handle the caught event if the GameEnding hasn't already
                        // marked the player as caught to avoid duplicate logs.
                        if (!gameEnding.IsPlayerCaught)
                        {
                            Debug.Log("Caught, calling GameEnding...");
                            LoggerFactory.GetLogger()?.LogEvent(GlobalGameData.PlayerName, GlobalGameData.GameTimer, "Caught by observer");
                            gameEnding.CaughtPlayer();
                        }
                    }
                }
            }
        }
    }
}
