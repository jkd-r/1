using UnityEngine;

namespace ProtocolEMR.Systems
{
    /// <summary>
    /// Example door that can be opened/closed by player interaction.
    /// Demonstrates inheritance from InteractableObject for specific behaviors.
    /// Animates door rotation when interacted with.
    /// </summary>
    public class InteractableDoor : InteractableObject
    {
        [Header("Door Settings")]
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openSpeed = 2.0f;
        [SerializeField] private bool startOpen = false;
        [SerializeField] private Transform doorPivot;

        [Header("Door Audio")]
        [SerializeField] private AudioClip doorOpenSound;
        [SerializeField] private AudioClip doorCloseSound;
        [SerializeField] private float doorSoundVolume = 0.7f;

        private bool isOpen;
        private Quaternion closedRotation;
        private Quaternion openRotation;
        private bool isAnimating = false;
        private AudioSource audioSource;

        private void Start()
        {
            if (doorPivot == null)
                doorPivot = transform;

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1f;
                audioSource.maxDistance = 50f;
            }

            closedRotation = doorPivot.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);

            isOpen = startOpen;
            if (startOpen)
            {
                doorPivot.localRotation = openRotation;
            }
        }

        private void Update()
        {
            if (isAnimating)
            {
                Quaternion targetRotation = isOpen ? openRotation : closedRotation;
                doorPivot.localRotation = Quaternion.Slerp(doorPivot.localRotation, targetRotation, Time.deltaTime * openSpeed);

                if (Quaternion.Angle(doorPivot.localRotation, targetRotation) < 0.1f)
                {
                    doorPivot.localRotation = targetRotation;
                    isAnimating = false;
                }
            }
        }

        protected override void PerformInteraction(GameObject interactor)
        {
            ToggleDoor();
        }

        private void ToggleDoor()
        {
            isOpen = !isOpen;
            isAnimating = true;

            AudioClip soundToPlay = isOpen ? doorOpenSound : doorCloseSound;
            if (soundToPlay != null && audioSource != null)
            {
                audioSource.PlayOneShot(soundToPlay, doorSoundVolume);
            }

            Debug.Log($"Door {gameObject.name} is now {(isOpen ? "open" : "closed")}");
        }

        public void OpenDoor()
        {
            if (!isOpen)
            {
                isOpen = true;
                isAnimating = true;
            }
        }

        public void CloseDoor()
        {
            if (isOpen)
            {
                isOpen = false;
                isAnimating = true;
            }
        }

        public bool IsOpen()
        {
            return isOpen;
        }
    }
}
