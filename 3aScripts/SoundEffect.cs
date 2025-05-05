using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ArthurProduct.AnomalyDetection
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SoundEffect : UdonSharpBehaviour
    {
        [Header("Game State Sounds")]
        [Tooltip("Sound played when game starts")]
        [SerializeField]
        private AudioClip startSound;

        [Tooltip("Sound played when game is cleared")]
        [SerializeField]
        private AudioClip clearSound;

        [Header("Answer Sounds")]
        [Tooltip("Sound played when answer is correct")]
        [SerializeField]
        private AudioClip correctSound;

        [Tooltip("Sound played when answer is wrong")]
        [SerializeField]
        private AudioClip wrongSound;

        [SerializeField]
        private AudioSource audioSource;

        private void Start()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        public void PlayStartSound()
        {
            if (startSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(startSound);
            }
        }

        public void PlayClearSound()
        {
            if (clearSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(clearSound);
            }
        }

        public void PlayCorrectSound()
        {
            if (correctSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(correctSound);
            }
        }

        public void PlayWrongSound()
        {
            if (wrongSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(wrongSound);
            }
        }
    }
}
