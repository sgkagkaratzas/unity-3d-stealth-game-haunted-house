using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyGame.Menu
{
    namespace StealthGame
    {
        public class Info : MonoBehaviour
        {
            private UIDocument m_UIDocument;
            private Button m_BackButton;

            [Header("Scene References")]
            [Tooltip("The Main Menu GameObject to turn back on when Back is clicked.")]
            [SerializeField] private GameObject m_MenuUI;

            private void Awake()
            {
                m_UIDocument = GetComponent<UIDocument>();
            }

            private void OnEnable()
            {
                if (m_UIDocument == null) return;

                // Make sure the button in your Info UXML is named 'BackButton'
                m_BackButton = m_UIDocument.rootVisualElement.Q<Button>("BackButton");

                if (m_BackButton != null)
                {
                    m_BackButton.clicked += OnBackButtonClicked;
                }
            }

            private void OnBackButtonClicked()
            {
                // Turn the Main Menu back on
                if (m_MenuUI != null)
                {
                    m_MenuUI.SetActive(true);
                }

                // Turn this Info window off
                gameObject.SetActive(false);
            }
        }
    }
}