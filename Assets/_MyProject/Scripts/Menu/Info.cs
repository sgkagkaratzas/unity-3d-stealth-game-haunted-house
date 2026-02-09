using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace MyGame.Menu
{
    public class Info : MonoBehaviour
    {
        // Comments cleaned: removed non-essential comments
        private UIDocument m_UIDocument;
        private Button m_BackButton;

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
        }

        private void OnEnable()
        {
            cancelAction.Enable();

            if (m_UIDocument == null) return;

            m_BackButton = m_UIDocument.rootVisualElement.Q<Button>("BackButton");

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