using UnityEngine;
using System.Collections.Generic;
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

        // Dynamic Event Triggers
        public static void TriggerDynamicEventStarted(string eventId, string eventType, int chunkId, float threatLevel, Dictionary<string, object> additionalContext = null)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                MessageTrigger trigger = MessageTrigger.DynamicEventStarted;
                
                // Select appropriate trigger based on event type
                if (eventType.Contains("combat", System.StringComparison.OrdinalIgnoreCase))
                    trigger = MessageTrigger.DynamicEventCombat;
                else if (eventType.Contains("puzzle", System.StringComparison.OrdinalIgnoreCase))
                    trigger = MessageTrigger.DynamicEventPuzzle;
                else if (eventType.Contains("ambient", System.StringComparison.OrdinalIgnoreCase))
                    trigger = MessageTrigger.DynamicEventAmbient;
                
                UnknownMessageEvent evt = new UnknownMessageEvent(trigger);
                evt.AddContextData("eventId", eventId);
                evt.AddContextData("eventType", eventType);
                evt.AddContextData("chunkId", chunkId);
                evt.AddContextData("threatLevel", threatLevel);
                evt.AddContextData("isStart", true);
                
                if (additionalContext != null)
                {
                    foreach (var kvp in additionalContext)
                    {
                        evt.AddContextData(kvp.Key, kvp.Value);
                    }
                }
                
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerDynamicEventResolved(string eventId, bool success, float duration, Dictionary<string, object> additionalContext = null)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                MessageTrigger trigger = success ? MessageTrigger.DynamicEventResolved : MessageTrigger.DynamicEventFailed;
                UnknownMessageEvent evt = new UnknownMessageEvent(trigger);
                evt.AddContextData("eventId", eventId);
                evt.AddContextData("success", success);
                evt.AddContextData("duration", duration);
                evt.AddContextData("isStart", false);
                
                if (additionalContext != null)
                {
                    foreach (var kvp in additionalContext)
                    {
                        evt.AddContextData(kvp.Key, kvp.Value);
                    }
                }
                
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerDynamicEventMilestone(string eventId, string milestoneName, int progress, Dictionary<string, object> additionalContext = null)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.DynamicEventMilestone);
                evt.AddContextData("eventId", eventId);
                evt.AddContextData("milestoneName", milestoneName);
                evt.AddContextData("progress", progress);
                
                if (additionalContext != null)
                {
                    foreach (var kvp in additionalContext)
                    {
                        evt.AddContextData(kvp.Key, kvp.Value);
                    }
                }
                
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerDynamicEventBatch(List<UnknownMessageEvent> events)
        {
            if (UnknownDialogueManager.Instance != null && events != null && events.Count > 0)
            {
                // Trigger the highest priority event from the batch
                // Priority: Combat > Puzzle > Ambient
                UnknownMessageEvent selectedEvent = events[0];
                int highestPriority = -1;
                
                foreach (var evt in events)
                {
                    int priority = GetEventPriority(evt.trigger);
                    if (priority > highestPriority)
                    {
                        highestPriority = priority;
                        selectedEvent = evt;
                    }
                }
                
                UnknownDialogueManager.Instance.TriggerMessage(selectedEvent);
            }
        }

        private static int GetEventPriority(MessageTrigger trigger)
        {
            switch (trigger)
            {
                case MessageTrigger.DynamicEventCombat:
                    return 3;
                case MessageTrigger.DynamicEventPuzzle:
                    return 2;
                case MessageTrigger.DynamicEventAmbient:
                    return 1;
                default:
                    return 0;
            }
        }

        // Additional convenience methods for playtest flow
        public static void TriggerWelcome()
        {
            TriggerGameStart();
        }

        public static void TriggerTutorial(int tutorialStep)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.Custom);
                evt.AddContextData("tutorialStep", tutorialStep);
                evt.AddContextData("context", "tutorial");
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerPuzzleDiscovery(string puzzleName)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.PuzzleEncountered);
                evt.AddContextData("puzzleName", puzzleName);
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerThreatDetected(int threatLevel)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.DangerDetected);
                evt.AddContextData("threatLevel", threatLevel);
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerExtractionReady()
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownMessageEvent evt = new UnknownMessageEvent(MessageTrigger.Custom);
                evt.AddContextData("context", "extraction_ready");
                UnknownDialogueManager.Instance.TriggerMessage(evt);
            }
        }

        public static void TriggerMissionComplete()
        {
            TriggerMissionComplete("Main Mission");
        }

        public static void TriggerDeath()
        {
            TriggerPlayerDeath();
        }
    }
}
