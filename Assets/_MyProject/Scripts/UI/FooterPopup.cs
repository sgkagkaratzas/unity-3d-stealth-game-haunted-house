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

        public InputAction toggleHelpAction;
        public InputAction cancelAction;

        private void Awake()
        {
            m_UIDocument = GetComponent<UIDocument>();

            if (toggleHelpAction == null || toggleHelpAction.bindings.Count == 0)
            {
                toggleHelpAction = new InputAction("ToggleHelp");
                toggleHelpAction.AddBinding("<Keyboard>/h");
                toggleHelpAction.AddBinding("<Gamepad>/select");
            }

            if (cancelAction == null || cancelAction.bindings.Count == 0)
            {
                cancelAction = new InputAction("Cancel");
                cancelAction.AddBinding("<Keyboard>/escape");
                cancelAction.AddBinding("<Gamepad>/buttonEast");
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
                m_HelpButton.focusable = false;
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
            if (toggleHelpAction.WasPerformedThisFrame())
            {
                TogglePopup();
            }

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
