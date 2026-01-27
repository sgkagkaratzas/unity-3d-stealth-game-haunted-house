using UnityEngine;
using MyGame.Player;

namespace MyGame.Obstacles
{
    public class Door : MonoBehaviour
    {
        public string KeyName;

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
