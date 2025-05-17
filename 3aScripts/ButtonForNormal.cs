using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ArthurProduct.AnomalyDetection
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ButtonForNormal : UdonSharpBehaviour
    {
        [SerializeField] private GameManager gameManager;
        private const byte COOLDOWN_TIME = 1;
        private byte rapidPressCount = 0;

        private void Start()
        {
            if (!Utilities.IsValid(gameManager))
            {
                Debug.LogError("GameManager is not assigned");
            }
        }

        public override void Interact()
        {
            if (gameManager.isBanned) {
                gameObject.SetActive(false);
                return;
            }
            float currentTime = Time.time;
            if (currentTime - gameManager.lastPressTime >= COOLDOWN_TIME)
            {
                if (!Utilities.IsValid(gameManager)) return;
                
                // Check if this press is within the rapid press window
                if (currentTime - gameManager.lastPressTime <= gameManager.rapidPressWindow)
                {
                    rapidPressCount++;
                }
                else
                {
                    rapidPressCount = 1; // Reset count if outside window
                }

                if (rapidPressCount >= gameManager.maxRapidPresses)
                {
                    gameManager.BanPlayer();
                    return;
                }

                gameManager.CheckAnswer(false);
                gameManager.lastPressTime = currentTime;
            }
        }
    }
}