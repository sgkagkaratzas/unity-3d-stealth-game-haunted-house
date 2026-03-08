using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using MyGame.Global;

namespace MyGame.Menu
{
    public class Info : MonoBehaviour
    {
        private UIDocument m_UIDocument;
        private Button m_BackButton;
        private Label m_TitleLabel;
        private Label m_InfoContent;

        private GameDataRoot _gameData;

        [Header("Scene References")]
        [SerializeField] private GameObject m_MenuUI;

        public InputAction cancelAction;

        private void Awake()
        {
            // Setup cancel input binding when missing (Esc / Gamepad B)
            m_UIDocument = GetComponent<UIDocument>();

            if (cancelAction == null || cancelAction.bindings.Count == 0)
            {
                cancelAction = new InputAction("Cancel");
                cancelAction.AddBinding("<Keyboard>/escape");
                cancelAction.AddBinding("<Gamepad>/buttonEast");
            }

            LoadInfoData();
        }

        private void LoadInfoData()
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
            m_InfoContent = root.Q<Label>("InfoContent");
            m_BackButton = root.Q<Button>("BackButton");

            // Inject JSON Text Content
            if (_gameData != null && _gameData.mainMenu != null && _gameData.mainMenu.info != null)
            {
                if (m_TitleLabel != null) m_TitleLabel.text = _gameData.mainMenu.info.TitleLabel;
                if (m_InfoContent != null) m_InfoContent.text = _gameData.mainMenu.info.CreditsScroll;
                if (m_BackButton != null) m_BackButton.text = _gameData.mainMenu.info.BackButton;
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
