using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace MyGame.UI
{
    public class FooterPopup : MonoBehaviour
    {
        private UIDocument m_UIDocument;
        private VisualElement m_HelpPopup;
        private Button m_HelpButton;
        private Button m_CloseButton;

        // --- NEW: Input Actions for Controller Support ---
        public InputAction toggleHelpAction; // 'H' or 'Select'
        public InputAction cancelAction;     // 'Esc' or 'B'

        private void Awake()
        {
            m_UIDocument = GetComponent<UIDocument>();

            // 1. Setup Toggle (H or Xbox View/Select)
            if (toggleHelpAction == null || toggleHelpAction.bindings.Count == 0)
            {
                toggleHelpAction = new InputAction("ToggleHelp");
                toggleHelpAction.AddBinding("<Keyboard>/h");
                toggleHelpAction.AddBinding("<Gamepad>/select"); // The "View" button (Small button left of center)
            }

            // 2. Setup Cancel (Esc or Xbox B) - to close the popup easily
            if (cancelAction == null || cancelAction.bindings.Count == 0)
            {
                cancelAction = new InputAction("Cancel");
                cancelAction.AddBinding("<Keyboard>/escape");
                cancelAction.AddBinding("<Gamepad>/buttonEast"); // Xbox B
            }
        }

        private void OnEnable()
        {
            toggleHelpAction.Enable();
            cancelAction.Enable();

            if (m_UIDocument == null) return;

            var root = m_UIDocument.rootVisualElement;

            m_HelpPopup = root.Q<VisualElement>("HelpPopup");
            m_HelpButton = root.Q<Button>("HelpButton");
            m_CloseButton = root.Q<Button>("CloseHelpButton");

            if (m_HelpButton != null)
            {
                m_HelpButton.focusable = false; // Prevent tab/controller focus on the small '?' icon during gameplay
                m_HelpButton.clicked += TogglePopup;
            }

            if (m_CloseButton != null)
            {
                m_CloseButton.clicked += ClosePopup;
            }
        }

        private void OnDisable()
        {
            toggleHelpAction.Disable();
            cancelAction.Disable();
        }

        private void Update()
        {
            // 1. Toggle Button Pressed (H or Select)
            if (toggleHelpAction.WasPerformedThisFrame())
            {
                TogglePopup();
            }

            // 2. Cancel Button Pressed (B or Esc) - Only if popup is open
            if (m_HelpPopup != null && m_HelpPopup.style.display == DisplayStyle.Flex)
            {
                if (cancelAction.WasPerformedThisFrame())
                {
                    ClosePopup();
                }
            }
        }

        private void TogglePopup()
        {
            if (m_HelpPopup == null) return;

            bool isVisible = m_HelpPopup.style.display == DisplayStyle.Flex;

            if (isVisible)
                ClosePopup();
            else
                OpenPopup();
        }

        private void OpenPopup()
        {
            if (m_HelpPopup != null)
            {
                m_HelpPopup.style.display = DisplayStyle.Flex;
                Time.timeScale = 0f;

                // --- CRITICAL FOR CONTROLLER ---
                // Focus the Close button so the player can press 'A' (Submit) immediately
                if (m_CloseButton != null)
                {
                    m_CloseButton.schedule.Execute(() => m_CloseButton.Focus());
                }
            }
        }

        private void ClosePopup()
        {
            if (m_HelpPopup != null)
            {
                m_HelpPopup.style.display = DisplayStyle.None;
                Time.timeScale = 1f;
            }
        }
    }
}
