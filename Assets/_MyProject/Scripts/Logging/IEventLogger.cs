namespace MyGame.Logging
{
    public interface IEventLogger
    {
        void LogEvent(string username, float duration, string result);
        void SendPulse(string message);
    }
}
