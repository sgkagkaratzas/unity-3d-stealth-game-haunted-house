using UnityEngine;
using UnityEngine.UIElements;
using MyGame.Logging;
using UnityEngine.InputSystem;
using MyGame.Global; // Added to access GameDataRoot

namespace MyGame.UI
{
    public class FooterPopup : MonoBehaviour
    {
        private UIDocument m_UIDocument;
        private VisualElement m_HelpPopup;
        private Button m_HelpButton;
        private Button m_CloseButton;
        private Label m_HelpContentLabel;
        private float m_HelpOpenedRealtime = -1f;

        private GameDataRoot _gameData; // Store the master JSON data

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

        private void LoadHelpText()
        {
            // Pointing to the new centralized JSON file
            TextAsset jsonFile = Resources.Load<TextAsset>("game_content");
            if (jsonFile != null)
            {
                _gameData = JsonUtility.FromJson<GameDataRoot>(jsonFile.text);
            }
            else
            {
                Debug.LogError("Could not find game_content.json in Resources folder!");
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
            m_HelpContentLabel = root.Q<Label>("HelpContentLabel");

            // Load and inject JSON text
            LoadHelpText();

            if (_gameData != null && _gameData.scene != null && _gameData.scene.HelpPopup != null)
            {
                if (m_HelpContentLabel != null) m_HelpContentLabel.text = _gameData.scene.HelpPopup.HelpContentLabel;
                if (m_CloseButton != null) m_CloseButton.text = _gameData.scene.HelpPopup.CloseButton;
            }

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

                // Record real time when help opened and log the opening event
                m_HelpOpenedRealtime = Time.realtimeSinceStartup;
                LoggerFactory.GetLogger()?.LogEvent(MyGame.Global.GlobalGameData.PlayerName, MyGame.Global.GlobalGameData.GameTimer, "Help opened");

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

                // Calculate time spent in help using realtime (not affected by timeScale)
                if (m_HelpOpenedRealtime > 0f)
                {
                    float spent = Time.realtimeSinceStartup - m_HelpOpenedRealtime;
                    LoggerFactory.GetLogger()?.LogEvent(MyGame.Global.GlobalGameData.PlayerName, spent, "Help closed");
                    m_HelpOpenedRealtime = -1f;
                }
            }
        }
    }
}
