using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProtocolEMR.Core.Dialogue;
using ProtocolEMR.Core.Settings;

namespace ProtocolEMR.Core.Demo
{
    /// <summary>
    /// Demo controller for testing the Unknown Dialogue System.
    /// Provides UI for triggering different message types and testing features.
    /// </summary>
    public class UnknownDialogueDemoController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject demoPanel;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Slider hintFrequencySlider;
        [SerializeField] private TextMeshProUGUI hintFrequencyText;
        [SerializeField] private Dropdown personalityDropdown;
        [SerializeField] private Toggle enableMessagesToggle;

        [Header("Demo Settings")]
        [SerializeField] private KeyCode toggleDemoKey = KeyCode.F6;
        [SerializeField] private bool showDemoOnStart = true;

        private bool isDemoVisible = false;

        private void Start()
        {
            if (demoPanel != null)
            {
                demoPanel.SetActive(showDemoOnStart);
                isDemoVisible = showDemoOnStart;
            }

            InitializeUI();
            UpdateStatusText();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleDemoKey))
            {
                ToggleDemoPanel();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestCombatMessages();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestPuzzleMessages();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TestExplorationMessages();
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                TestMissionMessages();
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                TestNarrativeMessages();
            }
        }

        private void InitializeUI()
        {
            if (hintFrequencySlider != null)
            {
                float currentFrequency = UnknownDialogueManager.Instance != null 
                    ? UnknownDialogueManager.Instance.HintFrequency 
                    : 0.5f;
                
                hintFrequencySlider.value = currentFrequency;
                hintFrequencySlider.onValueChanged.AddListener(OnHintFrequencyChanged);
                UpdateHintFrequencyText(currentFrequency);
            }

            if (personalityDropdown != null)
            {
                personalityDropdown.ClearOptions();
                personalityDropdown.AddOptions(new System.Collections.Generic.List<string> 
                { 
                    "Verbose", 
                    "Balanced", 
                    "Cryptic" 
                });
                
                int currentPersonality = UnknownDialogueManager.Instance != null 
                    ? (int)UnknownDialogueManager.Instance.Personality 
                    : 1;
                
                personalityDropdown.value = currentPersonality;
                personalityDropdown.onValueChanged.AddListener(OnPersonalityChanged);
            }

            if (enableMessagesToggle != null)
            {
                bool enabled = SettingsManager.Instance != null 
                    ? SettingsManager.Instance.IsUnknownMessagesEnabled() 
                    : true;
                
                enableMessagesToggle.isOn = enabled;
                enableMessagesToggle.onValueChanged.AddListener(OnEnableMessagesChanged);
            }
        }

        private void ToggleDemoPanel()
        {
            isDemoVisible = !isDemoVisible;
            if (demoPanel != null)
            {
                demoPanel.SetActive(isDemoVisible);
            }
        }

        private void OnHintFrequencyChanged(float value)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.SetHintFrequency(value);
            }

            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SetUnknownHintFrequency(value);
            }

            UpdateHintFrequencyText(value);
            UpdateStatusText();
        }

        private void UpdateHintFrequencyText(float value)
        {
            if (hintFrequencyText != null)
            {
                hintFrequencyText.text = $"Hint Frequency: {(value * 100):F0}%";
            }
        }

        private void OnPersonalityChanged(int index)
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.Personality = (UnknownPersonality)index;
            }

            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SetUnknownPersonality(index);
            }

            UpdateStatusText();
        }

        private void OnEnableMessagesChanged(bool enabled)
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SetUnknownMessagesEnabled(enabled);
            }

            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            if (statusText == null || UnknownDialogueManager.Instance == null)
                return;

            string status = $"Unknown Dialogue System Status\n\n";
            status += $"Messages: {(SettingsManager.Instance?.IsUnknownMessagesEnabled() ?? true ? "Enabled" : "Disabled")}\n";
            status += $"Hint Frequency: {(UnknownDialogueManager.Instance.HintFrequency * 100):F0}%\n";
            status += $"Personality: {UnknownDialogueManager.Instance.Personality}\n";
            status += $"Game Stage: {UnknownDialogueManager.Instance.GameStage}\n";
            status += $"Difficulty: {UnknownDialogueManager.Instance.DifficultyLevel}\n\n";

            PlayerStyleProfile profile = UnknownDialogueManager.Instance.GetPlayerStyleProfile();
            status += $"Player Style Profile:\n";
            status += $"Total Actions: {profile.totalActions}\n";
            status += $"Stealth: {(profile.stealthRatio * 100):F0}%\n";
            status += $"Aggression: {(profile.aggressionRatio * 100):F0}%\n";
            status += $"Exploration: {(profile.explorationRatio * 100):F0}%\n\n";

            status += "Keyboard Shortcuts:\n";
            status += "F6 - Toggle Demo Panel\n";
            status += "1 - Test Combat Messages\n";
            status += "2 - Test Puzzle Messages\n";
            status += "3 - Test Exploration Messages\n";
            status += "4 - Test Mission Messages\n";
            status += "5 - Test Narrative Messages\n";

            statusText.text = status;
        }

        public void TestCombatMessages()
        {
            Debug.Log("Testing Combat Messages");
            UnknownDialogueTriggers.TriggerPlayerHitNPC(gameObject, 25f);
        }

        public void TestPlayerDamage()
        {
            Debug.Log("Testing Player Damage");
            UnknownDialogueTriggers.TriggerPlayerTookDamage(gameObject, 20f, 50f, 100f);
        }

        public void TestLowHealth()
        {
            Debug.Log("Testing Low Health Warning");
            UnknownDialogueTriggers.TriggerPlayerTookDamage(gameObject, 10f, 25f, 100f);
        }

        public void TestNPCDefeated()
        {
            Debug.Log("Testing NPC Defeated");
            UnknownDialogueTriggers.TriggerNPCDefeated(gameObject);
        }

        public void TestDodgeSuccessful()
        {
            Debug.Log("Testing Dodge Successful");
            UnknownDialogueTriggers.TriggerDodgeSuccessful();
        }

        public void TestCombatStarted()
        {
            Debug.Log("Testing Combat Started");
            UnknownDialogueTriggers.TriggerCombatStarted(gameObject);
        }

        public void TestPuzzleMessages()
        {
            Debug.Log("Testing Puzzle Messages");
            UnknownDialogueTriggers.TriggerPuzzleEncountered(gameObject);
        }

        public void TestPuzzleFailed()
        {
            Debug.Log("Testing Puzzle Failed");
            UnknownDialogueTriggers.TriggerPuzzleAttemptFailed(gameObject);
        }

        public void TestPuzzleSolved()
        {
            Debug.Log("Testing Puzzle Solved");
            UnknownDialogueTriggers.TriggerPuzzleSolved(gameObject, false);
        }

        public void TestPuzzlePerfect()
        {
            Debug.Log("Testing Puzzle Perfect");
            UnknownDialogueTriggers.TriggerPuzzleSolved(gameObject, true);
        }

        public void TestPlayerStuck()
        {
            Debug.Log("Testing Player Stuck");
            UnknownDialogueTriggers.TriggerPlayerStuck();
        }

        public void TestExplorationMessages()
        {
            Debug.Log("Testing Exploration Messages");
            UnknownDialogueTriggers.TriggerNewAreaDiscovered("Test Area");
        }

        public void TestSecretFound()
        {
            Debug.Log("Testing Secret Found");
            UnknownDialogueTriggers.TriggerSecretFound(gameObject);
        }

        public void TestNPCEncountered()
        {
            Debug.Log("Testing NPC Encountered");
            UnknownDialogueTriggers.TriggerNPCEncountered(gameObject);
        }

        public void TestItemFound()
        {
            Debug.Log("Testing Item Found");
            UnknownDialogueTriggers.TriggerItemFound(gameObject);
        }

        public void TestDangerDetected()
        {
            Debug.Log("Testing Danger Detected");
            UnknownDialogueTriggers.TriggerDangerDetected(gameObject);
        }

        public void TestMissionMessages()
        {
            Debug.Log("Testing Mission Messages");
            UnknownDialogueTriggers.TriggerMissionStart("Test Mission");
        }

        public void TestMissionMilestone()
        {
            Debug.Log("Testing Mission Milestone");
            UnknownDialogueTriggers.TriggerMissionMilestone("Test Milestone");
        }

        public void TestMissionComplete()
        {
            Debug.Log("Testing Mission Complete");
            UnknownDialogueTriggers.TriggerMissionComplete("Test Mission");
        }

        public void TestObjectiveFailed()
        {
            Debug.Log("Testing Objective Failed");
            UnknownDialogueTriggers.TriggerObjectiveFailed("Test Objective");
        }

        public void TestNewMissionAvailable()
        {
            Debug.Log("Testing New Mission Available");
            UnknownDialogueTriggers.TriggerNewMissionAvailable("New Test Mission");
        }

        public void TestNarrativeMessages()
        {
            Debug.Log("Testing Narrative Messages");
            UnknownDialogueTriggers.TriggerPlotPointReached("Test Plot Point");
        }

        public void TestProceduralStory()
        {
            Debug.Log("Testing Procedural Story");
            UnknownDialogueTriggers.TriggerProceduralStoryMilestone("Test Milestone");
        }

        public void TestMajorEvent()
        {
            Debug.Log("Testing Major Event");
            UnknownDialogueTriggers.TriggerMajorEventOccurred("Test Event");
        }

        public void TestSecretDiscovered()
        {
            Debug.Log("Testing Secret Discovered");
            UnknownDialogueTriggers.TriggerSecretDiscovered("Test Secret");
        }

        public void TestStealthApproach()
        {
            Debug.Log("Testing Stealth Approach");
            UnknownDialogueTriggers.TriggerStealthApproach();
        }

        public void TestAggressiveApproach()
        {
            Debug.Log("Testing Aggressive Approach");
            UnknownDialogueTriggers.TriggerAggressiveApproach();
        }

        public void TestDocumentRead()
        {
            Debug.Log("Testing Document Read");
            UnknownDialogueTriggers.TriggerDocumentRead("Test Document");
        }

        public void ClearMessageHistory()
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.ClearMessageHistory();
                Debug.Log("Message history cleared");
            }
            UpdateStatusText();
        }

        public void CycleGameStage()
        {
            if (UnknownDialogueManager.Instance != null)
            {
                int currentStage = UnknownDialogueManager.Instance.GameStage;
                int nextStage = (currentStage + 1) % 3;
                UnknownDialogueManager.Instance.GameStage = nextStage;
                Debug.Log($"Game stage changed to: {nextStage}");
            }
            UpdateStatusText();
        }

        public void CycleDifficulty()
        {
            if (UnknownDialogueManager.Instance != null)
            {
                int currentDiff = UnknownDialogueManager.Instance.DifficultyLevel;
                int nextDiff = (currentDiff + 1) % 4;
                UnknownDialogueManager.Instance.DifficultyLevel = nextDiff;
                Debug.Log($"Difficulty changed to: {nextDiff}");
            }
            UpdateStatusText();
        }
    }
}
