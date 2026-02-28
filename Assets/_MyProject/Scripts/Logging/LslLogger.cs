using System;
using UnityEngine;

namespace MyGame.Logging
{
    public class LslLogger : MonoBehaviour, IEventLogger
    {
        private static LslLogger _instance;

        public static LslLogger Instance
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                if (_instance == null)
                {
                    var go = new GameObject("LslLogger");
                    _instance = go.AddComponent<LslLogger>();
                    DontDestroyOnLoad(go);
                }
#endif
                return _instance;
            }
        }

        private IntPtr outlet = IntPtr.Zero;
        private IntPtr info = IntPtr.Zero;

        void Awake()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            info = LabStreamingLayer.lsl_create_streaminfo(
                "StealthGame_Events",
                "Markers",
                1,
                0,
                LabStreamingLayer.cf_string,
                Guid.NewGuid().ToString()
            );

            outlet = LabStreamingLayer.lsl_create_outlet(info, 0, 360);
#endif
        }

        // REQUIRED by IEventLogger
        public void LogEvent(string username, float duration, string result)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (outlet == IntPtr.Zero) return;

            string message = $"{username},{duration:F2},{result}";
            LabStreamingLayer.lsl_push_sample_str(outlet, new[] { message });
#endif
        }

        // This is what GameEnding currently calls
        public void LogAndRelease(string username, float duration, string result)
        {
            LogEvent(username, duration, result);
        }

        public void SendPulse(string message)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (outlet == IntPtr.Zero) return;

            LabStreamingLayer.lsl_push_sample_str(outlet, new[] { message });
#endif
        }

        void OnApplicationQuit()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (outlet != IntPtr.Zero)
            {
                LabStreamingLayer.lsl_destroy_outlet(outlet);
                outlet = IntPtr.Zero;
            }
#endif
        }
    }
}
