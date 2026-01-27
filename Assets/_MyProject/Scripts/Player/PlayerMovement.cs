using MyGame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyGame.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        public InputAction MoveAction;

        public float walkSpeed = 1.0f;
        public float turnSpeed = 20f;

        // Configuration for the speed boost
        [Header("Speed Boost Settings")]
        public float boostDuration = 3.0f;   // How long the boost lasts (in seconds)
        public float boostMultiplier = 2.0f; // How much faster you go (2.0 = double speed)

        // Slot to drag your sound file into
        [Header("Audio Settings")]
        public AudioClip keyPickupSound;
        public AudioClip doorOpenSound;

        private List<string> m_OwnedKeys = new List<string>();

        Rigidbody m_Rigidbody;
        Animator m_Animator;
        Vector3 m_Movement;
        Quaternion m_Rotation = Quaternion.identity;
        AudioSource m_AudioSource;

        // To store the normal speed and track the active timer
        private float m_BaseWalkSpeed;
        private Coroutine m_BoostCoroutine;

        void Start()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Animator = GetComponentInChildren<Animator>();
            m_AudioSource = GetComponent<AudioSource>();

            // Remember the starting speed so we can reset to it later
            m_BaseWalkSpeed = walkSpeed;

            MoveAction.Enable();
        }

        void FixedUpdate()
        {
            Vector2 pos = MoveAction.ReadValue<Vector2>();

            float horizontal = pos.x;
            float vertical = pos.y;

            m_Movement.Set(horizontal, 0f, vertical);
            m_Movement.Normalize();

            bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
            bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
            bool isWalking = hasHorizontalInput || hasVerticalInput;

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

        public void AddKey(string keyName)
        {
            m_OwnedKeys.Add(keyName);

            // Play the sound once
            if (keyPickupSound != null && m_AudioSource != null)
            {
                m_AudioSource.PlayOneShot(keyPickupSound);
            }

            // Find the UI script and tell it to remove an icon
            KeyDisplay display = FindFirstObjectByType<KeyDisplay>();

            if (display != null)
            {
                display.RemoveKeyIcon();
            }

            // Trigger the speed boost when a key is added
            HandleSpeedBoost();
        }

        public bool OwnKey(string keyName)
        {
            return m_OwnedKeys.Contains(keyName);
        }

        // The Door script will call this function before it destroys itself
        public void PlayDoorSound()
        {
            if (doorOpenSound != null && m_AudioSource != null)
            {
                m_AudioSource.PlayOneShot(doorOpenSound);
            }
        }

        // Helper method to manage the boost logic
        private void HandleSpeedBoost()
        {
            // If a boost is already active, stop it so we can restart the timer
            if (m_BoostCoroutine != null)
            {
                StopCoroutine(m_BoostCoroutine);
            }

            // Start the new boost routine
            m_BoostCoroutine = StartCoroutine(SpeedBoostRoutine());
        }

        // The Coroutine that handles the timing
        private IEnumerator SpeedBoostRoutine()
        {
            // 1. Increase speed
            walkSpeed = m_BaseWalkSpeed * boostMultiplier;
            Debug.Log("Speed Boost Activated! Current Speed: " + walkSpeed);

            // 2. Wait for X seconds
            yield return new WaitForSeconds(boostDuration);

            // 3. Reset speed
            walkSpeed = m_BaseWalkSpeed;
            m_BoostCoroutine = null;
            Debug.Log("Speed Boost Ended. Reset to: " + walkSpeed);
        }
    }
}