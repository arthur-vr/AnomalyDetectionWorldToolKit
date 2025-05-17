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
            if (!Utilities.IsValid(audioSource))
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        private void PlayBgm(AudioClip clip)
        {
            if (!Utilities.IsValid(clip) || !Utilities.IsValid(audioSource)) return;
            audioSource.clip = clip;
            audioSource.Play();
        }

        public void PlayPreGameBgm()
        {
            PlayBgm(preGameBgm);
        }

        public void PlayInGameBgm()
        {
            PlayBgm(inGameBgm);
        }

        public void PlayClearBgm()
        {
            PlayBgm(clearBgm);
        }

        public void StopBgm()
        {
            if (!Utilities.IsValid(audioSource)) return;
            audioSource.Stop();
        }
    }
}
