using UnityEngine;
using UnityEngine.UIElements;
using MyGame.Player;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace MyGame.Obstacles
{
    public class KeyQuizUI : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip failSound;
        [SerializeField] private AudioClip mercySound;

        [Header("Colors")]
        [SerializeField] private Color correctColor = new Color(0.18f, 0.8f, 0.44f);
        [SerializeField] private Color wrongColor = new Color(0.9f, 0.3f, 0.23f);
        [SerializeField] private Color mercyColor = new Color(1f, 0.84f, 0f);

        private UIDocument _document;
        private VisualElement _questionPopup;
        private Label _questionLabel;
        private VisualElement _answersContainer;

        private Key _currentActiveKey;
        private PlayerMovement _currentPlayer;
        private QuestionList _allQuestions;
        private QuestionData _currentQuestionData;

        private bool _isProcessingAnswer = false;
        private List<Button> _activeButtons = new List<Button>();

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            LoadQuestions();
        }

        private void OnEnable()
        {
            var root = _document.rootVisualElement;
            _questionPopup = root.Q<VisualElement>("QuestionPopup");
            _questionLabel = root.Q<Label>("QuestionLabel");
            _answersContainer = root.Q<VisualElement>("AnswersContainer");

            // Ensure the Question Label doesn't block clicks if it overlaps the button
            if (_questionLabel != null) _questionLabel.pickingMode = PickingMode.Ignore;

            root.Q<Button>("CancelQuizButton").clicked += ClosePopup;
        }

        private void LoadQuestions()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("questions");
            if (jsonFile != null)
                _allQuestions = JsonUtility.FromJson<QuestionList>(jsonFile.text);
        }

        public void ShowQuestion(Key keyInstance, PlayerMovement playerInstance)
        {
            _isProcessingAnswer = false;
            _currentActiveKey = keyInstance;
            _currentPlayer = playerInstance;
            _activeButtons.Clear();

            if (_currentPlayer != null) _currentPlayer.ForceIdle();

            // 1. Get all questions for "key0001"
            var fullPool = _allQuestions.questions
                .Where(q => q.id == keyInstance.KeyName)
                .ToList();

            if (fullPool.Count > 0)
            {
                // 2. Filter out the ID we used last time
                var filteredPool = fullPool
                    .Where(q => q.q_id != keyInstance.LastQuestionID)
                    .ToList();

                // Safety: If filtering removed everything (e.g. only 1 question exists), use full pool
                var finalPool = (filteredPool.Count > 0) ? filteredPool : fullPool;

                // 3. Pick Random
                int randomIndex = Random.Range(0, finalPool.Count);
                _currentQuestionData = finalPool[randomIndex];

                // 4. Remember this ID for next time
                keyInstance.LastQuestionID = _currentQuestionData.q_id;

                RenderQuestion();
            }
            else
            {
                Debug.LogError($"No questions found in JSON for Key ID: {keyInstance.KeyName}");
                ClosePopup();
            }
        }

        private void RenderQuestion()
        {
            _questionLabel.text = _currentQuestionData.text;
            _answersContainer.Clear();

            for (int i = 0; i < _currentQuestionData.answers.Length; i++)
            {
                Button newBtn = new Button();
                newBtn.text = _currentQuestionData.answers[i];
                newBtn.AddToClassList("answer-button");
                newBtn.style.height = 50;
                newBtn.style.marginBottom = 10;
                newBtn.style.fontSize = 18;
                newBtn.style.backgroundColor = StyleKeyword.Null;
                newBtn.style.borderTopLeftRadius = 10;
                newBtn.style.borderTopRightRadius = 10;
                newBtn.style.borderBottomLeftRadius = 10;
                newBtn.style.borderBottomRightRadius = 10;

                int index = i;
                newBtn.clicked += () =>
                {
                    if (!_isProcessingAnswer) StartCoroutine(HandleAnswerRoutine(index, newBtn));
                };

                _answersContainer.Add(newBtn);
                _activeButtons.Add(newBtn);
            }

            _questionPopup.style.display = DisplayStyle.Flex;
            Time.timeScale = 0;
        }

        private IEnumerator HandleAnswerRoutine(int selectedIndex, Button clickedButton)
        {
            _isProcessingAnswer = true;
            bool isCorrect = (selectedIndex == _currentQuestionData.correctIndex);

            if (isCorrect)
            {
                // --- CORRECT ANSWER ---
                clickedButton.style.backgroundColor = new StyleColor(correctColor);
                if (successSound != null) audioSource.PlayOneShot(successSound);

                yield return new WaitForSecondsRealtime(1.5f);

                // Give Boost (TRUE)
                UnlockDoor(giveBoost: true);
            }
            else
            {
                // --- WRONG ANSWER (ANY ATTEMPT) ---
                clickedButton.style.backgroundColor = new StyleColor(wrongColor);
                if (failSound != null) audioSource.PlayOneShot(failSound);

                _currentActiveKey.RegisterFailure();

                // --- NEW FEATURE: SHOW CORRECT ANSWER ALWAYS ---
                // Wait 1 second so they see their mistake (RED)
                yield return new WaitForSecondsRealtime(1.0f);

                // Highlight the correct answer in GOLD/ORANGE
                int correctIdx = _currentQuestionData.correctIndex;
                if (correctIdx < _activeButtons.Count)
                {
                    // Optional: Play a small "hint" sound here if you want
                    // if (mercySound != null) audioSource.PlayOneShot(mercySound);

                    _activeButtons[correctIdx].style.backgroundColor = new StyleColor(mercyColor);
                }

                // Wait 3 seconds so they can read and learn the correct answer
                yield return new WaitForSecondsRealtime(3.0f);

                // --- NOW DECIDE: MERCY OR RETRY? ---
                if (_currentActiveKey.FailureCount >= 2)
                {
                    // MERCY: Unlock the door, but NO Boost (FALSE)
                    UnlockDoor(giveBoost: false);
                }
                else
                {
                    // RETRY: Close popup so they can run away and try again later
                    ClosePopup();
                }
            }
        }

        private void UnlockDoor(bool giveBoost)
        {
            if (_currentPlayer != null && _currentActiveKey != null)
            {
                // 1. Give the Key (Always)
                _currentPlayer.AddKey(_currentActiveKey.KeyName);

                // 2. Give Boost (Conditional)
                if (giveBoost)
                {
                    _currentPlayer.HandleSpeedBoost();
                    Debug.Log("Boost Granted!");
                }
                else
                {
                    Debug.Log("Mercy Unlock: No Boost granted.");
                }

                _currentActiveKey.ResolveKey(isMercy: !giveBoost);

                _currentActiveKey = null;
            }
            ClosePopup();
        }

        private void ClosePopup()
        {
            // --- CRITICAL FIX ---
            // Stop the "HandleAnswerRoutine" immediately!
            // This ensures the code doesn't try to continue running 
            // after we have cleared the data below.
            StopAllCoroutines();

            if (_currentActiveKey != null)
            {
                // We restart this specific coroutine for the cooldown
                // (It's safe because we just stopped the old ones)
                StartCoroutine(ResetKeyCooldown(_currentActiveKey));
                _currentActiveKey = null;
            }

            _questionPopup.style.display = DisplayStyle.None;
            _currentQuestionData = null;
            Time.timeScale = 1;
        }

        private IEnumerator ResetKeyCooldown(Key keyToReset)
        {
            yield return new WaitForSeconds(2.0f);
            if (keyToReset != null)
            {
                keyToReset.EnableInteraction();
            }
        }
    }
}
