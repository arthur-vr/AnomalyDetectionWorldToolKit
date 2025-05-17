using UnityEngine;
using VRC.SDKBase;
using UdonSharp;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;
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
        private Transform initialSpawnPointCache;

        [Tooltip("Exit point when game is completed")]
        [SerializeField]
        private Transform exitPoint;

        [Header("Game Settings")]

        [Header("Stage Settings")]
        [Tooltip("Array of stage objects")]
        [SerializeField]
        private Stage[] stages;

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

        [Tooltip("Enable ban")]
        [SerializeField]
        private bool isBanEnabled = false;

        [Tooltip("Number of rapid presses before ban")]
        [SerializeField]
        public byte maxRapidPresses = 5;

        [Tooltip("Time window for rapid presses in seconds")]
        [SerializeField]
        public byte rapidPressWindow = 3;
        [Tooltip("Flag to check if player is banned")]
        public bool isBanned = false;

        [UdonSynced(UdonSyncMode.None)]
        private sbyte successStack = -1;  // -1: Not started, 0-maxSuccessStack: In progress, maxSuccessStack: Completed

        [UdonSynced(UdonSyncMode.None)]
        private sbyte currentStageIndex = -1;

        [UdonSynced(UdonSyncMode.None)]
        private sbyte anomalyStageIndex = -1;

        [Header("Public but just shared for other scripts")]
        [HideInInspector]
        [Tooltip("Time of last press")]
        public float lastPressTime = 0;

        private void Start()
        {
            ResetGame();
            ValidateVariables();
            initialSpawnPointCache = initialSpawnPoint;
        }
        private void ResetSyncVariables()
        {
            successStack = -1;
            currentStageIndex = -1;
            anomalyStageIndex = -1;
        }
        public void ResetGame()
        {
            bool isOwner = Networking.IsOwner(gameObject);
            if (isOwner)
            {
                ResetSyncVariables();
                RequestSerializationForSuccessStack();
            }
        }
        public void StartGame(byte stageIndex = 0)
        {
            successStack = 0;
            currentStageIndex = (sbyte)stageIndex;
            if (Utilities.IsValid(soundEffect))
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayStartSound));
            }
            RequestSerializationForSuccessStack();
        }

        private void ValidateVariables()
        {
            if (!Utilities.IsValid(soundEffect))
            {
                Debug.LogError("SoundEffect is invalid");
            }
            if (!Utilities.IsValid(stages))
            {
                Debug.LogError("Stages is invalid");
            }
        }

        private void UpdateBySuccessStack()
        {

            VRCPlayerApi player = Networking.LocalPlayer;
            if (!player.IsValid()){
                Debug.LogError("Player is invalid");
                return;
            }

            if(currentStageIndex < 0){
                Debug.LogError("currentStageIndex is less than 0: " + currentStageIndex);
                UpdateBgmBySuccessStack();
                return;
            }

            Stage currentStage = stages[currentStageIndex];

            if (successStack >= currentStage.maxSuccessStack)
            {
                successStack = (sbyte)currentStage.maxSuccessStack;
                if (Utilities.IsValid(soundEffect))
                {
                    soundEffect.PlayClearSound();
                }
                if (Utilities.IsValid(exitPoint))
                {
                    player.TeleportTo(exitPoint.position, exitPoint.rotation);
                }
                if (Utilities.IsValid(bgm))
                {
                    bgm.PlayClearBgm();
                }
                if (Utilities.IsValid(currentStage))
                {
                    foreach (Stage stage in stages)
                    {
                        stage.ToggleClearObjects(false);
                    }
                    currentStage.ToggleClearObjects(true);
                }
                //reset to restart
                initialSpawnPoint = initialSpawnPointCache;
            }
            else if (successStack >= 0)
            {
                if (Utilities.IsValid(stages) && currentStageIndex >= 0 && currentStageIndex < stages.Length)
                {
                    if (Utilities.IsValid(currentStage)) 
                    {
                        currentStage.TeleportPlayerToStartPoint();
                    }
                }
                if (Utilities.IsValid(bgm))
                {
                    bgm.PlayInGameBgm();
                }
            }
            else
            {
                if (Utilities.IsValid(initialSpawnPoint))
                {
                    player.TeleportTo(initialSpawnPoint.position, initialSpawnPoint.rotation);
                }
                if (Utilities.IsValid(bgm))
                {
                    bgm.PlayPreGameBgm();
                }
            }
        }
        private void UpdateBgmBySuccessStack(){
            if(successStack >= 0){
                if (Utilities.IsValid(bgm))
                {
                    bgm.PlayInGameBgm();
                }
            }
            else{
                if (Utilities.IsValid(bgm))
                {
                    bgm.PlayPreGameBgm();
                }
            }
        }
        private void UpdateProgressObjects()
        {
            if (!Utilities.IsValid(stages) || currentStageIndex < 0 || currentStageIndex >= stages.Length) return;

            Stage currentStage = stages[currentStageIndex];
            if (Utilities.IsValid(currentStage))
            {
                currentStage.UpdateProgressObjects((byte)successStack);
            }
        }
        private void UpdateStage()
        {

            foreach (Stage stage in stages)
            {
                if (!Utilities.IsValid(stage)) continue;
                stage.transform.gameObject.SetActive(false);
            }
            
            if (!Utilities.IsValid(stages))
            {
                Debug.LogError("stages is invalid");
                return;
            }

            if(currentStageIndex < 0){
                Debug.Log("currentStageIndex is less than 0: " + currentStageIndex);
                return;
            }

            Stage currentStage = stages[currentStageIndex];

            if (Utilities.IsValid(initialSpawnPoint) && Utilities.IsValid(currentStage))
            {
                initialSpawnPoint.position = currentStage.startPoint.position;
                initialSpawnPoint.rotation = currentStage.startPoint.rotation;
            }

            if (Utilities.IsValid(currentStage))
            {
                currentStage.transform.gameObject.SetActive(true);
                currentStage.UpdateStage(anomalyStageIndex);
            }
        }

        private void UpdateBySyncedVariables()
        {
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

            if(currentStageIndex < 0){
                Debug.LogError("currentStageIndex is less than 0: " + currentStageIndex);
                return;
            }
            Stage currentStage = stages[currentStageIndex];
            if(!Utilities.IsValid(currentStage)){
                Debug.LogError("currentStage is invalid");
                return;
            }

            if (isCorrect)
            {
                successStack++;
                if (successStack < currentStage.maxSuccessStack && successStack >= 0)
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayCorrectSound));
                }
            }
            else
            {
                successStack = 0;
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayWrongSound));
            }
            UpdateAnomalyStageIndex();
            RequestSerializationForSuccessStack();
        }

        private bool CheckAnomalyProbability()
        {
            if(currentStageIndex < 0){
                Debug.LogError("currentStageIndex is less than 0: " + currentStageIndex);
                return false;
            }
            Stage currentStage = stages[currentStageIndex];
            if(!Utilities.IsValid(currentStage)){
                Debug.LogError("currentStage is invalid");
                return false;
            }
            return Random.Range(0, 100) < currentStage.anomalyProbability;
        }

        private void UpdateAnomalyStageIndex()
        {
            bool hasAnomaly = CheckAnomalyProbability();
            if (hasAnomaly)
            {
                if(currentStageIndex < 0){
                    Debug.LogError("currentStageIndex is less than 0: " + currentStageIndex);
                    return;
                }
                Stage currentStage = stages[currentStageIndex];
                byte anomalyCount = currentStage.GetAnomalyStageCount();
                anomalyStageIndex = (sbyte)Random.Range(0, anomalyCount);
            }
            else
            {
                anomalyStageIndex = -1;
            }
            RequestSerializationForSuccessStack();
        }

        public void PlayStartSound()
        {
            if (Utilities.IsValid(soundEffect))
            {
                soundEffect.PlayStartSound();
            }
        }
        public void PlayCorrectSound()
        {
            if (Utilities.IsValid(soundEffect))
            {
                soundEffect.PlayCorrectSound();
            }
        }
        public void PlayWrongSound()
        {
            if (Utilities.IsValid(soundEffect))
            {
                soundEffect.PlayWrongSound();
            }
        }
        public void PlayClearSound()
        {
            if (Utilities.IsValid(soundEffect))
            {
                soundEffect.PlayClearSound();
            }
        }

        public void BanPlayer()
        {
            if (!isBanEnabled) return;
            isBanned = true;
        }

    }
}