using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ArthurProduct.AnomalyDetection
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ButtonForStartGame : UdonSharpBehaviour
    {
        [SerializeField]
        private GameManager gameManager;

        private const byte COOLDOWN_TIME = 1;

        public override void Interact()
        {
            if (gameManager.isBanned) {
                gameObject.SetActive(false);
                return;
            }
            float currentTime = Time.time;
            if (currentTime - gameManager.lastPressTime >= COOLDOWN_TIME)
            {
                if (gameManager != null)
                {
                    gameManager.StartGame();
                }
                gameManager.lastPressTime = currentTime;
            }
        }
    }
}