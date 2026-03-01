using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Enemy
{
    [System.Serializable]
    public struct InvestigationZone
    {
        public string keyID;
        public SearchNode[] specificSearchNodes;
    }

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
        public float alarmReactionTime = 3.0f;
        public List<InvestigationZone> investigationZones;

        private Rigidbody m_RigidBody;
        private int m_CurrentWaypointIndex;
        private bool m_IsInvestigating = false;
        private bool m_IsWaiting = false;
        private Vector3 m_TargetPosition;

        private VisualHuntManager _visualManager;

        void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_RigidBody.isKinematic = true;

            if (waypoints.Length > 0)
                m_TargetPosition = waypoints[0].position;

            _visualManager = FindFirstObjectByType<VisualHuntManager>();
        }

        public void AlertToPosition(Vector3 position, string keyID)
        {
            // BUG 1 FIX: Find the zone BEFORE starting the UI visuals
            InvestigationZone targetZone = investigationZones.Find(z => z.keyID == keyID);

            // If the ID isn't in our list OR has no nodes, don't hunt!
            if (string.IsNullOrEmpty(targetZone.keyID) || targetZone.specificSearchNodes == null || targetZone.specificSearchNodes.Length == 0)
            {
                Debug.Log($"Guardian: Ignoring alarm for {keyID}. No nodes assigned for this key.");
                return;
            }

            StopAllCoroutines();
            if (_visualManager != null) _visualManager.StartHunt();
            StartCoroutine(ExecuteSmartHunt(targetZone));
        }

        IEnumerator ExecuteSmartHunt(InvestigationZone zone)
        {
            m_IsWaiting = true;
            Debug.Log($"Guardian: Forced hunt for {zone.keyID}. Heading to entrance node: {zone.specificSearchNodes[0].name}");

            yield return new WaitForSeconds(alarmReactionTime);
            m_IsInvestigating = true;

            SearchNode startNode = FindClosestNode(transform.position);
            SearchNode entranceNode = zone.specificSearchNodes[0]; // The first point in your Inspector list

            if (startNode != null && entranceNode != null)
            {
                List<SearchNode> pathToEntrance = AStarPathfinder.FindPath(startNode, entranceNode);

                if (pathToEntrance != null)
                {
                    foreach (SearchNode pathNode in pathToEntrance)
                    {
                        m_TargetPosition = pathNode.transform.position;
                        m_IsWaiting = false;
                        while (Vector3.Distance(transform.position, m_TargetPosition) > 0.7f) yield return null;
                    }
                }
            }

            // Start from index 1 because index 0 was the entrance
            for (int i = 1; i < zone.specificSearchNodes.Length; i++)
            {
                SearchNode nextPoint = zone.specificSearchNodes[i];
                m_TargetPosition = nextPoint.transform.position;
                m_IsWaiting = false;

                while (Vector3.Distance(transform.position, m_TargetPosition) > 0.7f) yield return null;

                m_IsWaiting = true;
                yield return new WaitForSeconds(investigationWaitTime);
            }

            Debug.Log("Guardian: Forced search finished.");
            if (_visualManager != null) _visualManager.EndHunt();

            m_IsInvestigating = false;
            if (waypoints.Length > 0) m_TargetPosition = waypoints[m_CurrentWaypointIndex].position;
            m_IsWaiting = false;
        }

        private SearchNode FindClosestNode(Vector3 position)
        {
            SearchNode[] allNodes = FindObjectsByType<SearchNode>(FindObjectsSortMode.None);
            SearchNode closest = null;
            float minDist = float.MaxValue;
            foreach (var node in allNodes)
            {
                float d = Vector3.Distance(position, node.transform.position);
                if (d < minDist) { minDist = d; closest = node; }
            }
            return closest;
        }

        void FixedUpdate()
        {
            if (m_IsWaiting) return;

            Vector3 dir = m_TargetPosition - m_RigidBody.position;
            dir.y = 0;
            if (dir.magnitude < 0.6f)
            {
                if (!m_IsInvestigating) StartCoroutine(NextWaypoint());
            }
            else
            {
                MoveCharacter(dir);
            }
        }

        void MoveCharacter(Vector3 direction)
        {
            float speed = m_IsInvestigating ? runSpeed : patrolSpeed;
            m_RigidBody.MoveRotation(Quaternion.Slerp(m_RigidBody.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.fixedDeltaTime));
            m_RigidBody.MovePosition(m_RigidBody.position + direction.normalized * speed * Time.fixedDeltaTime);
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
    }
}
