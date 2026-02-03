using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

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

        private bool m_IsPaused = false;

        void Start()
        {
            // Enable the Input Action immediately
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

            // Hook up Mouse Clicks
            if (m_ResumeButton != null) m_ResumeButton.clicked += ResumeGame;
            if (m_ExitButton != null) m_ExitButton.clicked += QuitToMenu;
        }

        void OnDestroy()
        {
            // Good practice to disable actions when object is destroyed
            pauseAction.Disable();
        }

        void Update()
        {
            // Poll for the Pause Button (works for Keyboard ESC & Controller Start)
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
            Time.timeScale = 0f; // Freeze Game

            if (m_PauseMenu != null) m_PauseMenu.style.display = DisplayStyle.Flex;

            // Unlock mouse for PC users so they can click if they want
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }

        void ResumeGame()
        {
            m_IsPaused = false;
            Time.timeScale = 1f; // Unfreeze Game

            if (m_PauseMenu != null) m_PauseMenu.style.display = DisplayStyle.None;
        }

        void QuitToMenu()
        {
            Time.timeScale = 1f; // Always unfreeze before leaving

            // Reset Global Data
            GlobalGameData.GameTimer = 0f;
            GlobalGameData.PlayerName = "";

            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}