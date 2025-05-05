using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ArthurProduct.AnomalyDetection
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Bgm : UdonSharpBehaviour
    {
        [Header("BGM Tracks")]
        [Tooltip("BGM played before game starts")]
        [SerializeField]
        private AudioClip preGameBgm;

        [Tooltip("BGM played during gameplay")]
        [SerializeField]
        private AudioClip inGameBgm;

        [Tooltip("BGM played after game clear")]
        [SerializeField]
        private AudioClip clearBgm;

        [SerializeField]
        private AudioSource audioSource;

        private void Start()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        public void PlayPreGameBgm()
        {
            if (preGameBgm != null && audioSource != null)
            {
                audioSource.clip = preGameBgm;
                audioSource.Play();
            }
        }

        public void PlayInGameBgm()
        {
            if (inGameBgm != null && audioSource != null)
            {
                audioSource.clip = inGameBgm;
                audioSource.Play();
            }
        }

        public void PlayClearBgm()
        {
            if (clearBgm != null && audioSource != null)
            {
                audioSource.clip = clearBgm;
                audioSource.Play();
            }
        }

        public void StopBgm()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }
    }
}
