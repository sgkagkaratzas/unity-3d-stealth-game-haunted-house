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
        public float investigationWaitTime = 4f; // Time spent looking around at the key

        [Tooltip("How long the enemy waits AFTER the alarm rings before starting to run. Must be higher than your UI delay!")]
        public float alarmReactionTime = 5.0f; // NEW: The "Boot Up" delay

        // Internal Variables
        private Rigidbody m_RigidBody;
        private int m_CurrentWaypointIndex;
        private bool m_IsInvestigating = false;
        private bool m_IsWaiting = false;
        private Vector3 m_TargetPosition;

        private VisualHuntManager _visualManager; // 1. Add Variable

        void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_RigidBody.isKinematic = true;

            if (waypoints.Length > 0)
            {
                m_TargetPosition = waypoints[0].position;
            }

            _visualManager = FindFirstObjectByType<VisualHuntManager>(); // 2. Find it
        }

        // --- PUBLIC METHOD: CALLED BY THE KEY SCRIPT ---
        public void AlertToPosition(Vector3 position)
        {
            // 1. Stop patrol logic immediately
            StopAllCoroutines();

            // 2. Trigger Visuals ON
            if (_visualManager != null) _visualManager.StartHunt();

            // 3. Start the "Reaction" sequence
            StartCoroutine(PrepareToInvestigate(position));
        }

        // --- NEW LOGIC: THE DELAY ---
        IEnumerator PrepareToInvestigate(Vector3 targetPos)
        {
            m_IsWaiting = true; // Stop moving

            Debug.Log("Guardian: Alarm heard... Calculating route...");

            // WAIT here while the player finishes the quiz UI and starts running
            yield return new WaitForSeconds(alarmReactionTime);

            // NOW we run
            m_IsWaiting = false;
            m_IsInvestigating = true;
            m_TargetPosition = targetPos;

            Debug.Log("Guardian: Target acquired! Sprinting now.");
        }

        void FixedUpdate()
        {
            if (m_IsWaiting) return;

            Vector3 currentToTarget = m_TargetPosition - m_RigidBody.position;
            currentToTarget.y = 0;

            float distance = currentToTarget.magnitude;

            if (distance < 0.5f)
            {
                if (m_IsInvestigating)
                {
                    StartCoroutine(FinishInvestigation());
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

            float currentSpeed = m_IsInvestigating ? runSpeed : patrolSpeed;

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
            m_IsWaiting = true;
            Debug.Log("Guardian: Arrived at key. Looking around...");

            yield return new WaitForSeconds(investigationWaitTime);

            Debug.Log("Guardian: Nothing here. Returning to patrol.");

            // 4. Trigger Visuals OFF
            if (_visualManager != null) _visualManager.EndHunt();

            m_IsInvestigating = false;

            if (waypoints.Length > 0)
            {
                m_TargetPosition = waypoints[m_CurrentWaypointIndex].position;
            }

            m_IsWaiting = false;
        }
    }
}