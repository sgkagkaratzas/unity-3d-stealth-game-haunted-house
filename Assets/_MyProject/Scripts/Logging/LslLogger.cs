using System;
using UnityEngine;

namespace MyGame.Logging
{
    public class LslLogger : MonoBehaviour, IEventLogger
    {
        private static LslLogger _instance;
        public static LslLogger Instance => _instance;

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

            if (!LabStreamingLayer.IsAvailable)
            {
                Debug.LogWarning("LSL native library unavailable. Logging disabled.");
                return;
            }

            try
            {
                if (outlet == IntPtr.Zero)
                {
                    info = LabStreamingLayer.lsl_create_streaminfo(
                        "StealthGame_Events",
                        "Markers",
                        1,
                        0,
                        LabStreamingLayer.cf_string,
                        Guid.NewGuid().ToString()
                    );

                    outlet = LabStreamingLayer.lsl_create_outlet(info, 0, 360);
                    Debug.Log("[LSL] Outlet created successfully.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"LSL failed to create outlet: {e.Message}");
                info = IntPtr.Zero;
                outlet = IntPtr.Zero;
            }
#endif
        }

        public void LogEvent(string username, float duration, string result)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (outlet == IntPtr.Zero) return;

            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string message = $"{sceneName}|{username}|{result}|{duration:F2}|";
            LabStreamingLayer.lsl_push_sample_str(outlet, new[] { message });
#endif
        }

        public void SendPulse(string message)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (outlet == IntPtr.Zero) return;

            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string payload = $"{sceneName}||{message}||";
            LabStreamingLayer.lsl_push_sample_str(outlet, new[] { payload });
#endif
        }

        void OnApplicationQuit()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (outlet != IntPtr.Zero)
            {
                LabStreamingLayer.lsl_destroy_outlet(outlet);
                outlet = IntPtr.Zero;
                Debug.Log("[LSL] Outlet destroyed on quit.");
            }
#endif
        }
    }
}
