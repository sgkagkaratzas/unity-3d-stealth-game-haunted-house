using UnityEngine;

namespace MyGame.Global
{
    // 1. The Root JSON Object
    [System.Serializable]
    public class GameDataRoot
    {
        public MainMenuData mainMenu;
        public SceneData scene;
    }

    // 2. Main Menu Structure (Ready for your next steps)
    [System.Serializable]
    public class MainMenuData
    {
        public string title;
        public string subtitle;
        public MainMenuButtons buttons;
        public InfoData info;
        public CreditsData credits;
    }

    [System.Serializable]
    public class MainMenuButtons { public string StartButton; public string InfoButton; public string ExitButton; }
    [System.Serializable]
    public class InfoData { public string TitleLabel; public string CreditsScroll; public string BackButton; }
    [System.Serializable]
    public class CreditsData { public string TitleLabel; public string CreditsScroll; public string BackButton; }

    // 3. Scene Structure
    [System.Serializable]
    public class SceneData
    {
        public HelpPopupData HelpPopup;
        public QuestionPopupData QuestionPopup;
        public RatingPopupData RatingPopup;
        public PauseMenuData PauseMenu;
    }

    [System.Serializable]
    public class HelpPopupData { public string HelpContentLabel; public string CloseButton; }
    [System.Serializable]
    public class RatingPopupData { public string RatingContent; public string RatingYesButton; public string RatingNoButton; }
    [System.Serializable]
    public class PauseMenuData { public string PauseContent; public string ResumeButton; public string ExitButton; }

    // 4. Question Popup Structure (This replaces QuestionList)
    [System.Serializable]
    public class QuestionPopupData
    {
        public QuestionData[] QuestionContainer;
        public string CancelQuizButton;
    }

    // 5. The actual Question Data
    [System.Serializable]
    public class QuestionData
    {
        public string id;
        public string q_id;
        public string text;
        public string[] answers;
        public int correctIndex;
    }
}