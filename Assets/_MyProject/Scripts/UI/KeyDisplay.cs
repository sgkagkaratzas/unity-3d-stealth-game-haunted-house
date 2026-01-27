using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyGame.UI
{
    public class KeyDisplay : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("How many keys are in THIS specific scene? (Set 3 for Sc1, 4 for Sc2, etc)")]
        public int keysInThisLevel = 3;

        private UIDocument m_UIDocument;
        private List<VisualElement> m_KeyIcons = new List<VisualElement>();

        private void Awake()
        {
            m_UIDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (m_UIDocument == null) return;

            var root = m_UIDocument.rootVisualElement;

            // 1. Find the wrapper we made
            var keysWrapper = root.Q<VisualElement>("KeysWrapper");

            if (keysWrapper != null)
            {
                // 2. Find all children named "KeyIcon"
                // This grabs all 5 icons you made in UI Builder
                keysWrapper.Query<VisualElement>("KeyIcon").ToList(m_KeyIcons);
            }

            // 3. Initialize the display
            SetupKeysForLevel();
        }

        private void SetupKeysForLevel()
        {
            // Loop through all 5 icons
            for (int i = 0; i < m_KeyIcons.Count; i++)
            {
                if (i < keysInThisLevel)
                {
                    // Keep visible
                    m_KeyIcons[i].style.display = DisplayStyle.Flex;
                }
                else
                {
                    // Hide the extras (e.g., hide Key 4 and 5 in Scene 1)
                    m_KeyIcons[i].style.display = DisplayStyle.None;
                }
            }
        }

        // Call this when the player picks up a key
        public void RemoveKeyIcon()
        {
            // Find the last visible key and hide it
            // We loop backwards to find the right-most visible key
            for (int i = keysInThisLevel - 1; i >= 0; i--)
            {
                if (m_KeyIcons[i].style.display == DisplayStyle.Flex)
                {
                    m_KeyIcons[i].style.display = DisplayStyle.None;
                    return; // Stop after hiding one
                }
            }
        }
    }
}