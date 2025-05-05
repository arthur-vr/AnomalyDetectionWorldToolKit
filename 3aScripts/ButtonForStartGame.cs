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

        private float cooldownTime = 1.0f;

        private float lastPressTime = 0f;

        public override void Interact()
        {
            float currentTime = Time.time;
            if (currentTime - lastPressTime >= cooldownTime)
            {
                if (gameManager != null)
                {
                    gameManager.StartGame();
                }
                lastPressTime = currentTime;
            }
        }
    }
}
