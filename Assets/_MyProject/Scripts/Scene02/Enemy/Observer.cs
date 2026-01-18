using UnityEngine;
using MyGame.Scene02.System;

namespace MyGame.Scene02.Enemy
{
    public class Observer : MonoBehaviour
    {
        public Transform player;
        public GameEnding gameEnding;
        bool m_IsPlayerInRange;

        void OnTriggerEnter(Collider other)
        {
            if (other.transform == player)
            {
                m_IsPlayerInRange = true;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.transform == player)
            {
                m_IsPlayerInRange = false;
            }
        }

        void Update()
        {
            if (m_IsPlayerInRange)
            {
                Vector3 direction = player.position - transform.position + Vector3.up;
                Ray ray = new Ray(transform.position, direction);
                RaycastHit raycastHit;

                if (Physics.Raycast(ray, out raycastHit))
                {
                    if (raycastHit.collider.transform == player)
                    {
                        Debug.Log("Player was caught!");
                        gameEnding.CaughtPlayer();
                    }
                }
            }
        }
    }
}
