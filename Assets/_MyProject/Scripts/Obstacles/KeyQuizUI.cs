using UnityEngine;
using UnityEngine.UIElements;
using MyGame.Player;
using MyGame.Logging;
using MyGame.Global;
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
        [SerializeField] private Color selectedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
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
        private float _questionOpenedRealtime = -1f;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            // Load question pool from Resources/questions.json (TextAsset)
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
                // record open time and log the question opened event
                _questionOpenedRealtime = Time.realtimeSinceStartup;
                var logger = LoggerFactory.GetLogger();
                string qid = _currentQuestionData != null ? _currentQuestionData.q_id : "";
                string keyName = keyInstance != null ? keyInstance.KeyName : "";
                logger?.LogEvent(GlobalGameData.PlayerName, GlobalGameData.GameTimer, $"Question opened;Key:{keyName};QID:{qid}");
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

                newBtn.style.height = 50;
                newBtn.style.marginBottom = 10;
                newBtn.style.fontSize = 18;
                newBtn.style.backgroundColor = StyleKeyword.Null;
                newBtn.style.borderTopLeftRadius = 10;
                newBtn.style.borderTopRightRadius = 10;
                newBtn.style.borderBottomLeftRadius = 10;
                newBtn.style.borderBottomRightRadius = 10;

                newBtn.RegisterCallback<MouseEnterEvent>(evt => newBtn.style.backgroundColor = selectedColor);
                newBtn.RegisterCallback<MouseLeaveEvent>(evt => newBtn.style.backgroundColor = StyleKeyword.Null);

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

            if (_activeButtons.Count > 0)
            {
                _activeButtons[0].schedule.Execute(() => _activeButtons[0].Focus());
            }
        }

        private IEnumerator HandleAnswerRoutine(int selectedIndex, Button clickedButton)
        {
            _isProcessingAnswer = true;
            bool isCorrect = (selectedIndex == _currentQuestionData.correctIndex);

            // compute time spent on question
            float spent = -1f;
            if (_questionOpenedRealtime > 0f)
            {
                spent = Time.realtimeSinceStartup - _questionOpenedRealtime;
                _questionOpenedRealtime = -1f;
            }

            // log the answer event with indices (selected index and correct index)
            var loggerAns = LoggerFactory.GetLogger();
            string keyN = _currentActiveKey != null ? _currentActiveKey.KeyName : "";
            string qidAns = _currentQuestionData != null ? _currentQuestionData.q_id : "";
            int loggedCorrectIdx = _currentQuestionData != null ? _currentQuestionData.correctIndex : -1;

            loggerAns?.LogEvent(GlobalGameData.PlayerName,
                spent > 0f ? spent : GlobalGameData.GameTimer,
                $"Question answered;Key:{keyN};QID:{qidAns};SelectedIndex:{selectedIndex};Correct:{isCorrect};CorrectIndex:{loggedCorrectIdx}");

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

                var ghost = FindFirstObjectByType<MyGame.Enemy.GuardianPatrol>();
                if (ghost != null) ghost.AlertToPosition(_currentActiveKey.transform.position, _currentActiveKey.KeyName);

                yield return new WaitForSecondsRealtime(1.0f);

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

            // preserve key info before clearing _currentActiveKey
            string keyNameBefore = _currentActiveKey != null ? _currentActiveKey.KeyName : "";
            var keyToReset = _currentActiveKey;

            if (keyToReset != null)
            {
                StartCoroutine(ResetKeyCooldown(keyToReset));
            }

            // If a question was open, log the cancel/close event with duration
            if (_currentQuestionData != null && _questionOpenedRealtime > 0f)
            {
                float spentClose = Time.realtimeSinceStartup - _questionOpenedRealtime;
                var loggerClose = LoggerFactory.GetLogger();
                string keyC = keyNameBefore;
                string qidC = _currentQuestionData != null ? _currentQuestionData.q_id : "";
                loggerClose?.LogEvent(GlobalGameData.PlayerName, spentClose, $"Question canceled;Key:{keyC};QID:{qidC}");
                _questionOpenedRealtime = -1f;
            }

            _currentActiveKey = null;
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