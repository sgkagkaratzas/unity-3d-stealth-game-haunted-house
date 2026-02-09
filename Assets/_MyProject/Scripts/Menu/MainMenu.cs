using MyGame.Global;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace MyGame.Menu
{
    public class MainMenu : MonoBehaviour
    {
        private UIDocument m_UIDocument;

        private Button m_StartButton;
        private Button m_CreditsButton;
        private Button m_InfoButton;
        private Button m_ExitButton;

        [Header("Scene References")]
        [SerializeField] private GameObject m_CreditsUI;
        [SerializeField] private GameObject m_InfoUI;

        private void Awake()
        {
            m_UIDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (m_UIDocument == null) return;

            var root = m_UIDocument.rootVisualElement;

            m_StartButton = root.Q<Button>("StartButton");
            if (m_StartButton != null)
            {
                m_StartButton.clicked += () =>
                {
                    GlobalGameData.GameTimer = 0f;
                    // Generate a simple unique player name if none provided
                    string shortHash = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                    GlobalGameData.PlayerName = "User_" + shortHash;

                    Debug.Log($"New Game Started. Name: {GlobalGameData.PlayerName}");
                    SceneManager.LoadScene("Scene_01");
                };
                AddHoverAnimation(m_StartButton);

                m_StartButton.schedule.Execute(() => m_StartButton.Focus());
            }

            m_CreditsButton = root.Q<Button>("CreditsButton");
            if (m_CreditsButton != null)
            {
                m_CreditsButton.clicked += OnCreditsClicked;
                AddHoverAnimation(m_CreditsButton);
            }

            m_InfoButton = root.Q<Button>("InfoButton");
            if (m_InfoButton != null)
            {
                m_InfoButton.clicked += OnInfoClicked;
                AddHoverAnimation(m_InfoButton);
            }

            m_ExitButton = root.Q<Button>("ExitButton");
            if (m_ExitButton != null)
            {
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    m_ExitButton.style.display = DisplayStyle.None;
                }
                else
                {
                    m_ExitButton.style.display = DisplayStyle.Flex;
                    m_ExitButton.clicked += () => Application.Quit();
                    AddHoverAnimation(m_ExitButton);
                }
            }
        }

        private void AddHoverAnimation(Button button)
        {
            if (button == null) return;

            button.style.transitionDuration = new List<TimeValue>
            {
                new TimeValue(0.1f, TimeUnit.Second)
            };

            button.style.transitionTimingFunction = new List<EasingFunction>
            {
                new EasingFunction(EasingMode.EaseOutBack)
            };

            button.RegisterCallback<MouseEnterEvent>(evt =>
            {
                button.style.scale = new Scale(new Vector3(1.1f, 1.1f, 1f));
            });
            button.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                button.style.scale = new Scale(Vector3.one);
            });

            button.RegisterCallback<FocusEvent>(evt =>
            {
                button.style.scale = new Scale(new Vector3(1.1f, 1.1f, 1f));
            });
            button.RegisterCallback<BlurEvent>(evt =>
            {
                button.style.scale = new Scale(Vector3.one);
            });
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