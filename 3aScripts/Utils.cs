using UnityEngine;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

namespace ArthurProduct.AnomalyDetection
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Utils : UdonSharpBehaviour
    {
        [Header("Random Settings")]
        [Tooltip("Random seed for deterministic behavior")]
        [SerializeField]
        private ushort randomSeed = 0;

        [SerializeField] public GameManager gameManager;

        private System.Random random;

        public void Start()
        {
            random = new System.Random(randomSeed);
            if (randomSeed > ushort.MaxValue)
            {
                Debug.LogError("Random seed is greater than ushort.MaxValue");
            }
        }

        public byte GetRandomStageIndex(byte maxIndex, byte previousIndex)
        {
            byte randomIndex;
            do
            {
                randomIndex = (byte)Random.Range(0, maxIndex);
            } while (previousIndex != 255 && randomIndex == previousIndex); // 255 is used to indicate no previous index
            return randomIndex;
        }
    }
}