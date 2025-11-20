using UnityEngine;
using ProtocolEMR.Core.Dialogue;
using ProtocolEMR.Core.AI;

namespace ProtocolEMR.Core.Dialogue
{
    /// <summary>
    /// Example integration of Unknown Dialogue System with NPC AI and game systems.
    /// Shows how to trigger contextual messages based on game events.
    /// </summary>
    public class UnknownDialogueIntegrationExample : MonoBehaviour
    {
        [Header("Integration Settings")]
        [SerializeField] private bool enableCombatMessages = true;
        [SerializeField] private bool enableExplorationMessages = true;
        [SerializeField] private bool enableNPCMessages = true;
        [SerializeField] private float dangerDetectionRadius = 20f;

        private bool hasGameStarted = false;

        private void Start()
        {
            if (!hasGameStarted)
            {
                UnknownDialogueTriggers.TriggerGameStart();
                hasGameStarted = true;
            }

            SubscribeToNPCEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromNPCEvents();
        }

        private void SubscribeToNPCEvents()
        {
            if (NPCManager.Instance != null && enableNPCMessages)
            {
                NPCManager.Instance.OnGlobalAlertTriggered += HandleGlobalAlert;
            }
        }

        private void UnsubscribeFromNPCEvents()
        {
            if (NPCManager.Instance != null)
            {
                NPCManager.Instance.OnGlobalAlertTriggered -= HandleGlobalAlert;
            }
        }

        private void HandleGlobalAlert(Vector3 position, GameObject source, float radius, AlertType alertType)
        {
            if (!enableNPCMessages)
                return;

            switch (alertType)
            {
                case AlertType.SoundDetected:
                    UnknownDialogueTriggers.TriggerDangerDetected(source);
                    break;

                case AlertType.PlayerSighted:
                    UnknownDialogueTriggers.TriggerNPCEncountered(source);
                    break;

                case AlertType.AttackDetected:
                    UnknownDialogueTriggers.TriggerCombatStarted(source);
                    break;
            }
        }

        public void OnPlayerHitNPC(NPCController npc, float damage)
        {
            if (!enableCombatMessages)
                return;

            UnknownDialogueTriggers.TriggerPlayerHitNPC(npc.gameObject, damage);

            if (npc.GetCurrentHealth() <= 0f)
            {
                UnknownDialogueTriggers.TriggerNPCDefeated(npc.gameObject);
            }
        }

        public void OnPlayerTakeDamage(GameObject attacker, float damage, float currentHealth, float maxHealth)
        {
            if (!enableCombatMessages)
                return;

            UnknownDialogueTriggers.TriggerPlayerTookDamage(attacker, damage, currentHealth, maxHealth);
        }

        public void OnPlayerDodgeSuccessful()
        {
            if (!enableCombatMessages)
                return;

            UnknownDialogueTriggers.TriggerDodgeSuccessful();
        }

        public void OnAreaDiscovered(string areaName)
        {
            if (!enableExplorationMessages)
                return;

            UnknownDialogueTriggers.TriggerNewAreaDiscovered(areaName);
        }

        public void OnSecretFound(GameObject secretObject)
        {
            if (!enableExplorationMessages)
                return;

            UnknownDialogueTriggers.TriggerSecretFound(secretObject);
        }

        public void OnItemPickedUp(GameObject item)
        {
            if (!enableExplorationMessages)
                return;

            UnknownDialogueTriggers.TriggerItemFound(item);
        }

        public void OnPuzzleEncountered(GameObject puzzle)
        {
            UnknownDialogueTriggers.TriggerPuzzleEncountered(puzzle);
        }

        public void OnPuzzleAttemptFailed(GameObject puzzle)
        {
            UnknownDialogueTriggers.TriggerPuzzleAttemptFailed(puzzle);
        }

        public void OnPuzzleSolved(GameObject puzzle, bool isPerfect = false)
        {
            UnknownDialogueTriggers.TriggerPuzzleSolved(puzzle, isPerfect);
        }

        public void OnMissionStarted(string missionName)
        {
            UnknownDialogueTriggers.TriggerMissionStart(missionName);
        }

        public void OnMissionMilestoneReached(string milestoneName)
        {
            UnknownDialogueTriggers.TriggerMissionMilestone(milestoneName);
        }

        public void OnMissionCompleted(string missionName)
        {
            UnknownDialogueTriggers.TriggerMissionComplete(missionName);
        }

        public void OnObjectiveFailed(string objectiveName)
        {
            UnknownDialogueTriggers.TriggerObjectiveFailed(objectiveName);
        }

        public void OnPlotPointReached(string plotPointName)
        {
            UnknownDialogueTriggers.TriggerPlotPointReached(plotPointName);
        }

        public void OnDocumentRead(string documentName)
        {
            UnknownDialogueTriggers.TriggerDocumentRead(documentName);
        }

        public void OnPlayerDeath()
        {
            UnknownDialogueTriggers.TriggerPlayerDeath();
        }

        public void OnStealthApproach()
        {
            UnknownDialogueTriggers.TriggerStealthApproach();
        }

        public void OnAggressiveApproach()
        {
            UnknownDialogueTriggers.TriggerAggressiveApproach();
        }

        private void DetectNearbyDanger()
        {
            if (!enableNPCMessages)
                return;

            Collider[] nearbyNPCs = Physics.OverlapSphere(transform.position, dangerDetectionRadius);
            
            foreach (Collider col in nearbyNPCs)
            {
                NPCController npc = col.GetComponent<NPCController>();
                if (npc != null && npc.GetCurrentState() == NPCState.Chasing)
                {
                    UnknownDialogueTriggers.TriggerDangerDetected(npc.gameObject);
                    break;
                }
            }
        }

        public void EnableMessageCategory(string category, bool enabled)
        {
            switch (category.ToLower())
            {
                case "combat":
                    enableCombatMessages = enabled;
                    break;
                case "exploration":
                    enableExplorationMessages = enabled;
                    break;
                case "npc":
                    enableNPCMessages = enabled;
                    break;
            }
        }
    }
}
