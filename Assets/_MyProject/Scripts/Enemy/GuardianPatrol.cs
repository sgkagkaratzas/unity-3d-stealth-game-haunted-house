using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Enemy
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    public class GuardianPatrol : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float patrolSpeed = 2.0f;
        public float runSpeed = 4.5f;
        public float turnSpeed = 20f;

        [Header("Patrol Settings")]
        public Transform[] waypoints;
        public float waitAtWaypointTime = 1f;

        [Header("Investigation Settings")]
        public float investigationWaitTime = 2f;
        public float searchRadius = 1.5f;
        public float alarmReactionTime = 5.0f;

        // This layer mask should include Walls/Obstacles but NOT the Player or Keys
        [Tooltip("Set this to 'Default' or whatever your walls are on.")]
        public LayerMask obstacleLayer = 1;

        // Internal variables
        private Rigidbody m_RigidBody;
        private int m_CurrentWaypointIndex;
        private bool m_IsInvestigating = false;
        private bool m_IsWaiting = false;
        private Vector3 m_TargetPosition;

        private bool m_IsSearchingRoutineRunning = false;
        private bool m_IsSearchingPrecise = false;

        private VisualHuntManager _visualManager;

        void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_RigidBody.isKinematic = true;
            // Default layer mask if not set
            if (obstacleLayer == 0) obstacleLayer = 1;

            if (waypoints.Length > 0)
            {
                m_TargetPosition = waypoints[0].position;
            }

            _visualManager = FindFirstObjectByType<VisualHuntManager>();
        }

        public void AlertToPosition(Vector3 position)
        {
            StopAllCoroutines();
            m_IsSearchingRoutineRunning = false;
            m_IsSearchingPrecise = false;

            if (_visualManager != null) _visualManager.StartHunt();

            StartCoroutine(PrepareToInvestigate(position));
        }

        IEnumerator PrepareToInvestigate(Vector3 targetPos)
        {
            m_IsWaiting = true;
            Debug.Log("Guardian: Alarm heard... Calculating route...");

            yield return new WaitForSeconds(alarmReactionTime);

            m_IsWaiting = false;
            m_IsInvestigating = true;
            m_IsSearchingPrecise = false;
            m_TargetPosition = targetPos;

            Debug.Log("Guardian: Target acquired! Sprinting now.");
        }

        void FixedUpdate()
        {
            if (m_IsWaiting) return;

            Vector3 currentToTarget = m_TargetPosition - m_RigidBody.position;
            currentToTarget.y = 0;

            float distance = currentToTarget.magnitude;

            // Stop distance logic
            if (distance < 0.6f)
            {
                if (m_IsInvestigating)
                {
                    if (!m_IsSearchingRoutineRunning)
                    {
                        StartCoroutine(FinishInvestigation());
                    }
                }
                else
                {
                    StartCoroutine(NextWaypoint());
                }
            }
            else
            {
                MoveCharacter(currentToTarget);
            }
        }

        void MoveCharacter(Vector3 direction)
        {
            if (direction == Vector3.zero) return;

            float currentSpeed = (m_IsInvestigating && !m_IsSearchingPrecise) ? runSpeed : patrolSpeed;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion newRotation = Quaternion.Slerp(m_RigidBody.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
            m_RigidBody.MoveRotation(newRotation);

            Vector3 move = direction.normalized * currentSpeed * Time.fixedDeltaTime;
            m_RigidBody.MovePosition(m_RigidBody.position + move);
        }

        IEnumerator NextWaypoint()
        {
            m_IsWaiting = true;
            yield return new WaitForSeconds(waitAtWaypointTime);

            if (waypoints.Length > 0)
            {
                m_CurrentWaypointIndex = (m_CurrentWaypointIndex + 1) % waypoints.Length;
                m_TargetPosition = waypoints[m_CurrentWaypointIndex].position;
            }

            m_IsWaiting = false;
        }

        IEnumerator FinishInvestigation()
        {
            m_IsSearchingRoutineRunning = true;

            m_IsWaiting = true;
            m_IsSearchingPrecise = true;
            Debug.Log("Guardian: Arrived. Switching to search mode.");

            yield return new WaitForSeconds(1.0f);

            Vector3 keyPosition = transform.position;

            Vector3 bestPointA = FindValidSearchPoint(keyPosition);
            MoveToSearchPoint(bestPointA);

            yield return StartCoroutine(WaitForArrivalOrTimeout(2.5f));

            m_IsWaiting = true;
            yield return new WaitForSeconds(investigationWaitTime);

            Vector3 bestPointB = FindValidSearchPoint(keyPosition);
            MoveToSearchPoint(bestPointB);

            yield return StartCoroutine(WaitForArrivalOrTimeout(2.5f));

            m_IsWaiting = true;
            yield return new WaitForSeconds(investigationWaitTime);

            Debug.Log("Guardian: Nothing here. Returning to patrol.");

            if (_visualManager != null) _visualManager.EndHunt();

            m_IsInvestigating = false;
            m_IsSearchingRoutineRunning = false;
            m_IsSearchingPrecise = false;

            if (waypoints.Length > 0)
            {
                m_TargetPosition = waypoints[m_CurrentWaypointIndex].position;
            }

            m_IsWaiting = false;
        }

        // Helper to set target and unpause movement
        void MoveToSearchPoint(Vector3 pt)
        {
            m_TargetPosition = pt;
            m_IsWaiting = false;
        }

        // Waits until we reach the target OR the timer runs out
        IEnumerator WaitForArrivalOrTimeout(float maxDuration)
        {
            float timer = 0f;
            while (Vector3.Distance(transform.position, m_TargetPosition) > 0.8f && timer < maxDuration)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }

        // Smart algorithm: tries multiple times to find a valid search spot
        // This method randomly selects search points while avoiding obstacles
        Vector3 FindValidSearchPoint(Vector3 center)
        {
            for (int i = 0; i < 10; i++)
            {
                // pick a random spot
                Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                Vector3 candidatePos = center + randomDir * searchRadius;
                // safety check 1: raycast from key to spot
                // Ensure the path to the candidate position is clear of obstacles
                if (Physics.Linecast(center + Vector3.up * 0.5f, candidatePos + Vector3.up * 0.5f, obstacleLayer))
                {
                    continue; // Hit a wall, try next random spot
                }
                // safety check 2: raycast from enemy to spot
                // Check if the enemy can reach the candidate position
                if (Physics.Linecast(transform.position + Vector3.up * 0.5f, candidatePos + Vector3.up * 0.5f, obstacleLayer))
                {
                    continue; // Path blocked, try next
                }
                // if both checks pass, this is valid
                return candidatePos;
            }
            // failed attempts: stay at center
            // If no valid search points are found, return to the original position
            return center;
        }
    }
}