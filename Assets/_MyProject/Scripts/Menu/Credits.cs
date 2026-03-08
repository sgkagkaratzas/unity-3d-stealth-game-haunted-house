using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using MyGame.Global;

namespace MyGame.Menu
{
    public class Credits : MonoBehaviour
    {
        private UIDocument m_UIDocument;
        private Button m_BackButton;
        private Label m_TitleLabel;
        private Label m_CreditsContent;

        private GameDataRoot _gameData;

        [Header("Scene References")]
        [SerializeField] private GameObject m_MenuUI;

        public InputAction cancelAction; // Added for input handling

        private void Awake()
        {
            // Setup default cancel input (Esc / Gamepad B) when not provided in Inspector
            m_UIDocument = GetComponent<UIDocument>();

            if (cancelAction == null || cancelAction.bindings.Count == 0)
            {
                cancelAction = new InputAction("Cancel");
                cancelAction.AddBinding("<Keyboard>/escape");
                cancelAction.AddBinding("<Gamepad>/buttonEast");
            }

            LoadCreditsData();
        }

        private void LoadCreditsData()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("game_content");
            if (jsonFile != null)
            {
                _gameData = JsonUtility.FromJson<GameDataRoot>(jsonFile.text);
            }
            else
            {
                Debug.LogError("Could not find game_content.json in Resources!");
            }
        }

        private void OnEnable()
        {
            cancelAction.Enable();

            if (m_UIDocument == null) return;

            var root = m_UIDocument.rootVisualElement;

            m_TitleLabel = root.Q<Label>("TitleLabel");
            m_CreditsContent = root.Q<Label>("CreditsContent");
            m_BackButton = root.Q<Button>("BackButton");

            // Inject JSON Text Content
            if (_gameData != null && _gameData.mainMenu != null && _gameData.mainMenu.credits != null)
            {
                if (m_TitleLabel != null) m_TitleLabel.text = _gameData.mainMenu.credits.TitleLabel;
                if (m_CreditsContent != null) m_CreditsContent.text = _gameData.mainMenu.credits.CreditsScroll;
                if (m_BackButton != null) m_BackButton.text = _gameData.mainMenu.credits.BackButton;
            }

            if (m_BackButton != null)
            {
                m_BackButton.clicked += OnBackButtonClicked;
                m_BackButton.schedule.Execute(() => m_BackButton.Focus());
            }
        }

        private void OnDisable()
        {
            cancelAction.Disable();
        }

        private void Update()
        {
            if (cancelAction.WasPerformedThisFrame())
            {
                OnBackButtonClicked();
            }
        }

        private void OnBackButtonClicked()
        {
            if (m_MenuUI != null) m_MenuUI.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
