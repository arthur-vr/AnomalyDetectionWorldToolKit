using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ArthurProduct.AnomalyDetection
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Enemy : UdonSharpBehaviour
    {
        [Header("Game Manager")]
        [Tooltip("Reference to the GameManager")]
        [SerializeField]
        private GameManager gameManager;

        [Header("Detection Settings")]
        [Tooltip("Detection radius - player enters this range, enemy starts chasing")]
        [SerializeField]
        private float detectionRadius = 5f;

        [Tooltip("Speed at which enemy moves towards player")]
        [SerializeField]
        private float chaseSpeed = 3f;

        [Header("Audio Settings")]
        [Tooltip("Audio source for enemy sounds")]
        [SerializeField]
        private AudioSource audioSource;

        [Tooltip("Sound to play when player enters detection radius")]
        [SerializeField]
        private AudioClip detectionSound;

        [Header("Animation Settings")]
        [Tooltip("Animator component for enemy animations")]
        [SerializeField]
        private Animator animator;

        [Header("Enemy State")]
        [Tooltip("Is enemy currently chasing player")]
        private bool isChasing = false;

        private VRCPlayerApi localPlayer;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private bool hasTriggered = false;
        private float lastDistanceCheckTime = 0f;
        private float distanceCheckInterval = 0.2f;

        void Start()
        {
            ValidateComponents();
            InitializeEnemy();
        }

        void Update()
        {
            if (!Utilities.IsValid(localPlayer) || !localPlayer.IsValid()) return;

            if (isChasing)
            {
                ChasePlayer();
            }
            else
            {
                // Check distance at intervals
                if (Time.time - lastDistanceCheckTime >= distanceCheckInterval)
                {
                    CheckPlayerDistance();
                    lastDistanceCheckTime = Time.time;
                }
            }
        }

        private void ValidateComponents()
        {
            if (!Utilities.IsValid(gameManager))
            {
                Debug.LogError("GameManager is not assigned to Enemy");
            }
            if (!Utilities.IsValid(audioSource))
            {
                Debug.LogError("AudioSource is not assigned to Enemy");
            }
            if (!Utilities.IsValid(animator))
            {
                Debug.LogError("Animator is not assigned to Enemy");
            }
        }

        private void InitializeEnemy()
        {
            localPlayer = Networking.LocalPlayer;
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            
            if (Utilities.IsValid(animator))
            {
                animator.gameObject.SetActive(false);
            }
        }

        private void CheckPlayerDistance()
        {
            if (!Utilities.IsValid(localPlayer) || !localPlayer.IsValid()) return;

            float distance = Vector3.Distance(transform.position, localPlayer.GetPosition());
            
            if (!isChasing && distance <= detectionRadius)
            {
                StartChasing();
            }
        }

        private void StartChasing()
        {
            isChasing = true;
            hasTriggered = false;
            
            // Play detection sound
            if (Utilities.IsValid(audioSource) && Utilities.IsValid(detectionSound))
            {
                audioSource.PlayOneShot(detectionSound);
            }
            
            // Activate animator
            if (Utilities.IsValid(animator))
            {
                animator.gameObject.SetActive(true);
                animator.SetBool("IsChasing", true);
            }
        }

        private void ChasePlayer()
        {
            if (!Utilities.IsValid(localPlayer) || !localPlayer.IsValid()) return;

            Vector3 playerPosition = localPlayer.GetPosition();
            Vector3 direction = (playerPosition - transform.position).normalized;
            
            // Move towards player
            transform.position = Vector3.MoveTowards(transform.position, playerPosition, chaseSpeed * Time.deltaTime);
            
            // Look at player
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player == Networking.LocalPlayer)
            {
                localPlayer = player;
            }
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (player == localPlayer)
            {
                ResetEnemy();
            }
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (hasTriggered) return;
            if (!Utilities.IsValid(player)) return;
            if (player == localPlayer)
            {
                TriggerFailure();
            }
        }

        private void TriggerFailure()
        {
            if (!Utilities.IsValid(gameManager)) return;

            hasTriggered = true;
            
            // Force failure in game
            gameManager.CheckAnswer(false);
            
            // Reset enemy after triggering
            ResetEnemy();
        }

        public void ResetEnemy()
        {
            isChasing = false;
            hasTriggered = false;
            
            // Reset position and rotation
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            
            // Stop audio
            if (Utilities.IsValid(audioSource))
            {
                audioSource.Stop();
            }
            
            // Deactivate animator
            if (Utilities.IsValid(animator))
            {
                animator.SetBool("IsChasing", false);
                animator.gameObject.SetActive(false);
            }
        }

        void OnDrawGizmosSelected()
        {
            // Draw detection radius in scene view
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}