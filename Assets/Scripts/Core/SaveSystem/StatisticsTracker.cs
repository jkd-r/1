using System;
using UnityEngine;

namespace ProtocolEMR.Core.SaveSystem
{
    /// <summary>
    /// Tracks player statistics and progression throughout gameplay.
    /// Statistics are automatically saved with save files and accumulated in profiles.
    /// </summary>
    public class StatisticsTracker : MonoBehaviour
    {
        public static StatisticsTracker Instance { get; private set; }

        private PlayerStatistics currentStats;
        private float sessionStartTime;

        public PlayerStatistics CurrentStats => currentStats;

        public event Action<string, int> OnStatisticChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            currentStats = new PlayerStatistics();
            sessionStartTime = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            UpdatePlaytime();
        }

        private void UpdatePlaytime()
        {
            float sessionTime = Time.realtimeSinceStartup - sessionStartTime;
            currentStats.totalPlaytime = sessionTime;

            if (ProfileManager.Instance != null)
            {
                ProfileManager.Instance.UpdateProfilePlaytime(Time.unscaledDeltaTime);
            }
        }

        /// <summary>
        /// Increments mission completion count.
        /// </summary>
        public void IncrementMissionsCompleted()
        {
            currentStats.missionsCompleted++;
            OnStatisticChanged?.Invoke("missionsCompleted", currentStats.missionsCompleted);
            Debug.Log($"Missions completed: {currentStats.missionsCompleted}");
        }

        /// <summary>
        /// Increments NPC encounter count.
        /// </summary>
        public void IncrementNPCsEncountered()
        {
            currentStats.npcsEncountered++;
            OnStatisticChanged?.Invoke("npcsEncountered", currentStats.npcsEncountered);
        }

        /// <summary>
        /// Increments combat encounter count.
        /// </summary>
        public void IncrementCombatEncounters()
        {
            currentStats.combatEncounters++;
            OnStatisticChanged?.Invoke("combatEncounters", currentStats.combatEncounters);
        }

        /// <summary>
        /// Increments player death count.
        /// </summary>
        public void IncrementDeaths()
        {
            currentStats.deaths++;
            OnStatisticChanged?.Invoke("deaths", currentStats.deaths);
            Debug.Log($"Deaths: {currentStats.deaths}");
        }

        /// <summary>
        /// Increments collectibles found count.
        /// </summary>
        public void IncrementCollectiblesFound()
        {
            currentStats.collectiblesFound++;
            OnStatisticChanged?.Invoke("collectiblesFound", currentStats.collectiblesFound);
        }

        /// <summary>
        /// Increments puzzles solved count.
        /// </summary>
        public void IncrementPuzzlesSolved()
        {
            currentStats.puzzlesSolved++;
            OnStatisticChanged?.Invoke("puzzlesSolved", currentStats.puzzlesSolved);
            Debug.Log($"Puzzles solved: {currentStats.puzzlesSolved}");
        }

        /// <summary>
        /// Increments stealth kills count.
        /// </summary>
        public void IncrementStealthKills()
        {
            currentStats.stealthKills++;
            OnStatisticChanged?.Invoke("stealthKills", currentStats.stealthKills);
        }

        /// <summary>
        /// Increments alerts triggered count.
        /// </summary>
        public void IncrementAlertsTriggered()
        {
            currentStats.alertsTriggered++;
            OnStatisticChanged?.Invoke("alertsTriggered", currentStats.alertsTriggered);
        }

        /// <summary>
        /// Increments secrets found count.
        /// </summary>
        public void IncrementSecretsFound()
        {
            currentStats.secretsFound++;
            OnStatisticChanged?.Invoke("secretsFound", currentStats.secretsFound);
            Debug.Log($"Secrets found: {currentStats.secretsFound}");
        }

        /// <summary>
        /// Loads statistics from save data.
        /// </summary>
        public void LoadStatistics(PlayerStatistics stats)
        {
            currentStats = stats;
            sessionStartTime = Time.realtimeSinceStartup - stats.totalPlaytime;
            Debug.Log("Statistics loaded from save");
        }

        /// <summary>
        /// Resets all statistics (for new game).
        /// </summary>
        public void ResetStatistics()
        {
            currentStats = new PlayerStatistics();
            sessionStartTime = Time.realtimeSinceStartup;
            Debug.Log("Statistics reset");
        }

        /// <summary>
        /// Gets a formatted playtime string (HH:MM:SS).
        /// </summary>
        public string GetFormattedPlaytime()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(currentStats.totalPlaytime);
            return string.Format("{0:D2}:{1:D2}:{2:D2}", 
                timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }

        /// <summary>
        /// Gets a summary of current statistics.
        /// </summary>
        public string GetStatisticsSummary()
        {
            return $"Playtime: {GetFormattedPlaytime()}\n" +
                   $"Missions: {currentStats.missionsCompleted}\n" +
                   $"NPCs: {currentStats.npcsEncountered}\n" +
                   $"Combat: {currentStats.combatEncounters}\n" +
                   $"Deaths: {currentStats.deaths}\n" +
                   $"Collectibles: {currentStats.collectiblesFound}\n" +
                   $"Puzzles: {currentStats.puzzlesSolved}\n" +
                   $"Secrets: {currentStats.secretsFound}";
        }
    }
}
