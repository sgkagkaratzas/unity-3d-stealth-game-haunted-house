using UnityEngine;
using MyGame.Player;

namespace MyGame.Obstacles
{
    public class Door : MonoBehaviour
    {
        // Comments cleaned: non-essential comments removed
        public string KeyName;
        public bool IsOpen; // Added a new field to track if the door is open

        private void OnCollisionEnter(Collision collision)
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player == null) return;

            if (player.OwnKey(KeyName))
            {
                player.PlayDoorSound();

                Destroy(gameObject);
            }
        }
    }
}
