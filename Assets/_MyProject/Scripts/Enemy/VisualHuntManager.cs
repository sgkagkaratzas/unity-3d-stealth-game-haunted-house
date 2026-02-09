using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

namespace MyGame.Enemy
{
    public class VisualHuntManager : MonoBehaviour
    {
        [Header("Assign in Inspector")]
        [Tooltip("Drag your 'MainUI' GameObject here")]
        public UIDocument uiDocument;

        [Tooltip("Drag your Eye icon here")]
        public Texture2D searchIcon;

        [Header("Settings")]
        public float fadeSpeed = 2.0f;

        private VisualElement _overlayContainer;
        private bool _isHunting = false;

        void Start()
        {
            if (uiDocument != null)
            {
                CreateVisuals(uiDocument.rootVisualElement);
            }
            else
            {
                Debug.LogError("VisualHuntManager: UI Document is missing! Drag MainUI here.");
            }
        }

        private void CreateVisuals(VisualElement root)
        {
            // 1. Create the Transparent Container Layer
            _overlayContainer = new VisualElement();
            _overlayContainer.name = "HuntContainer";
            _overlayContainer.style.position = Position.Absolute;
            _overlayContainer.style.top = 0; _overlayContainer.style.bottom = 0;
            _overlayContainer.style.left = 0; _overlayContainer.style.right = 0;
            _overlayContainer.style.backgroundColor = new StyleColor(Color.clear);
            _overlayContainer.style.opacity = 0;

            // CRITICAL: Ignore clicks on the container
            _overlayContainer.pickingMode = PickingMode.Ignore;

            _overlayContainer.style.alignItems = Align.Center;
            _overlayContainer.style.justifyContent = Justify.FlexStart;
            _overlayContainer.style.paddingTop = 50;

            // 2. Create the Icon
            if (searchIcon != null)
            {
                VisualElement icon = new VisualElement();
                icon.style.backgroundImage = new StyleBackground(searchIcon);
                icon.style.backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100));
                icon.style.width = 64;
                icon.style.height = 64;
                icon.style.marginBottom = 10;

                // CRITICAL: Ignore clicks on the icon
                icon.pickingMode = PickingMode.Ignore;

                _overlayContainer.Add(icon);
            }

            // 3. Create the Text
            Label text = new Label("”≈ ÿ¡◊Õ≈…...");
            text.style.fontSize = 40;
            text.style.color = new StyleColor(Color.white);
            text.style.unityFontStyleAndWeight = FontStyle.Bold;
            text.style.textShadow = new TextShadow { offset = new Vector2(2, 2), color = new Color(0, 0, 0, 0.8f) };

            // CRITICAL: Ignore clicks on the text (This is what was blocking your button!)
            text.pickingMode = PickingMode.Ignore;

            _overlayContainer.Add(text);
            root.Add(_overlayContainer);
        }

        public void StartHunt()
        {
            if (_isHunting) return;
            _isHunting = true;
            StopAllCoroutines();
            // Fades the container (and its text/icon children) in
            StartCoroutine(FadeTo(1f));
        }

        public void EndHunt()
        {
            if (!_isHunting) return;
            _isHunting = false;
            StopAllCoroutines();
            // Fades the container out
            StartCoroutine(FadeTo(0f));
        }

        private IEnumerator FadeTo(float targetOpacity)
        {
            float startOpacity = _overlayContainer.style.opacity.value;
            float t = 0;

            while (t < 1)
            {
                t += Time.deltaTime * fadeSpeed;
                _overlayContainer.style.opacity = Mathf.Lerp(startOpacity, targetOpacity, t);
                yield return null;
            }
            _overlayContainer.style.opacity = targetOpacity;
        }
    }
}