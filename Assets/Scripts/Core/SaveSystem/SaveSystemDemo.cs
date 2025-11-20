using UnityEngine;
using ProtocolEMR.Core.SaveSystem;
using ProtocolEMR.UI;

namespace ProtocolEMR.Core
{
    /// <summary>
    /// Demonstration script for the save/load system.
    /// Press keys to test various save/load functionality.
    /// </summary>
    public class SaveSystemDemo : MonoBehaviour
    {
        [Header("Demo Settings")]
        [SerializeField] private bool showDebugUI = true;
        [SerializeField] private string demoProfileName = "DemoPlayer";

        private bool demoInitialized = false;
        private Rect windowRect = new Rect(20, 20, 400, 500);

        private void Start()
        {
            InitializeDemo();
        }

        private void InitializeDemo()
        {
            if (ProfileManager.Instance == null)
            {
                Debug.LogWarning("ProfileManager not found. Cannot initialize demo.");
                return;
            }

            if (ProfileManager.Instance.CurrentProfile == null)
            {
                ProfileData profile = ProfileManager.Instance.CreateProfile(
                    demoProfileName,
                    avatarIcon: 0,
                    difficulty: "Normal"
                );

                if (profile != null)
                {
                    ProfileManager.Instance.SwitchToProfile(profile.profileId);
                    Debug.Log($"Demo profile created: {demoProfileName}");
                    demoInitialized = true;
                }
            }
            else
            {
                Debug.Log($"Using existing profile: {ProfileManager.Instance.CurrentProfile.profileName}");
                demoInitialized = true;
            }
        }

        private void OnGUI()
        {
            if (!showDebugUI || !demoInitialized) return;

            windowRect = GUI.Window(0, windowRect, DrawDebugWindow, "Save/Load System Demo");
        }

        private void DrawDebugWindow(int windowID)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("=== PROFILE INFO ===", GUI.skin.box);
            if (ProfileManager.Instance?.CurrentProfile != null)
            {
                ProfileData profile = ProfileManager.Instance.CurrentProfile;
                GUILayout.Label($"Profile: {profile.profileName}");
                GUILayout.Label($"Playtime: {FormatPlaytime(profile.totalPlaytime)}");
                GUILayout.Label($"Missions: {profile.totalMissionsCompleted}");
                GUILayout.Label($"Deaths: {profile.cumulativeStats.totalDeaths}");
            }

            GUILayout.Space(10);
            GUILayout.Label("=== STATISTICS ===", GUI.skin.box);
            if (StatisticsTracker.Instance != null)
            {
                PlayerStatistics stats = StatisticsTracker.Instance.CurrentStats;
                GUILayout.Label($"Session Time: {StatisticsTracker.Instance.GetFormattedPlaytime()}");
                GUILayout.Label($"Missions: {stats.missionsCompleted}");
                GUILayout.Label($"NPCs: {stats.npcsEncountered}");
                GUILayout.Label($"Deaths: {stats.deaths}");
                GUILayout.Label($"Collectibles: {stats.collectiblesFound}");
            }

            GUILayout.Space(10);
            GUILayout.Label("=== CONTROLS ===", GUI.skin.box);
            GUILayout.Label("F5 - Quick Save");
            GUILayout.Label("F6 - Create Checkpoint");
            GUILayout.Label("F9 - Quick Load");
            GUILayout.Label("1 - Manual Save (Slot 0)");
            GUILayout.Label("2 - Increment Missions");
            GUILayout.Label("3 - Increment Deaths");
            GUILayout.Label("4 - Increment Collectibles");

            GUILayout.Space(10);
            GUILayout.Label("=== ACTIONS ===", GUI.skin.box);

            if (GUILayout.Button("Create New Profile"))
            {
                CreateNewDemoProfile();
            }

            if (GUILayout.Button("Manual Save (Slot 0)"))
            {
                PerformManualSave();
            }

            if (GUILayout.Button("Increment Mission Counter"))
            {
                IncrementMissionCounter();
            }

            if (GUILayout.Button("Increment Death Counter"))
            {
                IncrementDeathCounter();
            }

            if (GUILayout.Button("Show Save Slots"))
            {
                ShowSaveSlots();
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void Update()
        {
            if (!demoInitialized) return;

            // Number keys for quick actions
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                PerformManualSave();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                IncrementMissionCounter();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                IncrementDeathCounter();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                IncrementCollectibleCounter();
            }
        }

        private void CreateNewDemoProfile()
        {
            int profileCount = ProfileManager.Instance.AllProfiles.Count;
            string newName = $"DemoPlayer{profileCount + 1}";
            
            ProfileData profile = ProfileManager.Instance.CreateProfile(
                newName,
                avatarIcon: profileCount % 12,
                difficulty: "Normal"
            );

            if (profile != null)
            {
                ProfileManager.Instance.SwitchToProfile(profile.profileId);
                ShowNotification($"Created profile: {newName}");
            }
        }

        private void PerformManualSave()
        {
            if (SaveGameManager.Instance != null)
            {
                SaveGameManager.Instance.SaveGame(
                    SaveSlotType.ManualSave,
                    slotIndex: 0,
                    customName: "Demo Save"
                );
                ShowNotification("Manual save initiated (Slot 0)");
            }
        }

        private void IncrementMissionCounter()
        {
            if (StatisticsTracker.Instance != null)
            {
                StatisticsTracker.Instance.IncrementMissionsCompleted();
                ShowNotification("Mission completed!");
            }
        }

        private void IncrementDeathCounter()
        {
            if (StatisticsTracker.Instance != null)
            {
                StatisticsTracker.Instance.IncrementDeaths();
                ShowNotification("Player died!");
            }
        }

        private void IncrementCollectibleCounter()
        {
            if (StatisticsTracker.Instance != null)
            {
                StatisticsTracker.Instance.IncrementCollectiblesFound();
                ShowNotification("Collectible found!");
            }
        }

        private void ShowSaveSlots()
        {
            if (SaveGameManager.Instance != null)
            {
                var saves = SaveGameManager.Instance.GetSaveSlots();
                Debug.Log($"=== AVAILABLE SAVE SLOTS ({saves.Count}) ===");
                
                foreach (var save in saves)
                {
                    Debug.Log($"  {save.slotType} {save.slotIndex}: {save.locationName} - {save.playtime}s");
                }
            }
        }

        private void ShowNotification(string message)
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification(message);
            }
            else
            {
                Debug.Log($"[Demo] {message}");
            }
        }

        private string FormatPlaytime(float seconds)
        {
            System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(seconds);
            return string.Format("{0:D2}:{1:D2}:{2:D2}", 
                timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }
    }
}
