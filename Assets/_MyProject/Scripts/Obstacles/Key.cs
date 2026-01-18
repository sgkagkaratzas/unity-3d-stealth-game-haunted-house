using UnityEngine;
using MyGame.Player;

namespace MyGame.Obstacles
{
    public class Key : MonoBehaviour
    {
        public string KeyName;

        private void OnTriggerEnter(Collider other)
        {
            PlayerMovement player = other.gameObject.GetComponent<PlayerMovement>();
            if (player == null) return;

            player.AddKey(KeyName);
            Destroy(gameObject);
        }
    }
}
