using MyGame.Global;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace MyGame.Menu
{
    namespace StealthGame
    {
        public class MainMenu : MonoBehaviour
        {
            private UIDocument m_UIDocument;

            // UI Buttons
            private Button m_StartButton;
            private Button m_CreditsButton;
            private Button m_InfoButton;

            [Header("Scene References")]
            [Tooltip("The CreditsUI GameObject to turn on.")]
            [SerializeField] private GameObject m_CreditsUI;

            [Tooltip("The InfoUI GameObject to turn on.")]
            [SerializeField] private GameObject m_InfoUI;

            private void Awake()
            {
                m_UIDocument = GetComponent<UIDocument>();
            }

            private void OnEnable()
            {
                if (m_UIDocument == null) return;

                var root = m_UIDocument.rootVisualElement;

                // 1. Setup Start Button
                m_StartButton = root.Q<Button>("StartButton");
                if (m_StartButton != null)
                {
                    m_StartButton.clicked += () =>
                    {
                        // Generate a unique name: "User_" + first 4 chars of a global ID
                        string shortHash = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
                        GlobalGameData.PlayerName = "User_" + shortHash;

                        Debug.Log("Generated Name: " + GlobalGameData.PlayerName);

                        SceneManager.LoadScene("Scene_01");
                    };
                }

                // 2. Setup Credits Button
                m_CreditsButton = root.Q<Button>("CreditsButton");
                if (m_CreditsButton != null)
                {
                    m_CreditsButton.clicked += OnCreditsClicked;
                }

                // 3. Setup Info Button (Ensure your button is named 'InfoButton' in UI Builder)
                m_InfoButton = root.Q<Button>("InfoButton");
                if (m_InfoButton != null)
                {
                    m_InfoButton.clicked += OnInfoClicked;
                }
            }

            private void OnCreditsClicked()
            {
                if (m_CreditsUI != null) m_CreditsUI.SetActive(true);
                gameObject.SetActive(false);
            }

            private void OnInfoClicked()
            {
                if (m_InfoUI != null) m_InfoUI.SetActive(true);
                gameObject.SetActive(false);
            }
        }
    }
}