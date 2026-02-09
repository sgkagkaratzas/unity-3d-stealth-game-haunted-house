using UnityEngine;
using UnityEngine.UIElements;
using MyGame.Player;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace MyGame.Obstacles
{
    public class KeyQuizUI : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip failSound;

        [Header("Colors")]
        // The color when you are HOVERING or NAVIGATING to a button
        [SerializeField] private Color selectedColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Dark Gray
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

            if (_questionLabel != null) _questionLabel.pickingMode = PickingMode.Ignore;

            root.Q<Button>("CancelQuizButton").clicked += ClosePopup;
        }

        private void Update()
        {
            // Close on B / Escape
            if (_questionPopup.style.display == DisplayStyle.Flex && !_isProcessingAnswer)
            {
                bool cancelPressed = false;
                if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) cancelPressed = true;
                if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame) cancelPressed = true;

                if (cancelPressed) ClosePopup();
            }
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

            var fullPool = _allQuestions.questions.Where(q => q.id == keyInstance.KeyName).ToList();

            if (fullPool.Count > 0)
            {
                var filteredPool = fullPool.Where(q => q.q_id != keyInstance.LastQuestionID).ToList();
                var finalPool = (filteredPool.Count > 0) ? filteredPool : fullPool;

                int randomIndex = Random.Range(0, finalPool.Count);
                _currentQuestionData = finalPool[randomIndex];
                keyInstance.LastQuestionID = _currentQuestionData.q_id;

                RenderQuestion();
            }
            else
            {
                Debug.LogError($"No questions found for Key ID: {keyInstance.KeyName}");
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

                // Base Style
                newBtn.style.height = 50;
                newBtn.style.marginBottom = 10;
                newBtn.style.fontSize = 18;
                newBtn.style.backgroundColor = StyleKeyword.Null; // Clear background
                newBtn.style.borderTopLeftRadius = 10;
                newBtn.style.borderTopRightRadius = 10;
                newBtn.style.borderBottomLeftRadius = 10;
                newBtn.style.borderBottomRightRadius = 10;

                // --- VISUAL FEEDBACK LOGIC ---
                // 1. Mouse Hover
                newBtn.RegisterCallback<MouseEnterEvent>(evt => newBtn.style.backgroundColor = selectedColor);
                newBtn.RegisterCallback<MouseLeaveEvent>(evt => newBtn.style.backgroundColor = StyleKeyword.Null);

                // 2. Controller/Keyboard Focus
                newBtn.RegisterCallback<FocusEvent>(evt => newBtn.style.backgroundColor = selectedColor);
                newBtn.RegisterCallback<BlurEvent>(evt => newBtn.style.backgroundColor = StyleKeyword.Null);

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

            // Auto-Focus First Button
            if (_activeButtons.Count > 0)
            {
                _activeButtons[0].schedule.Execute(() => _activeButtons[0].Focus());
            }
        }

        private IEnumerator HandleAnswerRoutine(int selectedIndex, Button clickedButton)
        {
            _isProcessingAnswer = true;
            bool isCorrect = (selectedIndex == _currentQuestionData.correctIndex);

            if (isCorrect)
            {
                clickedButton.style.backgroundColor = new StyleColor(correctColor);
                if (successSound != null) audioSource.PlayOneShot(successSound);

                yield return new WaitForSecondsRealtime(1.5f);
                UnlockDoor(giveBoost: true);
            }
            else
            {
                clickedButton.style.backgroundColor = new StyleColor(wrongColor);
                if (failSound != null) audioSource.PlayOneShot(failSound);

                _currentActiveKey.RegisterFailure();

                yield return new WaitForSecondsRealtime(1.0f);

                // Show Mercy Hint
                int correctIdx = _currentQuestionData.correctIndex;
                if (correctIdx < _activeButtons.Count)
                {
                    _activeButtons[correctIdx].style.backgroundColor = new StyleColor(mercyColor);
                }

                yield return new WaitForSecondsRealtime(3.0f);

                if (_currentActiveKey.FailureCount >= 2)
                    UnlockDoor(giveBoost: false);
                else
                    ClosePopup();
            }
        }

        private void UnlockDoor(bool giveBoost)
        {
            if (_currentPlayer != null && _currentActiveKey != null)
            {
                _currentPlayer.AddKey(_currentActiveKey.KeyName);
                if (giveBoost) _currentPlayer.HandleSpeedBoost();

                _currentActiveKey.ResolveKey(isMercy: !giveBoost);
                _currentActiveKey = null;
            }
            ClosePopup();
        }

        private void ClosePopup()
        {
            StopAllCoroutines();
            if (_currentActiveKey != null)
            {
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
            if (keyToReset != null) keyToReset.EnableInteraction();
        }

        public void ForceClose()
        {
            StopAllCoroutines();
            if (_questionPopup != null) _questionPopup.style.display = DisplayStyle.None;
            _isProcessingAnswer = false;
            _currentActiveKey = null;
            Time.timeScale = 1;
        }
    }
}