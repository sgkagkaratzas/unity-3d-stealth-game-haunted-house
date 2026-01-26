using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyGame.Menu
{
    namespace StealthGame
    {
        public class Credits : MonoBehaviour
        {
            // Reference to the UI Document component on this GameObject
            private UIDocument m_UIDocument;

            // Reference to the Back Button inside the UI
            private Button m_BackButton;

            [Header("Scene References")]
            [Tooltip("Drag the 'MenuUI' GameObject here from your Hierarchy. This allows us to turn the Main Menu back on.")]
            [SerializeField] private GameObject m_MenuUI;

            private void Awake()
            {
                m_UIDocument = GetComponent<UIDocument>();
            }

            private void OnEnable()
            {
                // Verify the UI Document exists
                if (m_UIDocument == null) return;

                // Find the button named "BackButton" in your UXML hierarchy
                // Note: This searches for the Element Name you set in the Hierarchy, not the text displayed.
                m_BackButton = m_UIDocument.rootVisualElement.Q<Button>("BackButton");

                // If found, assign the click event
                if (m_BackButton != null)
                {
                    m_BackButton.clicked += OnBackButtonClicked;
                }
                else
                {
                    Debug.LogError("Could not find a button named 'BackButton' in the UXML. Please check the name in UI Builder.");
                }
            }

            // This method runs when the button is clicked
            private void OnBackButtonClicked()
            {
                // 1. Enable the Main Menu UI
                if (m_MenuUI != null)
                {
                    m_MenuUI.SetActive(true);
                }

                // 2. Disable this Credits UI
                gameObject.SetActive(false);
            }
        }
    }
}