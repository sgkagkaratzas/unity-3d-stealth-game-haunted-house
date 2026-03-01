using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using MyGame.Logging;

namespace MyGame.Global
{
    public class PauseMenu : MonoBehaviour
    {
        public UIDocument uiDocument;
        public string mainMenuSceneName = "MainMenu";

        [Header("Input Settings")]
        public InputAction pauseAction;

        private VisualElement m_PauseMenu;
        private Button m_ResumeButton;
        private Button m_ExitButton;

        private float m_ExitOpenedRealtime = -1f;

        private bool m_IsPaused = false;

        private void Awake()
        {
            if (pauseAction == null || pauseAction.bindings.Count == 0)
            {
                pauseAction = new InputAction("Pause");
                pauseAction.AddBinding("<Keyboard>/escape");
                pauseAction.AddBinding("<Gamepad>/start");
            }
        }

        void Start()
        {
            pauseAction.Enable();

            if (uiDocument == null)
            {
                Debug.LogError("PauseMenu: UI Document is empty!");
                return;
            }

            var root = uiDocument.rootVisualElement;
            m_PauseMenu = root.Q<VisualElement>("PauseMenu");
            m_ResumeButton = root.Q<Button>("ResumeButton");
            m_ExitButton = root.Q<Button>("ExitButton");

            if (m_ResumeButton != null)
            {
                m_ResumeButton.clicked += ResumeGame;
            }

            if (m_ExitButton != null)
            {
                m_ExitButton.clicked += OnExitButtonClicked;
            }
        }

        void OnDestroy()
        {
            pauseAction.Disable();
        }

        void Update()
        {
            if (pauseAction.WasPerformedThisFrame())
            {
                if (m_IsPaused)
                    ResumeGame();
                else
                    PauseGame();
            }
        }

        void PauseGame()
        {
            m_IsPaused = true;
            Time.timeScale = 0f;

            if (m_PauseMenu != null)
            {
                m_PauseMenu.style.display = DisplayStyle.Flex;

                if (m_ResumeButton != null)
                {
                    m_ResumeButton.schedule.Execute(() => m_ResumeButton.Focus());
                }
            }

            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;

            LoggerFactory.GetLogger()?.LogEvent(GlobalGameData.PlayerName, GlobalGameData.GameTimer, "Pause menu opened");
        }

        private void OnExitButtonClicked()
        {
            // Record when the user opened the exit action (clicked Exit)
            m_ExitOpenedRealtime = Time.realtimeSinceStartup;
            LoggerFactory.GetLogger()?.LogEvent(GlobalGameData.PlayerName, GlobalGameData.GameTimer, "Exit menu opened");

            // Proceed to quit to menu (existing behavior)
            QuitToMenu();
        }

        void ResumeGame()
        {
            m_IsPaused = false;
            Time.timeScale = 1f;

            if (m_PauseMenu != null) m_PauseMenu.style.display = DisplayStyle.None;

            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;

            LoggerFactory.GetLogger()?.LogEvent(GlobalGameData.PlayerName, GlobalGameData.GameTimer, "Pause menu closed");
        }

        void QuitToMenu()
        {
            Time.timeScale = 1f;

            // If exit was opened, log the closing and duration spent in the exit action
            if (m_ExitOpenedRealtime > 0f)
            {
                float spent = Time.realtimeSinceStartup - m_ExitOpenedRealtime;
                LoggerFactory.GetLogger()?.LogEvent(GlobalGameData.PlayerName, spent, "Exit menu closed");
                m_ExitOpenedRealtime = -1f;
            }

            LoggerFactory.GetLogger()?.LogEvent(GlobalGameData.PlayerName, GlobalGameData.GameTimer, "Quit to main menu");

            GlobalGameData.GameTimer = 0f;
            GlobalGameData.PlayerName = "";

            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
