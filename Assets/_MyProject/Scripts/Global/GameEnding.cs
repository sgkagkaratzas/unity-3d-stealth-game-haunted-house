using MyGame.Enemy;
using MyGame.Logging;
using MyGame.Obstacles;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace MyGame.Global
{
    public class GameEnding : MonoBehaviour
    {
        public float fadeDuration = 1f;
        public float displayImageDuration = 1f;
        public GameObject player;
        public UIDocument uiDocument;
        public AudioSource exitAudio;
        public AudioSource caughtAudio;
        public string nextSceneName = null;

        [Header("Controller Inputs")]
        public InputAction confirmAction;
        public InputAction cancelAction;

        bool m_HasAudioPlayed;
        bool m_IsPlayerAtExit;
        bool m_IsPlayerCaught;
        float m_Timer;

        private VisualElement m_EndScreen;
        private VisualElement m_CaughtScreen;
        private VisualElement m_RatingPopup;
        private Button m_YesButton;
        private Button m_NoButton;
        private Label m_TimerLabel;
        private Label m_UsernameLabel;

        void Start()
        {
            var root = uiDocument.rootVisualElement;
            m_EndScreen = root.Q<VisualElement>("EndScreen");
            m_CaughtScreen = root.Q<VisualElement>("CaughtScreen");
            m_RatingPopup = root.Q<VisualElement>("RatingPopup");
            m_YesButton = root.Q<Button>("RatingYesButton");
            m_NoButton = root.Q<Button>("RatingNoButton");

            if (m_YesButton != null) m_YesButton.clicked += () => OnRateClicked(true);
            if (m_NoButton != null) m_NoButton.clicked += () => OnRateClicked(false);

            m_TimerLabel = root.Q<Label>("TimerLabel");
            m_UsernameLabel = root.Q<Label>("UsernameLabel");
            UpdateTimerLabel();

            if (m_UsernameLabel != null) m_UsernameLabel.text = GlobalGameData.PlayerName;
        }

        void OnDestroy()
        {
            if (m_YesButton != null) m_YesButton.clicked -= () => OnRateClicked(true);
            if (m_NoButton != null) m_NoButton.clicked -= () => OnRateClicked(false);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == player && !m_IsPlayerAtExit && !m_IsPlayerCaught)
            {
                m_IsPlayerAtExit = true;
                var logger = LoggerFactory.GetLogger();
                logger?.LogEvent(GlobalGameData.PlayerName, GlobalGameData.GameTimer, "Success");
            }
        }

        public void CaughtPlayer()
        {
            if (m_IsPlayerCaught) return;
            m_IsPlayerCaught = true;

            var logger = LoggerFactory.GetLogger();
            logger?.LogEvent(GlobalGameData.PlayerName, GlobalGameData.GameTimer, "Caught");

            if (m_TimerLabel != null) m_TimerLabel.style.display = DisplayStyle.None;
            if (uiDocument != null)
            {
                var footer = uiDocument.rootVisualElement.Q<VisualElement>("Footer");
                if (footer != null) footer.style.display = DisplayStyle.None;
            }

            if (m_CaughtScreen != null)
            {
                m_CaughtScreen.style.display = DisplayStyle.Flex;
                m_CaughtScreen.BringToFront();
            }

            var huntManager = FindFirstObjectByType<VisualHuntManager>();
            if (huntManager != null) huntManager.HideImmediate();

            var quizUI = FindFirstObjectByType<KeyQuizUI>();
            if (quizUI != null) quizUI.ForceClose();

            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }

        void Update()
        {
            if (!m_IsPlayerAtExit && !m_IsPlayerCaught)
            {
                GlobalGameData.GameTimer += Time.deltaTime;
                UpdateTimerLabel();
            }

            if (m_IsPlayerAtExit) ShowRatingPopup(exitAudio);

            if (m_IsPlayerCaught)
            {
                EndLevel(m_CaughtScreen, true, caughtAudio, false);
            }
        }

        void ShowRatingPopup(AudioSource audioSource)
        {
            if (m_RatingPopup == null) return;

            if (!m_HasAudioPlayed && audioSource != null)
            {
                audioSource.Play();
                m_HasAudioPlayed = true;
            }

            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;

            m_RatingPopup.style.display = DisplayStyle.Flex;
            Time.timeScale = 0f;

            if (confirmAction.WasPerformedThisFrame()) OnRateClicked(true);
            if (cancelAction.WasPerformedThisFrame()) OnRateClicked(false);
        }

        void OnRateClicked(bool likedLevel)
        {
            Time.timeScale = 1f;
            if (!string.IsNullOrEmpty(nextSceneName))
                SceneManager.LoadScene(nextSceneName);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void UpdateTimerLabel()
        {
            if (m_TimerLabel != null)
            {
                int seconds = Mathf.FloorToInt(GlobalGameData.GameTimer);
                m_TimerLabel.text = seconds.ToString();
            }
        }

        void EndLevel(VisualElement element, bool doRestart, AudioSource audioSource, bool forceImmediate = false)
        {
            if (element == null) return;

            if (!m_HasAudioPlayed && audioSource != null)
            {
                audioSource.Play();
                m_HasAudioPlayed = true;
            }

            if (forceImmediate)
            {
                element.style.opacity = 1f;
                Time.timeScale = 0f;
            }
            else
            {
                m_Timer += Time.unscaledDeltaTime;
                element.style.opacity = Mathf.Clamp01(m_Timer / fadeDuration);
            }

            if (m_Timer > fadeDuration + displayImageDuration || forceImmediate)
            {
                Time.timeScale = 1f;
                if (doRestart)
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
