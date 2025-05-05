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
        private float cooldownTime = 1.0f;
        private float lastPressTime = 0f;
        private int rapidPressCount = 0;

        public override void Interact()
        {
            float currentTime = Time.time;
            if (currentTime - lastPressTime >= cooldownTime)
            {
                if (gameManager == null) return;
                
                // Check if this press is within the rapid press window
                if (currentTime - lastPressTime <= gameManager.rapidPressWindow)
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
                lastPressTime = currentTime;
            }
        }
    }
}