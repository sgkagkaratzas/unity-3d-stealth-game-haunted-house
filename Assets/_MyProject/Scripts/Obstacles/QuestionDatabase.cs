using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Obstacles
{
    [System.Serializable]
    public class QuestionData
    {
        // id links this question to a Key's KeyName
        public string id;
        public string q_id;
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
