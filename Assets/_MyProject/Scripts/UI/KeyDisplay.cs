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

            var keysWrapper = root.Q<VisualElement>("KeysWrapper");

            if (keysWrapper != null)
            {
                // Cache references to UI key icons defined in the UXML
                keysWrapper.Query<VisualElement>("KeyIcon").ToList(m_KeyIcons);
            }

            SetupKeysForLevel();
        }

        private void SetupKeysForLevel()
        {
            for (int i = 0; i < m_KeyIcons.Count; i++)
            {
                if (i < keysInThisLevel)
                {
                    m_KeyIcons[i].style.display = DisplayStyle.Flex;
                }
                else
                {
                    m_KeyIcons[i].style.display = DisplayStyle.None;
                }
            }
        }

        public void RemoveKeyIcon()
        {
            for (int i = keysInThisLevel - 1; i >= 0; i--)
            {
                if (m_KeyIcons[i].style.display == DisplayStyle.Flex)
                {
                    m_KeyIcons[i].style.display = DisplayStyle.None;
                    return;
                }
            }
        }
    }
}
