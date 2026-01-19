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

            private Button m_StartButton;
            private Button m_ExitButton;

            private void Awake()
            {
                m_UIDocument = GetComponent<UIDocument>();
            }

            private void OnEnable()
            {
                m_StartButton = m_UIDocument.rootVisualElement.Q<Button>("StartButton");
                m_ExitButton = m_UIDocument.rootVisualElement.Q<Button>("ExitButton");

                m_StartButton.clicked += () =>
                {
                    SceneManager.LoadScene("Scene_01");
                };
            }
        }
    }
}