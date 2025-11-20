using UnityEngine;
using System.Collections.Generic;

namespace ProtocolEMR.Core.Audio
{
    /// <summary>
    /// Surface types for contextual audio.
    /// </summary>
    public enum SurfaceType
    {
        Concrete,
        Metal,
        Carpet,
        Wood,
        Glass,
        Digital
    }

    /// <summary>
    /// Manages audio playback for footsteps, breathing, and movement sounds.
    /// Handles surface detection and contextual audio selection based on player location.
    /// Supports audio pooling for efficient sound playback.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource footstepSource;
        [SerializeField] private AudioSource breathingSource;
        [SerializeField] private AudioSource landingSource;

        [Header("Footstep Settings")]
        [SerializeField] private float footstepVolume = 0.5f;
        [SerializeField] private float pitchVariation = 0.1f;
        [SerializeField] private float minPitch = 0.9f;
        [SerializeField] private float maxPitch = 1.1f;

        [Header("Breathing Settings")]
        [SerializeField] private float breathingVolume = 0.3f;

        [Header("Surface Detection")]
        [SerializeField] private float surfaceCheckDistance = 0.5f;
        [SerializeField] private LayerMask surfaceLayer;

        private Dictionary<SurfaceType, AudioClip[]> footstepClips;
        private Dictionary<SurfaceType, AudioClip> breathingClips;
        private Dictionary<SurfaceType, AudioClip> landingClips;

        private SurfaceType currentSurface = SurfaceType.Concrete;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioDictionaries();
        }

        private void InitializeAudioDictionaries()
        {
            footstepClips = new Dictionary<SurfaceType, AudioClip[]>();
            breathingClips = new Dictionary<SurfaceType, AudioClip>();
            landingClips = new Dictionary<SurfaceType, AudioClip>();

            foreach (SurfaceType surface in System.Enum.GetValues(typeof(SurfaceType)))
            {
                footstepClips[surface] = LoadFootstepClips(surface);
                breathingClips[surface] = LoadBreathingClip(surface);
                landingClips[surface] = LoadLandingClip(surface);
            }
        }

        private AudioClip[] LoadFootstepClips(SurfaceType surface)
        {
            string path = $"Audio/Footsteps/{surface.ToString()}";
            return Resources.LoadAll<AudioClip>(path);
        }

        private AudioClip LoadBreathingClip(SurfaceType surface)
        {
            string path = $"Audio/Breathing/{surface.ToString()}";
            return Resources.Load<AudioClip>(path);
        }

        private AudioClip LoadLandingClip(SurfaceType surface)
        {
            string path = $"Audio/Landing/{surface.ToString()}";
            return Resources.Load<AudioClip>(path);
        }

        private void EnsureAudioSources()
        {
            if (footstepSource == null)
            {
                footstepSource = gameObject.AddComponent<AudioSource>();
                footstepSource.spatialBlend = 1.0f;
            }

            if (breathingSource == null)
            {
                breathingSource = gameObject.AddComponent<AudioSource>();
                breathingSource.spatialBlend = 0.0f;
            }

            if (landingSource == null)
            {
                landingSource = gameObject.AddComponent<AudioSource>();
                landingSource.spatialBlend = 1.0f;
            }
        }

        /// <summary>
        /// Detects the surface type below the player.
        /// </summary>
        public SurfaceType DetectSurface(Vector3 position)
        {
            Ray ray = new Ray(position + Vector3.up * 0.1f, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, surfaceCheckDistance, surfaceLayer))
            {
                return GetSurfaceTypeFromTag(hit.collider.tag);
            }

            return SurfaceType.Concrete;
        }

        private SurfaceType GetSurfaceTypeFromTag(string tag)
        {
            if (System.Enum.TryParse(tag, out SurfaceType surface))
            {
                return surface;
            }

            return SurfaceType.Concrete;
        }

        /// <summary>
        /// Plays a footstep sound at the specified position based on current surface.
        /// </summary>
        public void PlayFootstep(Vector3 position, bool isSprinting = false)
        {
            EnsureAudioSources();

            currentSurface = DetectSurface(position);

            if (footstepClips[currentSurface].Length == 0)
                return;

            AudioClip clip = footstepClips[currentSurface][Random.Range(0, footstepClips[currentSurface].Length)];

            float volume = footstepVolume;
            if (isSprinting)
                volume *= 1.3f;

            footstepSource.pitch = Random.Range(minPitch, maxPitch);
            footstepSource.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Plays breathing audio loop.
        /// </summary>
        public void PlayBreathing()
        {
            EnsureAudioSources();

            currentSurface = DetectSurface(transform.position);

            if (breathingClips[currentSurface] != null && breathingSource.clip != breathingClips[currentSurface])
            {
                breathingSource.clip = breathingClips[currentSurface];
                breathingSource.loop = true;
                breathingSource.volume = breathingVolume;
                breathingSource.Play();
            }
        }

        /// <summary>
        /// Stops breathing audio.
        /// </summary>
        public void StopBreathing()
        {
            EnsureAudioSources();
            breathingSource.Stop();
        }

        /// <summary>
        /// Plays landing impact sound.
        /// </summary>
        public void PlayLandingSound(Vector3 position)
        {
            EnsureAudioSources();

            currentSurface = DetectSurface(position);

            if (landingClips[currentSurface] != null)
            {
                landingSource.pitch = 1.0f;
                landingSource.PlayOneShot(landingClips[currentSurface], 0.7f);
            }
        }

        /// <summary>
        /// Updates current surface type.
        /// </summary>
        public void UpdateSurface(Vector3 position)
        {
            currentSurface = DetectSurface(position);
        }

        public SurfaceType GetCurrentSurface() => currentSurface;

        public void SetFootstepVolume(float volume)
        {
            footstepVolume = Mathf.Clamp01(volume);
        }

        public void SetBreathingVolume(float volume)
        {
            breathingVolume = Mathf.Clamp01(volume);
        }
    }
}
