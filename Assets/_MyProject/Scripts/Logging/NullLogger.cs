namespace MyGame.Logging
{
    public class NullLogger : IEventLogger
    {
        public void LogEvent(string username, float duration, string result)
        {
            // Intentionally does nothing, this is a no-op logger for the web build where LSL is not supported
        }

        public void SendPulse(string message)
        {
            // Intentionally does nothing, this is a no-op logger for the web build where LSL is not supported
        }
    }
}
