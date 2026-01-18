using UnityEngine;
using UnityEngine.InputSystem;

namespace MyGame.Scene02.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        public InputAction MoveAction;

        public float walkSpeed = 1.0f;
        public float turnSpeed = 20f;

        Rigidbody m_Rigidbody;
        Animator m_Animator; // 1. Added Animator reference
        Vector3 m_Movement;
        Quaternion m_Rotation = Quaternion.identity;
        AudioSource m_AudioSource;

        void Start()
        {
            m_Rigidbody = GetComponent<Rigidbody>();

            // 2. Look for the animator on the child model
            m_Animator = GetComponentInChildren<Animator>();

            m_AudioSource = GetComponent<AudioSource>();

            MoveAction.Enable();
        }

        void FixedUpdate()
        {
            Vector2 pos = MoveAction.ReadValue<Vector2>();

            float horizontal = pos.x;
            float vertical = pos.y;

            m_Movement.Set(horizontal, 0f, vertical);
            m_Movement.Normalize();

            // 3. Logic to determine if we are walking
            bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
            bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
            bool isWalking = hasHorizontalInput || hasVerticalInput;

            // 4. Tell the Animator to play the walk animation
            m_Animator.SetBool("IsWalking", isWalking);

            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
            m_Rotation = Quaternion.LookRotation(desiredForward);

            m_Rigidbody.MoveRotation(m_Rotation);
            m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * walkSpeed * Time.deltaTime);

            if (isWalking)
            {
                if (!m_AudioSource.isPlaying)
                {
                    m_AudioSource.Play();
                }
            }
            else
            {
                m_AudioSource.Stop();
            }
        }
    }
}