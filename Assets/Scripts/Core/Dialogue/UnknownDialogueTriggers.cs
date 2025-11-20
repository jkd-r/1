using UnityEngine;
using ProtocolEMR.Core.Dialogue;

namespace ProtocolEMR.Core.Dialogue
{
    /// <summary>
    /// Helper class for triggering Unknown messages from various game systems.
    /// Provides convenient methods for common trigger scenarios.
    /// </summary>
    public static class UnknownDialogueTriggers
    {
        public static void TriggerGameStart()
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.TriggerMessage(MessageTrigger.GameStart);
            }
        }

        public static void TriggerPlayerHitNPC(GameObject npc, float damage)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.PlayerHitNPC, npc);
                evt.AddContextData("damage", damage);
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerPlayerTookDamage(GameObject attacker, float damage, float currentHealth, float maxHealth)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.PlayerTookDamage, attacker);
                evt.AddContextData("damage", damage);
                evt.AddContextData("currentHealth", currentHealth);
                evt.AddContextData("maxHealth", maxHealth);
                
                float healthPercent = currentHealth / maxHealth;
                if (healthPercent < 0.3f)
                {
                    UnknownDialogueManager.Instance.TriggerMessage(MessageTrigger.PlayerLowHealth, attacker);
                }
                else
                {
                    UnknownDialogueManager.Instance.TriggerMessage(evt);
                }
            }
        }

        public static void TriggerNPCDefeated(GameObject npc)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.TriggerMessage(MessageTrigger.NPCDefeated, npc);
            }
        }

        public static void TriggerDodgeSuccessful()
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.TriggerMessage(MessageTrigger.DodgeSuccessful);
            }
        }

        public static void TriggerCombatStarted(GameObject enemy)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.TriggerMessage(MessageTrigger.CombatStarted, enemy);
            }
        }

        public static void TriggerPuzzleEncountered(GameObject puzzle)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.TriggerMessage(MessageTrigger.PuzzleEncountered, puzzle);
            }
        }

        public static void TriggerPuzzleAttemptFailed(GameObject puzzle)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.TriggerMessage(MessageTrigger.PuzzleAttemptFailed, puzzle);
            }
        }

        public static void TriggerPuzzleSolved(GameObject puzzle, bool perfect = false)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                MessageTrigger trigger = perfect ? MessageTrigger.PuzzleSolvedPerfectly : MessageTrigger.PuzzleSolved;
                UnknownDialogueManager.Instance.TriggerMessage(trigger, puzzle);
            }
        }

        public static void TriggerPlayerStuck()
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.TriggerMessage(MessageTrigger.PlayerStuck);
            }
        }

        public static void TriggerNewAreaDiscovered(string areaName)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.NewAreaDiscovered);
                evt.AddContextData("areaName", areaName);
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerSecretFound(GameObject secret)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.TriggerMessage(MessageTrigger.SecretFound, secret);
            }
        }

        public static void TriggerNPCEncountered(GameObject npc)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.TriggerMessage(MessageTrigger.NPCEncountered, npc);
            }
        }

        public static void TriggerItemFound(GameObject item)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.TriggerMessage(MessageTrigger.ItemFound, item);
            }
        }

        public static void TriggerDangerDetected(GameObject danger)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.TriggerMessage(MessageTrigger.DangerDetected, danger);
            }
        }

        public static void TriggerMissionStart(string missionName)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.MissionStart);
                evt.AddContextData("missionName", missionName);
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerMissionMilestone(string milestoneName)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.MissionMilestone);
                evt.AddContextData("milestoneName", milestoneName);
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerMissionComplete(string missionName)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.MissionComplete);
                evt.AddContextData("missionName", missionName);
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerObjectiveFailed(string objectiveName)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.ObjectiveFailed);
                evt.AddContextData("objectiveName", objectiveName);
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerNewMissionAvailable(string missionName)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.NewMissionAvailable);
                evt.AddContextData("missionName", missionName);
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerPlotPointReached(string plotPoint)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.PlotPointReached);
                evt.AddContextData("plotPoint", plotPoint);
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerProceduralStoryMilestone(string milestone)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.ProceduralStoryMilestone);
                evt.AddContextData("milestone", milestone);
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerMajorEventOccurred(string eventName)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.MajorEventOccurred);
                evt.AddContextData("eventName", eventName);
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerSecretDiscovered(string secretName)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.SecretDiscovered);
                evt.AddContextData("secretName", secretName);
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerStealthApproach()
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.TriggerMessage(MessageTrigger.StealthApproach);
            }
        }

        public static void TriggerAggressiveApproach()
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.TriggerMessage(MessageTrigger.AggressiveApproach);
            }
        }

        public static void TriggerDocumentRead(string documentName)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.DocumentRead);
                evt.AddContextData("documentName", documentName);
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerPlayerDeath()
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.TriggerMessage(MessageTrigger.PlayerDeath);
            }
        }

        public static void TriggerCustomMessage(string context)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.Custom);
                evt.AddContextData("context", context);
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }
    }
}
