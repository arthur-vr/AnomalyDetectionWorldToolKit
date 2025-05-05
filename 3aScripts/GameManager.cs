using UnityEngine;
using VRC.SDKBase;
using UdonSharp;
using System.Collections.Generic;

namespace ArthurProduct.AnomalyDetection
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class GameManager : UdonSharpBehaviour
    {
        [Header("Spawn Points")]
        [Tooltip("Initial spawn point when players join")]
        [SerializeField]
        private Transform initialSpawnPoint;

        [Tooltip("Respawn point during gameplay")]
        [SerializeField]
        private Transform gameStartPoint;

        [Tooltip("Exit point when game is completed")]
        [SerializeField]
        private Transform exitPoint;

        [Header("Game Settings")]
        [Tooltip("Number of successful attempts needed to win")]
        [SerializeField]
        private byte maxSuccessStack = 10;

        [Tooltip("Probability of anomaly appearing (0-100)")]
        [SerializeField]
        public byte anomalyProbability = 65;

        [Header("Stage Settings")]
        [Tooltip("Base stage objects that will be modified")]
        [SerializeField]
        private GameObject baseStage;

        [Tooltip("Anomaly stages that can be randomly selected")]
        [SerializeField]
        private GameObject[] anomalyStages;

        [Header("UI Elements")]
        [Tooltip("Progress objects that will be activated based on success stack")]
        [SerializeField]
        private GameObject[] progressObjects;

        [Header("Utils")]
        [Tooltip("Utils component for random operations")]
        [SerializeField]
        private Utils utils;

        [Header("Sound Effects")]
        [Tooltip("Sound effect component")]
        [SerializeField]
        private SoundEffect soundEffect;

        [Header("BGM")]
        [Tooltip("BGM component")]
        [SerializeField]
        private Bgm bgm;

        [Header("Ban Settings")]
        [Tooltip("Point where banned players are teleported")]
        [SerializeField]
        private Transform banRespawnPoint;

        [Tooltip("Number of rapid presses before ban")]
        [SerializeField]
        public byte maxRapidPresses = 7;

        [Tooltip("Time window for rapid presses in seconds")]
        [SerializeField]
        public float rapidPressWindow = 3.0f;

        [UdonSynced(UdonSyncMode.None)]
        private sbyte successStack = -1;  // -1: Not started, 0-maxSuccessStack: In progress, maxSuccessStack: Completed

        [UdonSynced(UdonSyncMode.None)]
        private sbyte anomalyStageIndex = -1;

        private bool isBanned = false;

        private void Start()
        {
            ResetGame();
            ValidateVariables();
        }
        public void ResetGame()
        {
            bool isOwner = Networking.IsOwner(gameObject);
            if (isOwner)
            {
                successStack = -1;
                anomalyStageIndex = -1;
                RequestSerializationForSuccessStack();
            }
        }
        public void StartGame()
        {
            if (isBanned)
            {
                BanBehaviour();
                return;
            }
            successStack = 0;
            if (soundEffect != null)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(PlayStartSound));
            }
            RequestSerializationForSuccessStack();
        }

        private void ValidateVariables()
        {
            ValidateProgressObjects();
        }

        private void ValidateProgressObjects()
        {
            if (progressObjects == null)
            {
                Debug.LogError("Progress objects array is null");
                return;
            }

            // Ensure all progress objects are initially inactive
            foreach (GameObject obj in progressObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }

            if (progressObjects.Length != maxSuccessStack)
            {
                Debug.LogError($"Progress objects count ({progressObjects.Length}) must match maxSuccessStack ({maxSuccessStack})");
                return;
            }

        }

        private void UpdateBySuccessStack()
        {
            VRCPlayerApi player = Networking.LocalPlayer;
            if (player == null || !player.IsValid()) return;

            if (successStack >= maxSuccessStack)
            {
                successStack = (sbyte)maxSuccessStack;
                if (soundEffect != null)
                {
                    soundEffect.PlayClearSound();
                }
                // Game completed - teleport to exit
                if (exitPoint != null)
                {
                    player.TeleportTo(exitPoint.position, exitPoint.rotation);
                }
                if (bgm != null)
                {
                    bgm.PlayClearBgm();
                }
            }
            else if (successStack >= 0)
            {
                if (gameStartPoint != null)
                {
                    player.TeleportTo(gameStartPoint.position, gameStartPoint.rotation);
                }
                if (bgm != null)
                {
                    bgm.PlayInGameBgm();
                }
            }
            else
            {
                if (initialSpawnPoint != null)
                {
                    player.TeleportTo(initialSpawnPoint.position, initialSpawnPoint.rotation);
                }
                if (bgm != null)
                {
                    bgm.PlayPreGameBgm();
                }
            }
        }
        private void UpdateProgressObjects()
        {
            if (progressObjects == null || progressObjects.Length != maxSuccessStack) return;

            for (byte i = 0; i < progressObjects.Length; i++)
            {
                if (progressObjects[i] != null)
                {
                    progressObjects[i].SetActive(i < successStack);
                }
            }
        }
        private void UpdateStage()
        {
            foreach (GameObject stage in anomalyStages)
            {
                stage.SetActive(false);
            }

            if (anomalyStageIndex == -1)
            {
                baseStage.SetActive(true);
            }
            else
            {
                baseStage.SetActive(false);
                anomalyStages[anomalyStageIndex].SetActive(true);
            }
        }

        private void UpdateBySyncedVariables()
        {
            if (isBanned)
            {
                BanBehaviour();
                return;
            }
            UpdateStage();
            UpdateProgressObjects();
            UpdateBySuccessStack();
        }
        private void RequestSerializationForSuccessStack()
        {
            UpdateBySyncedVariables(); //everyone execute this
            RequestSerializationByOwner();
        }
        public override void OnDeserialization()
        {
            UpdateBySyncedVariables();
        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player == Networking.LocalPlayer)
            {
                UpdateBySyncedVariables();
            }
        }

        private void RequestSerializationByOwner()
        {
            if (Networking.IsOwner(gameObject))
            {
                RequestSerialization();
            }
            else
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                RequestSerialization();
            }
        }

        public void CheckAnswer(bool hasAnomaly)
        {
            bool isValidNormal = anomalyStageIndex == -1 && !hasAnomaly;
            bool isValidAnomaly = anomalyStageIndex != -1 && hasAnomaly;
            bool isCorrect = isValidNormal || isValidAnomaly;

            if (isCorrect)
            {
                successStack++;
                if (successStack < maxSuccessStack && successStack >= 0)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(PlayCorrectSound));
                }
            }
            else
            {
                successStack = 0;
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(PlayWrongSound));
            }
            UpdateAnomalyStageIndex();
            RequestSerializationForSuccessStack();
        }

        private void UpdateAnomalyStageIndex()
        {
            bool hasAnomaly = utils.CheckAnomalyProbability();
            if (hasAnomaly)
            {
                byte previousIndex = anomalyStageIndex == -1 ? (byte)255 : (byte)anomalyStageIndex;
                anomalyStageIndex = (sbyte)utils.GetRandomStageIndex((byte)anomalyStages.Length, previousIndex);
            }
            else
            {
                anomalyStageIndex = -1;
            }
        }

        public void PlayStartSound()
        {
            if (soundEffect != null)
            {
                soundEffect.PlayStartSound();
            }
        }
        public void PlayCorrectSound()
        {
            if (soundEffect != null)
            {
                soundEffect.PlayCorrectSound();
            }
        }
        public void PlayWrongSound()
        {
            if (soundEffect != null)
            {
                soundEffect.PlayWrongSound();
            }
        }
        public void PlayClearSound()
        {
            if (soundEffect != null)
            {
                soundEffect.PlayClearSound();
            }
        }

        public void BanPlayer()
        {
            isBanned = true;
            BanBehaviour();
        }

        private void BanBehaviour()
        {
            if (isBanned)
            {
                VRCPlayerApi player = Networking.LocalPlayer;
                if (player != null && player.IsValid())
                {
                    player.TeleportTo(banRespawnPoint.position, banRespawnPoint.rotation);
                }
            }
        }
    }
}