using UnityEngine;
using System.Collections.Generic;

namespace ProtocolEMR.Combat.Core
{
    [System.Serializable]
    public class CombatHint
    {
        public string hintText;
        public float priority;
        public bool hasBeenShown;
    }

    public enum CombatContext
    {
        EffectivePunch,
        IneffectivePunch,
        WeaponSuggestion,
        LowHealth,
        EnemyLowHealth,
        MultipleEnemies,
        LowStamina,
        OutOfAmmo,
        DodgeRecommendation,
        CriticalHit
    }

    public class UnknownAIIntegration : MonoBehaviour
    {
        [Header("Hint Settings")]
        [SerializeField] private bool hintsEnabled = true;
        [SerializeField] private float hintFrequency = 0.5f;
        [SerializeField] private float minTimeBetweenHints = 10f;
        [SerializeField] private int maxHintsPerCombat = 3;

        [Header("Accessibility Settings")]
        [SerializeField] private bool accessibilityMode = false;
        [SerializeField] private float accessibilityHintFrequencyMultiplier = 2f;

        [Header("Hint Display")]
        [SerializeField] private UnityEngine.UI.Text hintDisplayText;
        [SerializeField] private float hintDisplayDuration = 5f;

        private Dictionary<CombatContext, List<CombatHint>> hintLibrary;
        private float lastHintTime = -999f;
        private int hintsShownThisCombat = 0;
        private bool inCombat = false;

        void Awake()
        {
            InitializeHintLibrary();
        }

        private void InitializeHintLibrary()
        {
            hintLibrary = new Dictionary<CombatContext, List<CombatHint>>();

            hintLibrary[CombatContext.EffectivePunch] = new List<CombatHint>
            {
                new CombatHint { hintText = "Your punch was effective.", priority = 0.3f },
                new CombatHint { hintText = "Good hit. Keep the pressure on.", priority = 0.3f },
                new CombatHint { hintText = "That connected well.", priority = 0.2f }
            };

            hintLibrary[CombatContext.WeaponSuggestion] = new List<CombatHint>
            {
                new CombatHint { hintText = "Consider a heavier weapon here.", priority = 0.7f },
                new CombatHint { hintText = "A melee weapon would be more effective.", priority = 0.7f },
                new CombatHint { hintText = "Try using that wrench you picked up.", priority = 0.8f }
            };

            hintLibrary[CombatContext.LowHealth] = new List<CombatHint>
            {
                new CombatHint { hintText = "Your health is critical. Consider retreating.", priority = 1.0f },
                new CombatHint { hintText = "You're taking too much damage. Fall back.", priority = 1.0f },
                new CombatHint { hintText = "Find cover and recover.", priority = 0.9f }
            };

            hintLibrary[CombatContext.EnemyLowHealth] = new List<CombatHint>
            {
                new CombatHint { hintText = "The enemy is weakening. Finish this.", priority = 0.5f },
                new CombatHint { hintText = "One more hit should do it.", priority = 0.4f },
                new CombatHint { hintText = "They're almost down.", priority = 0.3f }
            };

            hintLibrary[CombatContext.MultipleEnemies] = new List<CombatHint>
            {
                new CombatHint { hintText = "Multiple hostiles. Stay aware of your surroundings.", priority = 0.8f },
                new CombatHint { hintText = "You're outnumbered. Use the environment.", priority = 0.8f },
                new CombatHint { hintText = "Focus on one enemy at a time.", priority = 0.7f }
            };

            hintLibrary[CombatContext.LowStamina] = new List<CombatHint>
            {
                new CombatHint { hintText = "Your stamina is depleted. Space your attacks.", priority = 0.6f },
                new CombatHint { hintText = "You need to recover your stamina.", priority = 0.6f },
                new CombatHint { hintText = "Wait for your stamina to regenerate.", priority = 0.5f }
            };

            hintLibrary[CombatContext.OutOfAmmo] = new List<CombatHint>
            {
                new CombatHint { hintText = "You're out of ammunition. Switch to melee.", priority = 0.9f },
                new CombatHint { hintText = "No ammo left. Use your fists.", priority = 0.8f },
                new CombatHint { hintText = "Reload or switch weapons.", priority = 0.7f }
            };

            hintLibrary[CombatContext.DodgeRecommendation] = new List<CombatHint>
            {
                new CombatHint { hintText = "Dodge to avoid incoming attacks.", priority = 0.7f },
                new CombatHint { hintText = "Use dodge (Q) to evade.", priority = 0.7f },
                new CombatHint { hintText = "Movement is key to survival.", priority = 0.6f }
            };

            hintLibrary[CombatContext.CriticalHit] = new List<CombatHint>
            {
                new CombatHint { hintText = "Critical hit! Well done.", priority = 0.4f },
                new CombatHint { hintText = "Excellent strike.", priority = 0.3f },
                new CombatHint { hintText = "That was a devastating blow.", priority = 0.3f }
            };
        }

        public void OnCombatStart()
        {
            inCombat = true;
            hintsShownThisCombat = 0;
        }

        public void OnCombatEnd()
        {
            inCombat = false;
            hintsShownThisCombat = 0;
        }

        public void TriggerHint(CombatContext context)
        {
            if (!hintsEnabled)
                return;

            if (!inCombat)
                return;

            if (hintsShownThisCombat >= maxHintsPerCombat)
                return;

            float effectiveFrequency = hintFrequency;
            if (accessibilityMode)
                effectiveFrequency *= accessibilityHintFrequencyMultiplier;

            if (Random.value > effectiveFrequency)
                return;

            if (Time.time - lastHintTime < minTimeBetweenHints)
                return;

            if (!hintLibrary.ContainsKey(context))
                return;

            List<CombatHint> hints = hintLibrary[context];
            CombatHint selectedHint = SelectHint(hints);

            if (selectedHint != null)
            {
                DisplayHint(selectedHint.hintText);
                selectedHint.hasBeenShown = true;
                lastHintTime = Time.time;
                hintsShownThisCombat++;
            }
        }

        private CombatHint SelectHint(List<CombatHint> hints)
        {
            List<CombatHint> availableHints = hints.FindAll(h => !h.hasBeenShown);
            if (availableHints.Count == 0)
                return null;

            float totalPriority = 0f;
            foreach (CombatHint hint in availableHints)
                totalPriority += hint.priority;

            float randomValue = Random.value * totalPriority;
            float currentSum = 0f;

            foreach (CombatHint hint in availableHints)
            {
                currentSum += hint.priority;
                if (randomValue <= currentSum)
                    return hint;
            }

            return availableHints[0];
        }

        private void DisplayHint(string hintText)
        {
            if (hintDisplayText != null)
            {
                hintDisplayText.text = hintText;
                hintDisplayText.enabled = true;
                Invoke(nameof(HideHint), hintDisplayDuration);
            }

            Debug.Log($"[Unknown AI] {hintText}");
        }

        private void HideHint()
        {
            if (hintDisplayText != null)
                hintDisplayText.enabled = false;
        }

        public void SetHintsEnabled(bool enabled)
        {
            hintsEnabled = enabled;
        }

        public void SetAccessibilityMode(bool enabled)
        {
            accessibilityMode = enabled;
        }

        public void SetHintFrequency(float frequency)
        {
            hintFrequency = Mathf.Clamp01(frequency);
        }

        public void ResetHints()
        {
            foreach (var contextHints in hintLibrary.Values)
            {
                foreach (var hint in contextHints)
                {
                    hint.hasBeenShown = false;
                }
            }
        }
    }
}
