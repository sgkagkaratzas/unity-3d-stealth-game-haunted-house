using MyGame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyGame.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Input Settings")]
        public InputAction MoveAction;
        public InputAction InteractAction;

        [Header("Movement Settings")]
        public float walkSpeed = 10f;
        public float turnSpeed = 20f;

        [Header("Speed Boost Settings")]
        public float boostDuration = 3.0f;
        public float boostMultiplier = 2.0f;

        [Header("Audio Settings")]
        public AudioClip keyPickupSound;
        public AudioClip doorOpenSound;

        // Hiding system
        public bool IsHidden { get; private set; } = false;

        private List<string> m_OwnedKeys = new List<string>();

        Rigidbody m_Rigidbody;
        Animator m_Animator;
        Vector3 m_Movement;
        Quaternion m_Rotation = Quaternion.identity;
        AudioSource m_AudioSource;

        private float m_BaseWalkSpeed;
        private Coroutine m_BoostCoroutine;

        private void Awake()
        {
            // Setup default input bindings when none are assigned in the Inspector
            if (MoveAction == null || MoveAction.bindings.Count == 0)
            {
                MoveAction = new InputAction("Move");
                MoveAction.AddBinding("<Gamepad>/leftStick");
                MoveAction.AddBinding("<Gamepad>/dpad");
                MoveAction.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/w")
                    .With("Down", "<Keyboard>/s")
                    .With("Left", "<Keyboard>/a")
                    .With("Right", "<Keyboard>/d");
                MoveAction.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/upArrow")
                    .With("Down", "<Keyboard>/downArrow")
                    .With("Left", "<Keyboard>/leftArrow")
                    .With("Right", "<Keyboard>/rightArrow");
            }

            if (InteractAction == null || InteractAction.bindings.Count == 0)
            {
                // Interact uses common controller and keyboard keys
                InteractAction = new InputAction("Interact");
                InteractAction.AddBinding("<Gamepad>/buttonWest");
                InteractAction.AddBinding("<Gamepad>/buttonSouth");
                InteractAction.AddBinding("<Keyboard>/e");
                InteractAction.AddBinding("<Keyboard>/enter");
            }
        }

        void OnEnable()
        {
            MoveAction.Enable();
            InteractAction.Enable();
        }

        void OnDisable()
        {
            MoveAction.Disable();
            InteractAction.Disable();
        }

        void Start()
        {
            // Reset default game state on level start:
            // - Ensure time is running
            Time.timeScale = 1.0f;

            // - UNLOCK cursor so mouse works generally in game
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;

            // - Clear hidden state
            IsHidden = false;

            m_Rigidbody = GetComponent<Rigidbody>();
            m_Animator = GetComponentInChildren<Animator>();
            m_AudioSource = GetComponent<AudioSource>();

            m_BaseWalkSpeed = walkSpeed;
        }

        void FixedUpdate()
        {
            // If player is hidden or game is paused (UI open), ignore input
            if (IsHidden || Time.timeScale == 0f) return;

            Vector2 moveInput = MoveAction.ReadValue<Vector2>();

            float horizontal = moveInput.x;
            float vertical = moveInput.y;

            m_Movement.Set(horizontal, 0f, vertical);
            m_Movement.Normalize();

            bool hasMovementInput = m_Movement.magnitude > 0.1f;

            m_Animator.SetBool("IsWalking", hasMovementInput);

            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
            m_Rotation = Quaternion.LookRotation(desiredForward);

            m_Rigidbody.MoveRotation(m_Rotation);

            m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * walkSpeed * Time.deltaTime);

            if (hasMovementInput)
            {
                if (!m_AudioSource.isPlaying) m_AudioSource.Play();
            }
            else
            {
                m_AudioSource.Stop();
            }
        }

        // Hiding logic
        public void SetHidingState(bool hidden, Vector3 hidingPos)
        {
            IsHidden = hidden;

            if (hidden)
            {
                m_Rigidbody.linearVelocity = Vector3.zero;
                m_Rigidbody.isKinematic = true;
                transform.position = hidingPos;
                ForceIdle();
            }
            else
            {
                m_Rigidbody.isKinematic = false;
            }
        }

        public void ForceIdle()
        {
            if (m_Animator != null) m_Animator.SetBool("IsWalking", false);
            if (m_AudioSource != null) m_AudioSource.Stop();
        }

        // Key logic
        public void AddKey(string keyName)
        {
            m_OwnedKeys.Add(keyName);
            if (keyPickupSound != null && m_AudioSource != null) m_AudioSource.PlayOneShot(keyPickupSound);

            KeyDisplay display = FindFirstObjectByType<KeyDisplay>();
            if (display != null) display.RemoveKeyIcon();
        }

        public bool OwnKey(string keyName)
        {
            return m_OwnedKeys.Contains(keyName);
        }

        public void PlayDoorSound()
        {
            if (doorOpenSound != null && m_AudioSource != null) m_AudioSource.PlayOneShot(doorOpenSound);
        }

        // Powerup logic
        public void HandleSpeedBoost()
        {
            if (m_BoostCoroutine != null) StopCoroutine(m_BoostCoroutine);
            m_BoostCoroutine = StartCoroutine(SpeedBoostRoutine());
        }

        private IEnumerator SpeedBoostRoutine()
        {
            walkSpeed = m_BaseWalkSpeed * boostMultiplier;
            yield return new WaitForSeconds(boostDuration);
            walkSpeed = m_BaseWalkSpeed;
            m_BoostCoroutine = null;
        }
    }
}
