using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ArthurProduct.AnomalyDetection
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Stage : UdonSharpBehaviour
    {
        [Header("Stage Settings")]
        [Tooltip("Stage index for identification")]
        [SerializeField]
        private byte stageIndex;

        [Tooltip("Number of successful attempts needed to win")]
        [SerializeField]
        public byte maxSuccessStack = 10;

        [Tooltip("Probability of anomaly appearing (0-100)")]
        [SerializeField]
        public byte anomalyProbability = 65;

        [Tooltip("Base stage object")]
        [SerializeField]
        private GameObject baseStageObject;

        [Tooltip("Anomaly stage objects")]
        [SerializeField]
        private GameObject[] anomalyStageObjects;

        [Header("Progress Objects")]
        [Tooltip("Progress objects that will be activated based on success stack")]
        [SerializeField]
        private GameObject[] progressObjects;

        [Header("Clear Objects")] 
        [Tooltip("Object to display when the stage is cleared")]
        [SerializeField]
        private GameObject clearObject; 

        [Header("Teleport Points")]
        [Tooltip("Start point for this stage")]
        [SerializeField]
        public Transform startPoint;

        private void Start()
        {
            if (progressObjects.Length != maxSuccessStack)
            {
                Debug.LogError($"Progress objects count ({progressObjects.Length}) must match maxSuccessStack ({maxSuccessStack})");
                return;
            }
        }

        public byte GetStageIndex() => stageIndex;

        public byte GetAnomalyStageCount()
        {
            return Utilities.IsValid(anomalyStageObjects) ? (byte)anomalyStageObjects.Length : (byte)0;
        }

        public void UpdateStage(sbyte anomalyStageIndex)
        {
            Debug.Log("currentAnomalyStageIndex: " + anomalyStageIndex);

            if (Utilities.IsValid(baseStageObject))
            {
                baseStageObject.SetActive(false);
            }

            foreach (GameObject anomalyStage in anomalyStageObjects)
            {
                if (Utilities.IsValid(anomalyStage))
                {
                    anomalyStage.SetActive(false);
                }
            }

            if (anomalyStageIndex == -1)
            {
                if (Utilities.IsValid(baseStageObject))
                {
                    baseStageObject.SetActive(true);
                }
            }
            else if (anomalyStageIndex >= 0 && anomalyStageIndex < anomalyStageObjects.Length)
            {
                GameObject anomalyStage = anomalyStageObjects[anomalyStageIndex];
                if (Utilities.IsValid(anomalyStage))
                {
                    anomalyStage.SetActive(true);
                }
            }
        }

        public void UpdateProgressObjects(byte successStack)
        {
            if (!Utilities.IsValid(progressObjects)) return;

            for (byte i = 0; i < progressObjects.Length; i++)
            {
                if (Utilities.IsValid(progressObjects[i]))
                {
                    progressObjects[i].SetActive(i < successStack);
                }
            }
        }

        public void TeleportPlayerToStartPoint()
        {
            VRCPlayerApi player = Networking.LocalPlayer;
            if (!Utilities.IsValid(player) || !player.IsValid() || !Utilities.IsValid(startPoint)) return;

            player.TeleportTo(startPoint.position, startPoint.rotation);
        }

        public void Reset()
        {
            UpdateStage(-1);
            UpdateProgressObjects(0);
            ToggleClearObjects(false); 
        }

        public void ToggleClearObjects(bool active)
        {
            if (!Utilities.IsValid(clearObject)) return;

            clearObject.SetActive(active); 
        }
    }
}