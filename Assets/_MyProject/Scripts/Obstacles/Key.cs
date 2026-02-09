using MyGame.Enemy;
using MyGame.Player;
using UnityEngine;

namespace MyGame.Obstacles
{
    public class Key : MonoBehaviour
    {
        public string KeyName; // e.g., "key0001"

        [Header("Security Settings")]
        [Tooltip("OPTIONAL: Drag the Enemy (GuardianPatrol) here. If empty, no alarm will ring.")]
        public GuardianPatrol guardianEnemy;

        // Tracks how many times the student failed THIS specific key
        private int _failureCount = 0;
        public int FailureCount => _failureCount;

        // Memory of the last question asked (ID)
        public string LastQuestionID { get; set; } = "";

        private void OnTriggerEnter(Collider other)
        {
            PlayerMovement player = other.gameObject.GetComponent<PlayerMovement>();
            if (player == null) return;

            KeyQuizUI quizUI = FindFirstObjectByType<KeyQuizUI>();

            if (quizUI != null)
            {
                GetComponent<Collider>().enabled = false;
                quizUI.ShowQuestion(this, player);
            }
        }

        // Called by UI when player answers WRONG
        public void RegisterFailure()
        {
            _failureCount++;

            // --- THE NEW ALARM LOGIC ---
            if (guardianEnemy != null)
            {
                // Tell the enemy to run to THIS key's position
                guardianEnemy.AlertToPosition(transform.position);
            }
        }

        public void EnableInteraction()
        {
            GetComponent<Collider>().enabled = true;
        }

        public void ResolveKey(bool isMercy)
        {
            // If it is a Mercy pickup AND there is an enemy coming...
            if (isMercy && guardianEnemy != null)
            {
                // 1. Disable the collider so the player can't click it again
                GetComponent<Collider>().enabled = false;

                // 2. Keep the visual mesh ON so the ghost can "investigate" it
                // 3. Start a timer to destroy it AFTER the ghost leaves
                StartCoroutine(DestroyAfterGhostLeaves());
            }
            else
            {
                // Normal Success: Pick it up immediately
                Destroy(gameObject);
            }
        }

        private System.Collections.IEnumerator DestroyAfterGhostLeaves()
        {
            // Wait time = Enemy Reaction (5s) + Run Time (3s)
            yield return new WaitForSeconds(8.0f);
            Destroy(gameObject);
        }
    }
}