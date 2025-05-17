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
            if (!Utilities.IsValid(audioSource))
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        private void PlaySound(AudioClip sound)
        {
            if (!Utilities.IsValid(sound) || !Utilities.IsValid(audioSource)) return;
            if(audioSource.isPlaying){
                Debug.LogWarning("AudioSource is already playing");
                return;
            }
            audioSource.PlayOneShot(sound);
        }

        public void PlayStartSound()
        {
            PlaySound(startSound);
        }

        public void PlayClearSound()
        {
            PlaySound(clearSound);
        }

        public void PlayCorrectSound()
        {
            PlaySound(correctSound);
        }

        public void PlayWrongSound()
        {
            PlaySound(wrongSound);
        }
    }
}
