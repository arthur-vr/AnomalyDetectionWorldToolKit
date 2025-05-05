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

        public override void Interact()
        {
            float currentTime = Time.time;
            if (currentTime - lastPressTime >= cooldownTime)
            {
                gameManager.CheckAnswer(false);
                lastPressTime = currentTime;
            }
        }
    }
}