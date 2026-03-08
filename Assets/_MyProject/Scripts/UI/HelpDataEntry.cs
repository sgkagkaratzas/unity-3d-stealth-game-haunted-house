using UnityEngine;

namespace MyGame.UI
{
    [System.Serializable]
    public class HelpDataEntry
    {
        public string id;
        public string text;
    }

    [System.Serializable]
    public class HelpDataList
    {
        public HelpDataEntry[] helpers;
    }
}