using UnityEngine;
using ProtocolEMR.Core.Procedural;
using ProtocolEMR.Core.Dialogue;

namespace ProtocolEMR.Core.Demo
{
    /// <summary>
    /// Demo controller for testing the Dynamic Event Orchestrator system.
    /// Provides UI and shortcuts to simulate various world states and event scenarios.
    /// </summary>
    public class DynamicEventOrchestratorDemo : MonoBehaviour
    {
        [Header("Demo Settings")]
        [SerializeField] private bool enableDemo = true;
        [SerializeField] private KeyCode toggleDemoUIKey = KeyCode.F10;
        
        [Header("Quick Test Keys")]
        [SerializeField] private KeyCode spawnCombatEventKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode spawnPuzzleEventKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode spawnAmbientEventKey = KeyCode.Alpha3;
        [SerializeField] private KeyCode changeThreatLevelKey = KeyCode.T;
        [SerializeField] private KeyCode changeChunkKey = KeyCode.C;
        [SerializeField] private KeyCode completeEventKey = KeyCode.E;
        
        private bool showUI = false;
        private float simulatedThreatLevel = 0.5f;
        private int simulatedChunkId = 0;
        private Vector2 scrollPosition;

        private void Update()
        {
            if (!enableDemo) return;

            if (Input.GetKeyDown(toggleDemoUIKey))
            {
                showUI = !showUI;
            }

            // Quick test shortcuts
            if (Input.GetKeyDown(spawnCombatEventKey))
            {
                SpawnTestCombatEvent();
            }

            if (Input.GetKeyDown(spawnPuzzleEventKey))
            {
                SpawnTestPuzzleEvent();
            }

            if (Input.GetKeyDown(spawnAmbientEventKey))
            {
                SpawnTestAmbientEvent();
            }

            if (Input.GetKeyDown(changeThreatLevelKey))
            {
                CycleThreatLevel();
            }

            if (Input.GetKeyDown(changeChunkKey))
            {
                ChangeChunk();
            }

            if (Input.GetKeyDown(completeEventKey))
            {
                CompleteActiveEvent();
            }
        }

        private void SpawnTestCombatEvent()
        {
            if (DynamicEventOrchestrator.Instance == null)
            {
                Debug.LogWarning("DynamicEventOrchestrator not found!");
                return;
            }

            // Simulate a combat event by setting high threat level
            DynamicEventOrchestrator.Instance.SetThreatLevel(simulatedChunkId, 0.8f);
            DynamicEventOrchestrator.Instance.RecordPlayerAction(PlayerActionType.Combat);
            
            Debug.Log("Simulated combat event trigger (high threat)");
        }

        private void SpawnTestPuzzleEvent()
        {
            if (DynamicEventOrchestrator.Instance == null) return;

            // Simulate a puzzle event by recording puzzle actions
            DynamicEventOrchestrator.Instance.RecordPlayerAction(PlayerActionType.Puzzle);
            DynamicEventOrchestrator.Instance.SetThreatLevel(simulatedChunkId, 0.3f);
            
            Debug.Log("Simulated puzzle event trigger");
        }

        private void SpawnTestAmbientEvent()
        {
            if (DynamicEventOrchestrator.Instance == null) return;

            // Simulate an ambient event by recording exploration
            DynamicEventOrchestrator.Instance.RecordPlayerAction(PlayerActionType.Exploration);
            DynamicEventOrchestrator.Instance.SetThreatLevel(simulatedChunkId, 0.2f);
            
            Debug.Log("Simulated ambient event trigger");
        }

        private void CycleThreatLevel()
        {
            if (DynamicEventOrchestrator.Instance == null) return;

            simulatedThreatLevel += 0.25f;
            if (simulatedThreatLevel > 1f) simulatedThreatLevel = 0f;

            DynamicEventOrchestrator.Instance.SetThreatLevel(simulatedChunkId, simulatedThreatLevel);
            Debug.Log($"Threat level set to: {simulatedThreatLevel:P0}");
        }

        private void ChangeChunk()
        {
            if (DynamicEventOrchestrator.Instance == null) return;

            simulatedChunkId++;
            Vector3 newPosition = new Vector3(simulatedChunkId * 50f, 0, 0);
            DynamicEventOrchestrator.Instance.SetChunk(simulatedChunkId, newPosition);
            
            Debug.Log($"Changed to chunk: {simulatedChunkId}");
        }

        private void CompleteActiveEvent()
        {
            if (DynamicEventOrchestrator.Instance == null) return;

            var activeEvents = DynamicEventOrchestrator.Instance.ActiveEvents;
            if (activeEvents.Count > 0)
            {
                DynamicEventOrchestrator.Instance.CompleteEvent(activeEvents[0].eventId, true);
                Debug.Log($"Completed event: {activeEvents[0].eventId}");
            }
            else
            {
                Debug.Log("No active events to complete");
            }
        }

        private void OnGUI()
        {
            if (!showUI || !enableDemo) return;

            GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
            windowStyle.fontSize = 14;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 12;
            labelStyle.wordWrap = true;

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 12;

            float windowWidth = 400f;
            float windowHeight = 600f;
            Rect windowRect = new Rect(Screen.width - windowWidth - 20, 20, windowWidth, windowHeight);

            GUI.Window(1, windowRect, DrawDemoWindow, "Dynamic Event Orchestrator Demo", windowStyle);
        }

        private void DrawDemoWindow(int windowID)
        {
            GUILayout.BeginVertical();

            // Header
            GUILayout.Label("<b>QUICK TEST CONTROLS</b>", GUI.skin.label);
            GUILayout.Space(10);

            // Quick test buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"Combat Event ({spawnCombatEventKey})"))
            {
                SpawnTestCombatEvent();
            }
            if (GUILayout.Button($"Puzzle Event ({spawnPuzzleEventKey})"))
            {
                SpawnTestPuzzleEvent();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"Ambient Event ({spawnAmbientEventKey})"))
            {
                SpawnTestAmbientEvent();
            }
            if (GUILayout.Button($"Change Chunk ({changeChunkKey})"))
            {
                ChangeChunk();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"Cycle Threat ({changeThreatLevelKey})"))
            {
                CycleThreatLevel();
            }
            if (GUILayout.Button($"Complete Event ({completeEventKey})"))
            {
                CompleteActiveEvent();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("<b>WORLD STATE</b>", GUI.skin.label);
            
            if (DynamicEventOrchestrator.Instance != null)
            {
                var worldState = DynamicEventOrchestrator.Instance.WorldState;
                GUILayout.Label($"Current Chunk: {worldState.CurrentChunkId}");
                GUILayout.Label($"Threat Level: {worldState.GetChunkThreatLevel(worldState.CurrentChunkId):P0}");
                GUILayout.Label($"Game Progress: {worldState.GameProgress:P0}");
                GUILayout.Label($"Mission Phase: {worldState.CurrentMissionPhase}");
                
                GUILayout.Space(10);
                GUILayout.Label("<b>PLAYER STYLE</b>", GUI.skin.label);
                GUILayout.Label($"Stealth: {worldState.PlayerStyle.StealthRatio:P0}");
                GUILayout.Label($"Combat: {worldState.PlayerStyle.CombatRatio:P0}");
                GUILayout.Label($"Exploration: {worldState.PlayerStyle.ExplorationRatio:P0}");
                GUILayout.Label($"Puzzle: {worldState.PlayerStyle.PuzzleRatio:P0}");
                GUILayout.Label($"Total Actions: {worldState.PlayerStyle.TotalActions}");
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>ACTIVE EVENTS</b>", GUI.skin.label);

            if (DynamicEventOrchestrator.Instance != null)
            {
                var activeEvents = DynamicEventOrchestrator.Instance.ActiveEvents;
                
                if (activeEvents.Count == 0)
                {
                    GUILayout.Label("No active events");
                }
                else
                {
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
                    
                    foreach (var evt in activeEvents)
                    {
                        GUILayout.BeginVertical(GUI.skin.box);
                        GUILayout.Label($"<b>{evt.profile.eventName}</b>");
                        GUILayout.Label($"Type: {evt.profile.eventType}");
                        GUILayout.Label($"Chunk: {evt.chunkId}");
                        GUILayout.Label($"Time: {evt.ElapsedTime:F1}s / {evt.duration:F1}s");
                        GUILayout.Label($"Progress: {evt.progress}%");
                        GUILayout.EndVertical();
                        GUILayout.Space(5);
                    }
                    
                    GUILayout.EndScrollView();
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>ORCHESTRATOR STATUS</b>", GUI.skin.label);
            
            if (DynamicEventOrchestrator.Instance != null)
            {
                bool orchestrationEnabled = DynamicEventOrchestrator.Instance.EnableOrchestration;
                string statusText = orchestrationEnabled ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>";
                GUILayout.Label($"Orchestration: {statusText}");
                
                if (GUILayout.Button(orchestrationEnabled ? "Disable Orchestration" : "Enable Orchestration"))
                {
                    DynamicEventOrchestrator.Instance.EnableOrchestration = !orchestrationEnabled;
                }
            }
            else
            {
                GUILayout.Label("<color=red>Orchestrator not found!</color>");
            }

            GUILayout.Space(10);
            if (GUILayout.Button($"Close ({toggleDemoUIKey})"))
            {
                showUI = false;
            }

            GUILayout.EndVertical();
        }
    }
}
