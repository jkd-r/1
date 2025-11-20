using UnityEngine;
using UnityEditor;
using ProtocolEMR.Core.Dialogue;
using System.Collections.Generic;

namespace ProtocolEMR.Core.Editor
{
    /// <summary>
    /// Editor utility to create and populate the Unknown Message Database.
    /// </summary>
    public class UnknownMessageDatabaseCreator : EditorWindow
    {
        [MenuItem("Protocol EMR/Dialogue/Create Message Database")]
        public static void CreateDatabase()
        {
            string path = "Assets/Resources/Dialogue/UnknownMessageDatabase.asset";
            
            System.IO.Directory.CreateDirectory("Assets/Resources/Dialogue");
            
            UnknownMessageDatabase database = ScriptableObject.CreateInstance<UnknownMessageDatabase>();
            
            database.messages = GenerateAllMessages();
            database.globalMessageCooldown = 5f;
            database.maxHistorySize = 100;
            
            AssetDatabase.CreateAsset(database, path);
            AssetDatabase.SaveAssets();
            
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = database;
            
            Debug.Log($"Unknown Message Database created with {database.messages.Count} messages at: {path}");
        }

        private static List<UnknownMessage> GenerateAllMessages()
        {
            List<UnknownMessage> messages = new List<UnknownMessage>();
            
            messages.AddRange(GenerateCombatMessages());
            messages.AddRange(GeneratePuzzleMessages());
            messages.AddRange(GenerateExplorationMessages());
            messages.AddRange(GenerateMissionMessages());
            messages.AddRange(GenerateNarrativeMessages());
            messages.AddRange(GenerateWarningMessages());
            messages.AddRange(GenerateEncouragementMessages());
            messages.AddRange(GenerateCommentaryMessages());
            messages.AddRange(GenerateDynamicEventMessages());
            
            return messages;
        }

        private static List<UnknownMessage> GenerateCombatMessages()
        {
            List<UnknownMessage> messages = new List<UnknownMessage>();
            
            messages.Add(CreateMessage("That worked.", MessageCategory.Combat, MessageTrigger.PlayerHitNPC, 0, 3, 0));
            messages.Add(CreateMessage("Effective.", MessageCategory.Combat, MessageTrigger.PlayerHitNPC, 0, 3, 0));
            messages.Add(CreateMessage("Direct approach.", MessageCategory.Combat, MessageTrigger.PlayerHitNPC, 0, 3, 0));
            messages.Add(CreateMessage("Try to predict their movements.", MessageCategory.Combat, MessageTrigger.PlayerHitNPC, 1, 3, 0));
            messages.Add(CreateMessage("Watch their patterns.", MessageCategory.Combat, MessageTrigger.PlayerHitNPC, 1, 3, 1));
            
            messages.Add(CreateMessage("You're hurt. Caution.", MessageCategory.Combat, MessageTrigger.PlayerTookDamage, 0, 3, 0));
            messages.Add(CreateMessage("That wasn't wise.", MessageCategory.Combat, MessageTrigger.PlayerTookDamage, 0, 3, 0));
            messages.Add(CreateMessage("You're not as resilient as you thought.", MessageCategory.Combat, MessageTrigger.PlayerTookDamage, 1, 3, 1));
            messages.Add(CreateMessage("Pain is instructive.", MessageCategory.Combat, MessageTrigger.PlayerTookDamage, 2, 3, 2));
            
            messages.Add(CreateMessage("Well, that's one less.", MessageCategory.Combat, MessageTrigger.NPCDefeated, 0, 3, 0));
            messages.Add(CreateMessage("Eliminated.", MessageCategory.Combat, MessageTrigger.NPCDefeated, 0, 3, 0));
            messages.Add(CreateMessage("They won't trouble you again.", MessageCategory.Combat, MessageTrigger.NPCDefeated, 1, 3, 1));
            messages.Add(CreateMessage("Necessary.", MessageCategory.Combat, MessageTrigger.NPCDefeated, 2, 3, 2));
            
            messages.Add(CreateMessage("Your condition is critical.", MessageCategory.Warning, MessageTrigger.PlayerLowHealth, 0, 3, 0));
            messages.Add(CreateMessage("Survival becomes unlikely.", MessageCategory.Warning, MessageTrigger.PlayerLowHealth, 1, 3, 0));
            messages.Add(CreateMessage("Find safety. Now.", MessageCategory.Warning, MessageTrigger.PlayerLowHealth, 0, 3, 0));
            
            messages.Add(CreateMessage("Excellent reflexes.", MessageCategory.Encouragement, MessageTrigger.DodgeSuccessful, 0, 3, 0));
            messages.Add(CreateMessage("Adaptive.", MessageCategory.Encouragement, MessageTrigger.DodgeSuccessful, 1, 3, 1));
            messages.Add(CreateMessage("Your instincts serve you well.", MessageCategory.Encouragement, MessageTrigger.DodgeSuccessful, 2, 3, 2));
            
            messages.Add(CreateMessage("They're getting serious.", MessageCategory.Warning, MessageTrigger.CombatStarted, 1, 3, 0));
            messages.Add(CreateMessage("Engagement imminent.", MessageCategory.Warning, MessageTrigger.CombatStarted, 0, 3, 0));
            messages.Add(CreateMessage("Combat situation developing.", MessageCategory.Warning, MessageTrigger.CombatStarted, 0, 3, 1));
            
            return messages;
        }

        private static List<UnknownMessage> GeneratePuzzleMessages()
        {
            List<UnknownMessage> messages = new List<UnknownMessage>();
            
            messages.Add(CreateMessage("The pieces don't fit yet.", MessageCategory.Puzzle, MessageTrigger.PuzzleAttemptFailed, 0, 3, 0));
            messages.Add(CreateMessage("Try again.", MessageCategory.Puzzle, MessageTrigger.PuzzleAttemptFailed, 0, 3, 0));
            messages.Add(CreateMessage("Not quite.", MessageCategory.Puzzle, MessageTrigger.PuzzleAttemptFailed, 0, 3, 0));
            messages.Add(CreateMessage("You might be overthinking this.", MessageCategory.Puzzle, MessageTrigger.PuzzleAttemptFailed, 1, 3, 1));
            
            messages.Add(CreateMessage("Clever.", MessageCategory.Encouragement, MessageTrigger.PuzzleSolved, 0, 3, 0));
            messages.Add(CreateMessage("Well done.", MessageCategory.Encouragement, MessageTrigger.PuzzleSolved, 0, 3, 0));
            messages.Add(CreateMessage("Your logic is sound.", MessageCategory.Encouragement, MessageTrigger.PuzzleSolved, 1, 3, 1));
            messages.Add(CreateMessage("Impressive deduction.", MessageCategory.Encouragement, MessageTrigger.PuzzleSolved, 2, 3, 2));
            
            messages.Add(CreateMessage("Flawless.", MessageCategory.Encouragement, MessageTrigger.PuzzleSolvedPerfectly, 0, 3, 0));
            messages.Add(CreateMessage("Perfect execution.", MessageCategory.Encouragement, MessageTrigger.PuzzleSolvedPerfectly, 1, 3, 1));
            messages.Add(CreateMessage("Your mind is sharper than I anticipated.", MessageCategory.Encouragement, MessageTrigger.PuzzleSolvedPerfectly, 2, 3, 2));
            
            messages.Add(CreateMessage("Check your surroundings.", MessageCategory.Puzzle, MessageTrigger.PlayerStuck, 0, 2, 0));
            messages.Add(CreateMessage("Look more carefully.", MessageCategory.Puzzle, MessageTrigger.PlayerStuck, 0, 2, 0));
            messages.Add(CreateMessage("The solution is closer than you think.", MessageCategory.Puzzle, MessageTrigger.PlayerStuck, 1, 3, 1));
            
            messages.Add(CreateMessage("Interesting challenge.", MessageCategory.Commentary, MessageTrigger.PuzzleEncountered, 0, 3, 0));
            messages.Add(CreateMessage("Logic will serve you here.", MessageCategory.Puzzle, MessageTrigger.PuzzleEncountered, 0, 3, 0));
            
            return messages;
        }

        private static List<UnknownMessage> GenerateExplorationMessages()
        {
            List<UnknownMessage> messages = new List<UnknownMessage>();
            
            messages.Add(CreateMessage("This space holds secrets.", MessageCategory.Exploration, MessageTrigger.NewAreaDiscovered, 0, 3, 0));
            messages.Add(CreateMessage("Unfamiliar territory.", MessageCategory.Exploration, MessageTrigger.NewAreaDiscovered, 0, 3, 0));
            messages.Add(CreateMessage("Interesting choice.", MessageCategory.Commentary, MessageTrigger.NewAreaDiscovered, 1, 3, 1));
            messages.Add(CreateMessage("Few have walked this path.", MessageCategory.Exploration, MessageTrigger.NewAreaDiscovered, 2, 3, 2));
            
            messages.Add(CreateMessage("Not many find this place.", MessageCategory.Encouragement, MessageTrigger.SecretFound, 0, 3, 0));
            messages.Add(CreateMessage("I wondered if you'd find that.", MessageCategory.Commentary, MessageTrigger.SecretFound, 1, 3, 0));
            messages.Add(CreateMessage("Perceptive.", MessageCategory.Encouragement, MessageTrigger.SecretFound, 0, 3, 1));
            messages.Add(CreateMessage("I didn't know this was possible.", MessageCategory.Commentary, MessageTrigger.SecretFound, 2, 3, 2));
            
            messages.Add(CreateMessage("Company.", MessageCategory.Exploration, MessageTrigger.NPCEncountered, 0, 3, 0));
            messages.Add(CreateMessage("You're not alone here.", MessageCategory.Warning, MessageTrigger.NPCEncountered, 0, 3, 0));
            messages.Add(CreateMessage("Approach with care.", MessageCategory.Warning, MessageTrigger.NPCEncountered, 1, 3, 1));
            
            messages.Add(CreateMessage("That might be useful.", MessageCategory.Exploration, MessageTrigger.ItemFound, 0, 3, 0));
            messages.Add(CreateMessage("Worth keeping.", MessageCategory.Exploration, MessageTrigger.ItemFound, 0, 3, 0));
            messages.Add(CreateMessage("Resourceful.", MessageCategory.Encouragement, MessageTrigger.ItemFound, 1, 3, 1));
            
            messages.Add(CreateMessage("Caution.", MessageCategory.Warning, MessageTrigger.DangerDetected, 0, 3, 0));
            messages.Add(CreateMessage("Something approaches.", MessageCategory.Warning, MessageTrigger.DangerDetected, 0, 3, 0));
            messages.Add(CreateMessage("Threat proximity increasing.", MessageCategory.Warning, MessageTrigger.DangerDetected, 1, 3, 1));
            messages.Add(CreateMessage("You should move.", MessageCategory.Warning, MessageTrigger.DangerDetected, 0, 2, 0));
            
            return messages;
        }

        private static List<UnknownMessage> GenerateMissionMessages()
        {
            List<UnknownMessage> messages = new List<UnknownMessage>();
            
            messages.Add(CreateMessage("Your objective is clear.", MessageCategory.Mission, MessageTrigger.MissionStart, 0, 3, 0));
            messages.Add(CreateMessage("Begin.", MessageCategory.Mission, MessageTrigger.MissionStart, 0, 3, 0));
            messages.Add(CreateMessage("This task requires precision.", MessageCategory.Mission, MessageTrigger.MissionStart, 1, 3, 1));
            
            messages.Add(CreateMessage("Progress.", MessageCategory.Encouragement, MessageTrigger.MissionMilestone, 0, 3, 0));
            messages.Add(CreateMessage("You're closer.", MessageCategory.Encouragement, MessageTrigger.MissionMilestone, 0, 3, 0));
            messages.Add(CreateMessage("Continue forward.", MessageCategory.Mission, MessageTrigger.MissionMilestone, 0, 3, 1));
            
            messages.Add(CreateMessage("Task completed.", MessageCategory.Encouragement, MessageTrigger.MissionComplete, 0, 3, 0));
            messages.Add(CreateMessage("Well done.", MessageCategory.Encouragement, MessageTrigger.MissionComplete, 0, 3, 0));
            messages.Add(CreateMessage("Your success was... expected.", MessageCategory.Commentary, MessageTrigger.MissionComplete, 2, 3, 2));
            
            messages.Add(CreateMessage("That wasn't supposed to happen.", MessageCategory.Commentary, MessageTrigger.ObjectiveFailed, 0, 3, 0));
            messages.Add(CreateMessage("Unexpected outcome.", MessageCategory.Commentary, MessageTrigger.ObjectiveFailed, 1, 3, 1));
            messages.Add(CreateMessage("Adjusting parameters.", MessageCategory.Commentary, MessageTrigger.ObjectiveFailed, 2, 3, 2));
            
            messages.Add(CreateMessage("Something new has emerged.", MessageCategory.Mission, MessageTrigger.NewMissionAvailable, 0, 3, 0));
            messages.Add(CreateMessage("New objective available.", MessageCategory.Mission, MessageTrigger.NewMissionAvailable, 0, 3, 0));
            messages.Add(CreateMessage("The situation evolves.", MessageCategory.Mission, MessageTrigger.NewMissionAvailable, 1, 3, 1));
            
            return messages;
        }

        private static List<UnknownMessage> GenerateNarrativeMessages()
        {
            List<UnknownMessage> messages = new List<UnknownMessage>();
            
            messages.Add(CreateMessage("You're not where you think you are.", MessageCategory.Narrative, MessageTrigger.PlotPointReached, 0, 3, 0));
            messages.Add(CreateMessage("This facility has many secrets.", MessageCategory.Narrative, MessageTrigger.PlotPointReached, 0, 3, 0));
            messages.Add(CreateMessage("You're not the first to be here.", MessageCategory.Narrative, MessageTrigger.PlotPointReached, 1, 3, 1));
            messages.Add(CreateMessage("This facility is older than you think.", MessageCategory.Narrative, MessageTrigger.PlotPointReached, 1, 3, 1));
            messages.Add(CreateMessage("Everything has a purpose.", MessageCategory.Narrative, MessageTrigger.PlotPointReached, 2, 3, 2));
            messages.Add(CreateMessage("Freedom isn't always what it seems.", MessageCategory.Narrative, MessageTrigger.PlotPointReached, 2, 3, 2));
            
            messages.Add(CreateMessage("The pieces align.", MessageCategory.Narrative, MessageTrigger.ProceduralStoryMilestone, 0, 3, 0));
            messages.Add(CreateMessage("Your path becomes clearer.", MessageCategory.Narrative, MessageTrigger.ProceduralStoryMilestone, 1, 3, 1));
            messages.Add(CreateMessage("History repeats.", MessageCategory.Narrative, MessageTrigger.ProceduralStoryMilestone, 2, 3, 2));
            
            messages.Add(CreateMessage("Significant.", MessageCategory.Commentary, MessageTrigger.MajorEventOccurred, 0, 3, 0));
            messages.Add(CreateMessage("This changes things.", MessageCategory.Commentary, MessageTrigger.MajorEventOccurred, 1, 3, 1));
            messages.Add(CreateMessage("The equation shifts.", MessageCategory.Commentary, MessageTrigger.MajorEventOccurred, 2, 3, 2));
            
            messages.Add(CreateMessage("Few know this truth.", MessageCategory.Narrative, MessageTrigger.SecretDiscovered, 0, 3, 0));
            messages.Add(CreateMessage("Now you understand.", MessageCategory.Narrative, MessageTrigger.SecretDiscovered, 1, 3, 1));
            messages.Add(CreateMessage("Knowledge has consequences.", MessageCategory.Narrative, MessageTrigger.SecretDiscovered, 2, 3, 2));
            
            return messages;
        }

        private static List<UnknownMessage> GenerateWarningMessages()
        {
            List<UnknownMessage> messages = new List<UnknownMessage>();
            
            messages.Add(CreateMessage("Careful with that.", MessageCategory.Warning, MessageTrigger.ItemFound, 0, 3, 0));
            messages.Add(CreateMessage("Handle with care.", MessageCategory.Warning, MessageTrigger.ItemFound, 1, 3, 1));
            messages.Add(CreateMessage("That's more dangerous than it appears.", MessageCategory.Warning, MessageTrigger.ItemFound, 2, 3, 2));
            
            return messages;
        }

        private static List<UnknownMessage> GenerateEncouragementMessages()
        {
            List<UnknownMessage> messages = new List<UnknownMessage>();
            
            messages.Add(CreateMessage("Your survival instincts are commendable.", MessageCategory.Encouragement, MessageTrigger.Custom, 0, 3, 0));
            messages.Add(CreateMessage("Impressive adaptability.", MessageCategory.Encouragement, MessageTrigger.Custom, 1, 3, 1));
            messages.Add(CreateMessage("You exceed expectations.", MessageCategory.Encouragement, MessageTrigger.Custom, 2, 3, 2));
            
            return messages;
        }

        private static List<UnknownMessage> GenerateCommentaryMessages()
        {
            List<UnknownMessage> messages = new List<UnknownMessage>();
            
            messages.Add(CreateMessage("Subtle.", MessageCategory.Commentary, MessageTrigger.StealthApproach, 0, 3, 0));
            messages.Add(CreateMessage("Wisdom.", MessageCategory.Commentary, MessageTrigger.StealthApproach, 1, 3, 1));
            messages.Add(CreateMessage("The silent path.", MessageCategory.Commentary, MessageTrigger.StealthApproach, 2, 3, 2));
            
            messages.Add(CreateMessage("Direct.", MessageCategory.Commentary, MessageTrigger.AggressiveApproach, 0, 3, 0));
            messages.Add(CreateMessage("Bold choice.", MessageCategory.Commentary, MessageTrigger.AggressiveApproach, 1, 3, 1));
            messages.Add(CreateMessage("Force has its place.", MessageCategory.Commentary, MessageTrigger.AggressiveApproach, 2, 3, 2));
            
            messages.Add(CreateMessage("Information is power.", MessageCategory.Commentary, MessageTrigger.DocumentRead, 0, 3, 0));
            messages.Add(CreateMessage("Knowledge accumulates.", MessageCategory.Commentary, MessageTrigger.DocumentRead, 1, 3, 1));
            messages.Add(CreateMessage("The truth lies in details.", MessageCategory.Commentary, MessageTrigger.DocumentRead, 2, 3, 2));
            
            messages.Add(CreateMessage("I'm watching you.", MessageCategory.Narrative, MessageTrigger.GameStart, 0, 3, 0));
            messages.Add(CreateMessage("Welcome to Protocol EMR.", MessageCategory.Narrative, MessageTrigger.GameStart, 0, 3, 0));
            messages.Add(CreateMessage("Your journey begins now.", MessageCategory.Narrative, MessageTrigger.GameStart, 0, 3, 0));
            
            messages.Add(CreateMessage("End of the line.", MessageCategory.Commentary, MessageTrigger.PlayerDeath, 0, 3, 0));
            messages.Add(CreateMessage("Unfortunate.", MessageCategory.Commentary, MessageTrigger.PlayerDeath, 0, 3, 0));
            messages.Add(CreateMessage("Try again.", MessageCategory.Commentary, MessageTrigger.PlayerDeath, 0, 3, 0));
            
            return messages;
        }

        private static List<UnknownMessage> GenerateDynamicEventMessages()
        {
            List<UnknownMessage> messages = new List<UnknownMessage>();
            
            messages.Add(CreateMessage("Ambient anomaly forming.", MessageCategory.Exploration, MessageTrigger.DynamicEventAmbient, 0, 3, 0));
            messages.Add(CreateMessage("Something subtle in that sector.", MessageCategory.Commentary, MessageTrigger.DynamicEventAmbient, 1, 3, 1));
            messages.Add(CreateMessage("Worldstate deviation recorded.", MessageCategory.Narrative, MessageTrigger.DynamicEventAmbient, 2, 3, 2));
            
            messages.Add(CreateMessage("Encounter wave inbound.", MessageCategory.Warning, MessageTrigger.DynamicEventCombat, 0, 3, 0));
            messages.Add(CreateMessage("They're converging on you.", MessageCategory.Warning, MessageTrigger.DynamicEventCombat, 1, 3, 1));
            messages.Add(CreateMessage("High-risk cluster detected.", MessageCategory.Warning, MessageTrigger.DynamicEventCombat, 2, 3, 2));
            
            messages.Add(CreateMessage("Puzzle node activating.", MessageCategory.Puzzle, MessageTrigger.DynamicEventPuzzle, 0, 3, 0));
            messages.Add(CreateMessage("Encryption layer exposed.", MessageCategory.Puzzle, MessageTrigger.DynamicEventPuzzle, 1, 3, 1));
            messages.Add(CreateMessage("This requires finesse.", MessageCategory.Commentary, MessageTrigger.DynamicEventPuzzle, 2, 3, 2));
            
            messages.Add(CreateMessage("Dynamic protocol engaged.", MessageCategory.Mission, MessageTrigger.DynamicEventStarted, 0, 3, 0));
            messages.Add(CreateMessage("Stay focused.", MessageCategory.Encouragement, MessageTrigger.DynamicEventStarted, 1, 3, 1));
            messages.Add(CreateMessage("Trajectory altered.", MessageCategory.Narrative, MessageTrigger.DynamicEventStarted, 2, 3, 2));
            
            messages.Add(CreateMessage("Resolved. For now.", MessageCategory.Commentary, MessageTrigger.DynamicEventResolved, 0, 3, 0));
            messages.Add(CreateMessage("Threat subsided.", MessageCategory.Encouragement, MessageTrigger.DynamicEventResolved, 1, 3, 1));
            messages.Add(CreateMessage("Stability restored.", MessageCategory.Narrative, MessageTrigger.DynamicEventResolved, 2, 3, 2));
            
            messages.Add(CreateMessage("Milestone reached.", MessageCategory.Mission, MessageTrigger.DynamicEventMilestone, 0, 3, 0));
            messages.Add(CreateMessage("Trajectory acceptable.", MessageCategory.Commentary, MessageTrigger.DynamicEventMilestone, 1, 3, 1));
            messages.Add(CreateMessage("The world adjusts with you.", MessageCategory.Narrative, MessageTrigger.DynamicEventMilestone, 2, 3, 2));
            
            messages.Add(CreateMessage("You lost that thread.", MessageCategory.Warning, MessageTrigger.DynamicEventFailed, 0, 3, 0));
            messages.Add(CreateMessage("System collapse in that sector.", MessageCategory.Warning, MessageTrigger.DynamicEventFailed, 1, 3, 1));
            messages.Add(CreateMessage("Failure recorded for analysis.", MessageCategory.Commentary, MessageTrigger.DynamicEventFailed, 2, 3, 2));
            
            return messages;
        }

        private static UnknownMessage CreateMessage(
            string text,
            MessageCategory category,
            MessageTrigger trigger,
            int minDifficulty,
            int maxDifficulty,
            int gameStage)
        {
            UnknownMessage msg = new UnknownMessage(text, category, trigger);
            msg.minDifficultyLevel = minDifficulty;
            msg.maxDifficultyLevel = maxDifficulty;
            msg.gameStage = gameStage;
            msg.displayMode = MessageDisplayMode.Both;
            msg.displayDelay = 0.5f;
            msg.displayDuration = 3f;
            msg.cooldown = 60f;
            msg.canRepeat = true;
            msg.selectionWeight = 1f;
            
            return msg;
        }
    }
}
