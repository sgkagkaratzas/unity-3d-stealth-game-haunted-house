using UnityEngine;

namespace MyGame.Logging
{
    public static class LoggerFactory
    {
        private static IEventLogger _logger;

        public static IEventLogger GetLogger()
        {
            if (_logger != null)
                return _logger;

#if UNITY_EDITOR || UNITY_STANDALONE
            // Try to find existing LslLogger in scene
            var lsl = Object.FindFirstObjectByType<LslLogger>();
            if (lsl != null)
            {
                _logger = lsl;
                return _logger;
            }

            Debug.LogWarning("LslLogger not found in scene. Using NullLogger.");
#endif

            _logger = new NullLogger();
            return _logger;
        }
    }
}
