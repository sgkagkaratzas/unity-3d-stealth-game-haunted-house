using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Obstacles
{
    [System.Serializable]
    public class QuestionData
    {
        public string id;           // Links to the Key (e.g. "key0001")
        public string q_id;         // Unique ID for this specific question (e.g. "k1_a")
        public string text;
        public string[] answers;
        public int correctIndex;
    }

    [System.Serializable]
    public class QuestionList
    {
        public QuestionData[] questions;
    }
}
