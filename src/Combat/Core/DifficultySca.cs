using UnityEngine;

namespace ProtocolEMR.Combat.Core
{
    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard,
        Nightmare
    }

    [System.Serializable]
    public class DifficultySettings
    {
        public DifficultyLevel level;
        public float playerDamageMultiplier;
        public float npcDamageMultiplier;
        public float ammoDropMultiplier;
        public float healthDropMultiplier;
        public float npcHealthMultiplier;
        public float npcAggressionMultiplier;
    }

    public class DifficultyScaling : MonoBehaviour
    {
        [Header("Current Difficulty")]
        [SerializeField] private DifficultyLevel currentDifficulty = DifficultyLevel.Normal;

        [Header("Difficulty Presets")]
        [SerializeField] private DifficultySettings easySettings = new DifficultySettings
        {
            level = DifficultyLevel.Easy,
            playerDamageMultiplier = 1.5f,
            npcDamageMultiplier = 0.5f,
            ammoDropMultiplier = 2.0f,
            healthDropMultiplier = 1.5f,
            npcHealthMultiplier = 0.75f,
            npcAggressionMultiplier = 0.7f
        };

        [SerializeField] private DifficultySettings normalSettings = new DifficultySettings
        {
            level = DifficultyLevel.Normal,
            playerDamageMultiplier = 1.0f,
            npcDamageMultiplier = 1.0f,
            ammoDropMultiplier = 1.0f,
            healthDropMultiplier = 1.0f,
            npcHealthMultiplier = 1.0f,
            npcAggressionMultiplier = 1.0f
        };

        [SerializeField] private DifficultySettings hardSettings = new DifficultySettings
        {
            level = DifficultyLevel.Hard,
            playerDamageMultiplier = 0.75f,
            npcDamageMultiplier = 1.25f,
            ammoDropMultiplier = 0.5f,
            healthDropMultiplier = 0.75f,
            npcHealthMultiplier = 1.25f,
            npcAggressionMultiplier = 1.3f
        };

        [SerializeField] private DifficultySettings nightmareSettings = new DifficultySettings
        {
            level = DifficultyLevel.Nightmare,
            playerDamageMultiplier = 0.5f,
            npcDamageMultiplier = 1.5f,
            ammoDropMultiplier = 0.0f,
            healthDropMultiplier = 0.5f,
            npcHealthMultiplier = 1.5f,
            npcAggressionMultiplier = 1.5f
        };

        private DifficultySettings activeSettings;

        private static DifficultyScaling instance;

        public static DifficultyScaling Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<DifficultyScaling>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("DifficultyScaling");
                        instance = go.AddComponent<DifficultyScaling>();
                    }
                }
                return instance;
            }
        }

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            SetDifficulty(currentDifficulty);
        }

        public void SetDifficulty(DifficultyLevel level)
        {
            currentDifficulty = level;

            switch (level)
            {
                case DifficultyLevel.Easy:
                    activeSettings = easySettings;
                    break;
                case DifficultyLevel.Normal:
                    activeSettings = normalSettings;
                    break;
                case DifficultyLevel.Hard:
                    activeSettings = hardSettings;
                    break;
                case DifficultyLevel.Nightmare:
                    activeSettings = nightmareSettings;
                    break;
            }

            ApplyDifficultyToScene();
        }

        private void ApplyDifficultyToScene()
        {
            NPCCombatController[] npcs = FindObjectsOfType<NPCCombatController>();
            foreach (NPCCombatController npc in npcs)
            {
                npc.SetAggressionLevel(activeSettings.npcAggressionMultiplier);
            }
        }

        public float ScalePlayerDamage(float baseDamage)
        {
            return baseDamage * activeSettings.playerDamageMultiplier;
        }

        public float ScaleNPCDamage(float baseDamage)
        {
            return baseDamage * activeSettings.npcDamageMultiplier;
        }

        public float ScaleNPCHealth(float baseHealth)
        {
            return baseHealth * activeSettings.npcHealthMultiplier;
        }

        public bool ShouldDropAmmo()
        {
            if (activeSettings.ammoDropMultiplier == 0f)
                return false;

            return Random.value <= activeSettings.ammoDropMultiplier;
        }

        public bool ShouldDropHealth()
        {
            return Random.value <= activeSettings.healthDropMultiplier;
        }

        public float GetAmmoDropMultiplier()
        {
            return activeSettings.ammoDropMultiplier;
        }

        public float GetNPCAggressionMultiplier()
        {
            return activeSettings.npcAggressionMultiplier;
        }

        public DifficultyLevel GetCurrentDifficulty()
        {
            return currentDifficulty;
        }

        public DifficultySettings GetActiveSettings()
        {
            return activeSettings;
        }
    }
}
