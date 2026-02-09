using MyGame.Enemy;
using MyGame.Player;
using UnityEngine;

namespace MyGame.Obstacles
{
    public class Key : MonoBehaviour
    {
        public string KeyName;

        [Header("Security Settings")]
        [Tooltip("OPTIONAL: Drag the Enemy (GuardianPatrol) here. If empty, no alarm will ring.")]
        public GuardianPatrol guardianEnemy;

        private int _failureCount = 0;
        public int FailureCount => _failureCount;

        public string LastQuestionID { get; set; } = "";

        private void OnTriggerEnter(Collider other)
        {
            PlayerMovement player = other.gameObject.GetComponent<PlayerMovement>();
            if (player == null) return;

            KeyQuizUI quizUI = FindFirstObjectByType<KeyQuizUI>();

            if (quizUI != null)
            {
                // Disable collider to prevent re-trigger while question UI is open
                GetComponent<Collider>().enabled = false;
                quizUI.ShowQuestion(this, player);
            }
        }

        public void RegisterFailure()
        {
            _failureCount++;

            if (guardianEnemy != null)
            {
                guardianEnemy.AlertToPosition(transform.position);
            }
        }

        public void EnableInteraction()
        {
            GetComponent<Collider>().enabled = true;
        }

        public void ResolveKey(bool isMercy)
        {
            if (isMercy && guardianEnemy != null)
            {
                GetComponent<Collider>().enabled = false;
                StartCoroutine(DestroyAfterGhostLeaves());
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private System.Collections.IEnumerator DestroyAfterGhostLeaves()
        {
            yield return new WaitForSeconds(8.0f);
            Destroy(gameObject);
        }
    }
}