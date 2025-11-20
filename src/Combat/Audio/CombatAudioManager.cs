using UnityEngine;
using System.Collections.Generic;

namespace ProtocolEMR.Combat.Core
{
    [System.Serializable]
    public class CombatAudioClip
    {
        public string name;
        public AudioClip[] clips;
        public float volumeMin = 0.9f;
        public float volumeMax = 1.0f;
        public float pitchMin = 0.9f;
        public float pitchMax = 1.1f;
    }

    public class CombatAudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource combatAudioSource;
        [SerializeField] private AudioSource impactAudioSource;
        [SerializeField] private AudioSource weaponAudioSource;

        [Header("Punch Sounds")]
        [SerializeField] private CombatAudioClip punchSounds;
        [SerializeField] private CombatAudioClip kickSounds;

        [Header("Weapon Sounds")]
        [SerializeField] private CombatAudioClip wrenchSwingSounds;
        [SerializeField] private CombatAudioClip crowbarSwingSounds;
        [SerializeField] private CombatAudioClip pipeSwingSounds;

        [Header("Impact Sounds")]
        [SerializeField] private CombatAudioClip hitImpactSounds;
        [SerializeField] private CombatAudioClip weaponImpactSounds;

        [Header("Ranged Weapon Sounds")]
        [SerializeField] private CombatAudioClip taserSounds;
        [SerializeField] private CombatAudioClip plasmaSounds;
        [SerializeField] private CombatAudioClip reloadSounds;
        [SerializeField] private CombatAudioClip emptyClickSounds;

        [Header("Movement Sounds")]
        [SerializeField] private CombatAudioClip dodgeSounds;
        [SerializeField] private CombatAudioClip breathSounds;

        [Header("Player Sounds")]
        [SerializeField] private CombatAudioClip playerPainSounds;
        [SerializeField] private CombatAudioClip playerExertionSounds;

        [Header("UI Sounds")]
        [SerializeField] private CombatAudioClip weaponSwitchSounds;
        [SerializeField] private CombatAudioClip pickupSounds;
        [SerializeField] private CombatAudioClip weaponBreakSounds;

        [Header("3D Audio Settings")]
        [SerializeField] private float spatialBlend = 1.0f;
        [SerializeField] private float maxDistance = 50f;
        [SerializeField] private AnimationCurve rolloffCurve;

        private Dictionary<string, float> lastPlayedTimes = new Dictionary<string, float>();
        private float minTimeBetweenSounds = 0.1f;

        void Awake()
        {
            InitializeAudioSources();
            InitializeAudioClips();
        }

        private void InitializeAudioSources()
        {
            if (combatAudioSource == null)
            {
                GameObject audioObj = new GameObject("CombatAudioSource");
                audioObj.transform.SetParent(transform);
                combatAudioSource = audioObj.AddComponent<AudioSource>();
            }

            if (impactAudioSource == null)
            {
                GameObject audioObj = new GameObject("ImpactAudioSource");
                audioObj.transform.SetParent(transform);
                impactAudioSource = audioObj.AddComponent<AudioSource>();
            }

            if (weaponAudioSource == null)
            {
                GameObject audioObj = new GameObject("WeaponAudioSource");
                audioObj.transform.SetParent(transform);
                weaponAudioSource = audioObj.AddComponent<AudioSource>();
            }

            ConfigureAudioSource(combatAudioSource);
            ConfigureAudioSource(impactAudioSource);
            ConfigureAudioSource(weaponAudioSource);
        }

        private void ConfigureAudioSource(AudioSource source)
        {
            source.spatialBlend = 0f;
            source.maxDistance = maxDistance;
            source.rolloffMode = AudioRolloffMode.Custom;
            if (rolloffCurve != null && rolloffCurve.keys.Length > 0)
                source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, rolloffCurve);
        }

        private void InitializeAudioClips()
        {
            punchSounds = CreateDefaultAudioClip("Punch");
            kickSounds = CreateDefaultAudioClip("Kick");
            wrenchSwingSounds = CreateDefaultAudioClip("WrenchSwing");
            crowbarSwingSounds = CreateDefaultAudioClip("CrowbarSwing");
            pipeSwingSounds = CreateDefaultAudioClip("PipeSwing");
            hitImpactSounds = CreateDefaultAudioClip("HitImpact");
            weaponImpactSounds = CreateDefaultAudioClip("WeaponImpact");
            taserSounds = CreateDefaultAudioClip("Taser");
            plasmaSounds = CreateDefaultAudioClip("Plasma");
            reloadSounds = CreateDefaultAudioClip("Reload");
            emptyClickSounds = CreateDefaultAudioClip("EmptyClick");
            dodgeSounds = CreateDefaultAudioClip("Dodge");
            breathSounds = CreateDefaultAudioClip("Breath");
            playerPainSounds = CreateDefaultAudioClip("PlayerPain");
            playerExertionSounds = CreateDefaultAudioClip("PlayerExertion");
            weaponSwitchSounds = CreateDefaultAudioClip("WeaponSwitch");
            pickupSounds = CreateDefaultAudioClip("Pickup");
            weaponBreakSounds = CreateDefaultAudioClip("WeaponBreak");
        }

        private CombatAudioClip CreateDefaultAudioClip(string name)
        {
            return new CombatAudioClip
            {
                name = name,
                clips = new AudioClip[0],
                volumeMin = 0.9f,
                volumeMax = 1.0f,
                pitchMin = 0.9f,
                pitchMax = 1.1f
            };
        }

        public void PlayAttackSound(AttackType attackType)
        {
            CombatAudioClip clipData = null;

            switch (attackType)
            {
                case AttackType.Punch:
                    clipData = punchSounds;
                    break;
                case AttackType.Kick:
                    clipData = kickSounds;
                    break;
                case AttackType.Wrench:
                    clipData = wrenchSwingSounds;
                    break;
                case AttackType.Crowbar:
                    clipData = crowbarSwingSounds;
                    break;
                case AttackType.Pipe:
                    clipData = pipeSwingSounds;
                    break;
            }

            if (clipData != null)
                PlayRandomClip(combatAudioSource, clipData);
        }

        public void PlayHitSound(Vector3 position)
        {
            PlayRandomClipAtPoint(hitImpactSounds, position);
        }

        public void PlayTaserSound()
        {
            PlayRandomClip(weaponAudioSource, taserSounds);
        }

        public void PlayPlasmaSound()
        {
            PlayRandomClip(weaponAudioSource, plasmaSounds);
        }

        public void PlayReloadSound()
        {
            PlayRandomClip(weaponAudioSource, reloadSounds);
        }

        public void PlayEmptyClickSound()
        {
            PlayRandomClip(weaponAudioSource, emptyClickSounds);
        }

        public void PlayDodgeSound()
        {
            PlayRandomClip(combatAudioSource, dodgeSounds);
        }

        public void PlayPlayerPainSound()
        {
            PlayRandomClip(combatAudioSource, playerPainSounds);
        }

        public void PlayPlayerExertionSound()
        {
            PlayRandomClip(combatAudioSource, playerExertionSounds);
        }

        public void PlayWeaponSwitchSound()
        {
            PlayRandomClip(weaponAudioSource, weaponSwitchSounds);
        }

        public void PlayPickupSound()
        {
            PlayRandomClip(weaponAudioSource, pickupSounds);
        }

        public void PlayWeaponBreakSound()
        {
            PlayRandomClip(weaponAudioSource, weaponBreakSounds);
        }

        public void PlayBreathingSound()
        {
            if (!combatAudioSource.isPlaying)
                PlayRandomClip(combatAudioSource, breathSounds);
        }

        private void PlayRandomClip(AudioSource source, CombatAudioClip clipData)
        {
            if (clipData == null || clipData.clips == null || clipData.clips.Length == 0)
                return;

            if (!CanPlaySound(clipData.name))
                return;

            AudioClip clip = clipData.clips[Random.Range(0, clipData.clips.Length)];
            float volume = Random.Range(clipData.volumeMin, clipData.volumeMax);
            float pitch = Random.Range(clipData.pitchMin, clipData.pitchMax);

            source.pitch = pitch;
            source.PlayOneShot(clip, volume);

            lastPlayedTimes[clipData.name] = Time.time;
        }

        private void PlayRandomClipAtPoint(CombatAudioClip clipData, Vector3 position)
        {
            if (clipData == null || clipData.clips == null || clipData.clips.Length == 0)
                return;

            if (!CanPlaySound(clipData.name))
                return;

            AudioClip clip = clipData.clips[Random.Range(0, clipData.clips.Length)];
            float volume = Random.Range(clipData.volumeMin, clipData.volumeMax);

            AudioSource.PlayClipAtPoint(clip, position, volume);

            lastPlayedTimes[clipData.name] = Time.time;
        }

        private bool CanPlaySound(string soundName)
        {
            if (!lastPlayedTimes.ContainsKey(soundName))
                return true;

            return Time.time - lastPlayedTimes[soundName] >= minTimeBetweenSounds;
        }

        public void SetCombatMusicVolume(float volume)
        {
            combatAudioSource.volume = volume;
        }

        public void EnableEnvironmentalEcho(bool enabled)
        {
        }
    }
}
