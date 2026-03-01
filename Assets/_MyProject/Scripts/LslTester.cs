using UnityEngine;
using MyGame.Logging;

public class LslTester : MonoBehaviour
{
    void Update()
    {
        // Press 'T' on your keyboard to force a message
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (LslLogger.Instance != null)
            {
                LslLogger.Instance.SendPulse("TEST_CONNECTION_SUCCESS");
                Debug.Log("<color=green>SUCCESS:</color> Unity fired the LSL test message!");
            }
            else
            {
                Debug.LogError("<color=red>ERROR:</color> LslLogger.Instance is NULL! You forgot to put the LslLogger script in your scene.");
            }
        }
    }
}
