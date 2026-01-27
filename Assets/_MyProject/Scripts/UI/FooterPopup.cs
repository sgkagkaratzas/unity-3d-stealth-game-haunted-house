using UnityEngine;
using UnityEngine.UIElements;

namespace MyGame.UI
{
    public class FooterPopup : MonoBehaviour
    {
        private UIDocument m_UIDocument;
        private VisualElement m_HelpPopup;
        private Button m_HelpButton;
        private Button m_CloseButton;

        private void Awake()
        {
            m_UIDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (m_UIDocument == null) return;

            var root = m_UIDocument.rootVisualElement;

            // 1. Find the elements by the names you set in UI Builder
            m_HelpPopup = root.Q<VisualElement>("HelpPopup");
            m_HelpButton = root.Q<Button>("HelpButton");
            m_CloseButton = root.Q<Button>("CloseHelpButton");

            // 2. Setup Open Button
            if (m_HelpButton != null)
            {
                m_HelpButton.clicked += OpenPopup;
            }

            // 3. Setup Close Button
            if (m_CloseButton != null)
            {
                m_CloseButton.clicked += ClosePopup;
            }
        }

        private void OpenPopup()
        {
            if (m_HelpPopup != null)
            {
                // Show the window
                m_HelpPopup.style.display = DisplayStyle.Flex;

                // Optional: Pause the game while reading
                Time.timeScale = 0f;
            }
        }

        private void ClosePopup()
        {
            if (m_HelpPopup != null)
            {
                // Hide the window
                m_HelpPopup.style.display = DisplayStyle.None;

                // Resume the game
                Time.timeScale = 1f;
            }
        }
    }
}