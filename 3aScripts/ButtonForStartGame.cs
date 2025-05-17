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

        [SerializeField]
        [Tooltip("Stage index to start from (0-based)")]
        private byte startStageIndex;

        private const byte COOLDOWN_TIME = 1;

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
                if (Utilities.IsValid(gameManager))
                {
                    gameManager.StartGame(startStageIndex);
                }
                gameManager.lastPressTime = currentTime;
            }
        }
    }
}