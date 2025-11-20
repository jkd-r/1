using UnityEngine;

namespace ProtocolEMR.Core.AI
{
    /// <summary>
    /// Example NPC prefab configurations for different NPC types.
    /// This component can be added to GameObjects to configure them as specific NPC types.
    /// </summary>
    public class NPCPrefabConfiguration : MonoBehaviour
    {
        [Header("NPC Type Configuration")]
        [SerializeField] private NPCType npcType = NPCType.Guard;
        [SerializeField] private NPCParameters baseParameters = new NPCParameters();
        
        [Header("Visual Configuration")]
        [SerializeField] private Material npcMaterial;
        [SerializeField] private GameObject[] visualVariations;
        [SerializeField] private bool randomizeVisual = true;
        
        [Header("Audio Configuration")]
        [SerializeField] private AudioClip[] footstepSounds;
        [SerializeField] private AudioClip[] attackSounds;
        [SerializeField] private AudioClip[] hurtSounds;
        [SerializeField] private AudioClip[] deathSounds;
        
        [Header("Weapon Configuration")]
        [SerializeField] private GameObject weaponPrefab;
        [SerializeField] private Transform weaponAttachPoint;
        [SerializeField] private bool equipWeapon = true;

        private NPCController npcController;

        private void Awake()
        {
            npcController = GetComponent<NPCController>();
            ConfigureNPC();
        }

        /// <summary>
        /// Configures NPC based on type and settings.
        /// </summary>
        private void ConfigureNPC()
        {
            if (npcController == null)
            {
                npcController = gameObject.AddComponent<NPCController>();
            }

            // Apply type-specific configuration
            ApplyTypeConfiguration();
            
            // Apply visual configuration
            ApplyVisualConfiguration();
            
            // Apply weapon configuration
            ApplyWeaponConfiguration();
            
            // Apply audio configuration (placeholder for future audio system)
            // ApplyAudioConfiguration();
        }

        /// <summary>
        /// Applies type-specific parameters and behaviors.
        /// </summary>
        private void ApplyTypeConfiguration()
        {
            switch (npcType)
            {
                case NPCType.Guard:
                    ConfigureGuard();
                    break;
                case NPCType.Scientist:
                    ConfigureScientist();
                    break;
                case NPCType.Civilian:
                    ConfigureCivilian();
                    break;
                case NPCType.Beast:
                    ConfigureBeast();
                    break;
            }
        }

        /// <summary>
        /// Configures Guard NPC.
        /// </summary>
        private void ConfigureGuard()
        {
            baseParameters.maxHealth = Random.Range(60f, 80f);
            baseParameters.walkSpeed = Random.Range(3.5f, 4.5f);
            baseParameters.runSpeed = Random.Range(7f, 9f);
            baseParameters.sprintSpeed = Random.Range(11f, 13f);
            baseParameters.damagePerHit = Random.Range(15f, 20f);
            baseParameters.perceptionRange = Random.Range(15f, 20f);
            baseParameters.aggression = Random.Range(70f, 90f);
            baseParameters.intelligence = Random.Range(50f, 70f);
            baseParameters.morale = Random.Range(70f, 85f);
            baseParameters.attackFrequency = Random.Range(0.8f, 1.2f);
            baseParameters.fieldOfView = 120f;
            baseParameters.hearingRange = 12f;
            baseParameters.reactionTime = Random.Range(0.3f, 0.6f);
            
            // Set NPC type on controller
            npcController.GetType().GetField("npcType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(npcController, npcType);
        }

        /// <summary>
        /// Configures Scientist NPC.
        /// </summary>
        private void ConfigureScientist()
        {
            baseParameters.maxHealth = Random.Range(30f, 50f);
            baseParameters.walkSpeed = Random.Range(3f, 4f);
            baseParameters.runSpeed = Random.Range(6f, 8f);
            baseParameters.sprintSpeed = Random.Range(9f, 11f);
            baseParameters.damagePerHit = Random.Range(5f, 10f);
            baseParameters.perceptionRange = Random.Range(10f, 15f);
            baseParameters.aggression = Random.Range(10f, 30f);
            baseParameters.intelligence = Random.Range(80f, 100f);
            baseParameters.morale = Random.Range(30f, 50f);
            baseParameters.attackFrequency = Random.Range(1.2f, 1.8f);
            baseParameters.fieldOfView = 100f;
            baseParameters.hearingRange = 8f;
            baseParameters.reactionTime = Random.Range(0.5f, 0.8f);
        }

        /// <summary>
        /// Configures Civilian NPC.
        /// </summary>
        private void ConfigureCivilian()
        {
            baseParameters.maxHealth = Random.Range(40f, 60f);
            baseParameters.walkSpeed = Random.Range(3f, 4f);
            baseParameters.runSpeed = Random.Range(6f, 8f);
            baseParameters.sprintSpeed = Random.Range(10f, 12f);
            baseParameters.damagePerHit = Random.Range(8f, 12f);
            baseParameters.perceptionRange = Random.Range(12f, 18f);
            baseParameters.aggression = Random.Range(20f, 40f);
            baseParameters.intelligence = Random.Range(40f, 60f);
            baseParameters.morale = Random.Range(40f, 60f);
            baseParameters.attackFrequency = Random.Range(1.0f, 1.5f);
            baseParameters.fieldOfView = 110f;
            baseParameters.hearingRange = 10f;
            baseParameters.reactionTime = Random.Range(0.4f, 0.7f);
        }

        /// <summary>
        /// Configures Beast NPC.
        /// </summary>
        private void ConfigureBeast()
        {
            baseParameters.maxHealth = Random.Range(70f, 100f);
            baseParameters.walkSpeed = Random.Range(4f, 5f);
            baseParameters.runSpeed = Random.Range(9f, 11f);
            baseParameters.sprintSpeed = Random.Range(13f, 15f);
            baseParameters.damagePerHit = Random.Range(20f, 30f);
            baseParameters.perceptionRange = Random.Range(18f, 25f);
            baseParameters.aggression = Random.Range(80f, 100f);
            baseParameters.intelligence = Random.Range(20f, 40f);
            baseParameters.morale = Random.Range(85f, 100f);
            baseParameters.attackFrequency = Random.Range(0.6f, 1.0f);
            baseParameters.fieldOfView = 140f;
            baseParameters.hearingRange = 15f;
            baseParameters.reactionTime = Random.Range(0.2f, 0.5f);
        }

        /// <summary>
        /// Applies visual configuration.
        /// </summary>
        private void ApplyVisualConfiguration()
        {
            // Apply material
            if (npcMaterial != null)
            {
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    renderer.material = npcMaterial;
                }
            }
            
            // Apply visual variations
            if (randomizeVisual && visualVariations.Length > 0)
            {
                GameObject randomVisual = visualVariations[Random.Range(0, visualVariations.Length)];
                if (randomVisual != null)
                {
                    // Disable all variations
                    foreach (var variation in visualVariations)
                    {
                        if (variation != null)
                        {
                            variation.SetActive(false);
                        }
                    }
                    
                    // Enable selected variation
                    randomVisual.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Applies weapon configuration.
        /// </summary>
        private void ApplyWeaponConfiguration()
        {
            if (!equipWeapon || weaponPrefab == null || weaponAttachPoint == null)
                return;

            GameObject weapon = Instantiate(weaponPrefab, weaponAttachPoint);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Creates NPC prefab of specified type.
        /// </summary>
        public static GameObject CreateNPCPrefab(NPCType type, string name = null)
        {
            string prefabName = name ?? $"{type}_NPC";
            GameObject npcObject = new GameObject(prefabName);
            
            // Add required components
            npcObject.AddComponent<NPCPrefabConfiguration>();
            
            // Configure based on type
            NPCPrefabConfiguration config = npcObject.GetComponent<NPCPrefabConfiguration>();
            config.GetType().GetField("npcType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(config, type);
            
            // Add basic visual representation
            CreateBasicVisuals(npcObject, type);
            
            return npcObject;
        }

        /// <summary>
        /// Creates basic visual representation for NPC.
        /// </summary>
        private static void CreateBasicVisuals(GameObject npcObject, NPCType type)
        {
            // Create capsule as basic representation
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "Body";
            capsule.transform.SetParent(npcObject.transform);
            capsule.transform.localPosition = Vector3.up * 0.5f;
            capsule.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            
            // Remove collider from visual
            Collider collider = capsule.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }
            
            // Add color based on type
            Renderer renderer = capsule.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = new Material(Shader.Find("Standard"));
                
                switch (type)
                {
                    case NPCType.Guard:
                        material.color = Color.red;
                        break;
                    case NPCType.Scientist:
                        material.color = Color.blue;
                        break;
                    case NPCType.Civilian:
                        material.color = Color.green;
                        break;
                    case NPCType.Beast:
                        material.color = Color.magenta;
                        break;
                    default:
                        material.color = Color.gray;
                        break;
                }
                
                renderer.material = material;
            }
            
            // Add weapon attach point
            GameObject weaponPoint = new GameObject("WeaponAttachPoint");
            weaponPoint.transform.SetParent(npcObject.transform);
            weaponPoint.transform.localPosition = new Vector3(0.3f, 0.8f, 0.2f);
        }

        /// <summary>
        /// Gets NPC type from configuration.
        /// </summary>
        public NPCType GetNPCType()
        {
            return npcType;
        }

        /// <summary>
        /// Gets base parameters.
        /// </summary>
        public NPCParameters GetBaseParameters()
        {
            return baseParameters;
        }

        private void OnValidate()
        {
            // Ensure parameters are valid
            baseParameters.maxHealth = Mathf.Max(1f, baseParameters.maxHealth);
            baseParameters.walkSpeed = Mathf.Max(0.1f, baseParameters.walkSpeed);
            baseParameters.runSpeed = Mathf.Max(baseParameters.walkSpeed, baseParameters.runSpeed);
            baseParameters.sprintSpeed = Mathf.Max(baseParameters.runSpeed, baseParameters.sprintSpeed);
            baseParameters.perceptionRange = Mathf.Max(1f, baseParameters.perceptionRange);
            baseParameters.fieldOfView = Mathf.Clamp(baseParameters.fieldOfView, 1f, 360f);
        }
    }
}