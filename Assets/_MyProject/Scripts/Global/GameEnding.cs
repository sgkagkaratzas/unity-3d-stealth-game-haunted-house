using UnityEngine;
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

        // Name of the next scene to load
        public string nextSceneName = null;

        bool m_HasAudioPlayed;
        bool m_IsPlayerAtExit;
        bool m_IsPlayerCaught;
        float m_Timer;

        private VisualElement m_EndScreen;    // The old fading screen
        private VisualElement m_CaughtScreen; // The Caught screen
        private VisualElement m_RatingPopup;  // The Yes/No Popup

        private Button m_YesButton;
        private Button m_NoButton;

        private Label m_TimerLabel;
        private Label m_UsernameLabel;

        void Start()
        {
            var root = uiDocument.rootVisualElement;

            m_EndScreen = root.Q<VisualElement>("EndScreen");
            m_CaughtScreen = root.Q<VisualElement>("CaughtScreen");

            // 1. Find the new Popup and Buttons
            m_RatingPopup = root.Q<VisualElement>("RatingPopup");
            m_YesButton = root.Q<Button>("RatingYesButton");
            m_NoButton = root.Q<Button>("RatingNoButton");

            // 2. Hook up button clicks
            if (m_YesButton != null) m_YesButton.clicked += () => OnRateClicked(true);
            if (m_NoButton != null) m_NoButton.clicked += () => OnRateClicked(false);

            m_TimerLabel = root.Q<Label>("TimerLabel");
            m_UsernameLabel = root.Q<Label>("UsernameLabel");

            GlobalGameData.GameTimer = 0f;
            UpdateTimerLabel();

            if (m_UsernameLabel != null)
                m_UsernameLabel.text = GlobalGameData.PlayerName;
        }

        // Clean up event listeners to avoid memory leaks
        void OnDestroy()
        {
            if (m_YesButton != null) m_YesButton.clicked -= () => OnRateClicked(true);
            if (m_NoButton != null) m_NoButton.clicked -= () => OnRateClicked(false);
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
                // WIN CONDITION: Don't fade. Just show popup.
                ShowRatingPopup(exitAudio);
            }
            else if (m_IsPlayerCaught)
            {
                // LOSE CONDITION: Keep original behavior (Fade red screen -> Restart)
                EndLevel(m_CaughtScreen, true, caughtAudio);
            }
        }

        // Handles the Win Popup logic
        void ShowRatingPopup(AudioSource audioSource)
        {
            if (!m_HasAudioPlayed)
            {
                audioSource.Play();
                m_HasAudioPlayed = true;
            }

            // Show the popup
            if (m_RatingPopup != null) m_RatingPopup.style.display = DisplayStyle.Flex;

            // Pause the game so the timer stops and player can't move
            Time.timeScale = 0f;
        }

        // Called when Yes or No is clicked
        void OnRateClicked(bool likedLevel)
        {
            Debug.Log(likedLevel ? "Player Liked the Level!" : "Player did not like the level.");

            // Resume time just in case
            Time.timeScale = 1f;

            // Load the next scene
            SceneManager.LoadScene(nextSceneName);
        }

        void UpdateTimerLabel()
        {
            if (m_TimerLabel != null)
            {
                int seconds = Mathf.FloorToInt(GlobalGameData.GameTimer);
                m_TimerLabel.text = seconds.ToString();
            }
        }

        // Kept only for the "Caught" screen logic
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
                Time.timeScale = 1; // Ensure time is running before load
                if (doRestart)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }
    }
}