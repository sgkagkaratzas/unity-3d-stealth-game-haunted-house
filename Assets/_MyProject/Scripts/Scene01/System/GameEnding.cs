using MyGame.Global;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace MyGame.Scene01.System
{
    public class GameEnding : MonoBehaviour
    {
        public float fadeDuration = 1f;
        public float displayImageDuration = 1f;
        public GameObject player;
        public UIDocument uiDocument;
        public AudioSource exitAudio;
        public AudioSource caughtAudio;

        bool m_HasAudioPlayed;
        bool m_IsPlayerAtExit;
        bool m_IsPlayerCaught;
        float m_Timer;

        private VisualElement m_EndScreen;
        private VisualElement m_CaughtScreen;

        private Label m_TimerLabel;
        private Label m_UsernameLabel;

        void Start()
        {
            m_EndScreen = uiDocument.rootVisualElement.Q<VisualElement>("EndScreen");
            m_CaughtScreen = uiDocument.rootVisualElement.Q<VisualElement>("CaughtScreen");

            m_TimerLabel = uiDocument.rootVisualElement.Q<Label>("TimerLabel");
            m_UsernameLabel = uiDocument.rootVisualElement.Q<Label>("UsernameLabel");

            GlobalGameData.GameTimer = 0f;

            UpdateTimerLabel();

            if (m_UsernameLabel != null)
            {
                m_UsernameLabel.text = GlobalGameData.PlayerName;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == player)
            {
                m_IsPlayerAtExit = true;
            }
        }

        public void CaughtPlayer()
        {
            m_IsPlayerCaught = true;
        }

        void Update()
        {
            if (!m_IsPlayerAtExit && !m_IsPlayerCaught)
            {
                GlobalGameData.GameTimer += Time.deltaTime;
                UpdateTimerLabel();
            }

            if (m_IsPlayerAtExit)
            {
                EndLevel(m_EndScreen, false, exitAudio);
            }
            else if (m_IsPlayerCaught)
            {
                EndLevel(m_CaughtScreen, true, caughtAudio);
            }
        }

        void UpdateTimerLabel()
        {
            if (m_TimerLabel != null)
            {
                m_TimerLabel.text = GlobalGameData.GameTimer.ToString("0.00");
            }
        }

        void EndLevel(VisualElement element, bool doRestart, AudioSource audioSource)
        {
            if (!m_HasAudioPlayed)
            {
                audioSource.Play();
                m_HasAudioPlayed = true;
            }

            m_Timer += Time.deltaTime;
            element.style.opacity = m_Timer / fadeDuration;

            if (m_Timer > fadeDuration + displayImageDuration)
            {
                if (doRestart)
                {
                    Time.timeScale = 1;
                    SceneManager.LoadScene("Scene_01");
                }
                else
                {
                    Time.timeScale = 1;
                    SceneManager.LoadScene("Scene_02");
                }
            }
        }
    }
}