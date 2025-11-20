using UnityEngine;
using ProtocolEMR.Core.Input;

namespace ProtocolEMR.Core.Procedural
{
    /// <summary>
    /// Demo controller for testing procedural level generation.
    /// Provides UI controls and metrics display for development/QA.
    /// </summary>
    public class ProceduralLevelTester : MonoBehaviour
    {
        [SerializeField] private ProceduralLevelBuilder levelBuilder;
        [SerializeField] private KeyCode regenerateKey = KeyCode.R;
        [SerializeField] private KeyCode clearKey = KeyCode.C;
        [SerializeField] private bool showUI = true;

        private Rect uiRect = new Rect(10, 10, 300, 200);
        private int totalGenerations = 0;
        private float lastGenerationTime = 0;

        private void Start()
        {
            if (levelBuilder == null)
            {
                levelBuilder = FindObjectOfType<ProceduralLevelBuilder>();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(regenerateKey))
            {
                if (levelBuilder != null)
                {
                    Debug.Log("Regenerating level...");
                    levelBuilder.ClearLevel();
                    levelBuilder.GenerateLevel();
                    totalGenerations++;
                }
            }

            if (Input.GetKeyDown(clearKey))
            {
                if (levelBuilder != null)
                {
                    Debug.Log("Clearing level...");
                    levelBuilder.ClearLevel();
                }
            }
        }

        private void OnGUI()
        {
            if (!showUI)
                return;

            GUI.Window(0, uiRect, DrawUI, "Procedural Level Tester");
        }

        private void DrawUI(int windowID)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("=== Level Generation Controls ===", EditorStyles.boldLabel);
            
            GUILayout.Space(10);

            if (levelBuilder == null)
            {
                GUILayout.Label("ERROR: ProceduralLevelBuilder not found!", EditorStyles.helpBox);
            }
            else
            {
                GUILayout.Label($"Status: {(levelBuilder.IsGenerating ? "GENERATING..." : (levelBuilder.GenerationComplete ? "COMPLETE" : "IDLE"))}");
                GUILayout.Label($"Chunks: {levelBuilder.GeneratedChunks.Count}");
                
                if (GUILayout.Button("Regenerate Level (R)", GUILayout.Height(30)))
                {
                    levelBuilder.ClearLevel();
                    levelBuilder.GenerateLevel();
                    totalGenerations++;
                }

                if (GUILayout.Button("Clear Level (C)", GUILayout.Height(30)))
                {
                    levelBuilder.ClearLevel();
                }

                GUILayout.Space(10);
                GUILayout.Label("Statistics:", EditorStyles.boldLabel);
                GUILayout.Label($"Total Generations: {totalGenerations}");
                GUILayout.Label($"Seed: {(SeedManager.Instance != null ? SeedManager.Instance.CurrentSeed.ToString() : "N/A")}");
            }

            GUILayout.Space(10);
            GUILayout.Label("F8: Copy Seed to Clipboard");
            GUILayout.Label("ESC: Toggle UI (not implemented)");

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private class EditorStyles
        {
            public static GUIStyle boldLabel
            {
                get
                {
                    var style = new GUIStyle(GUI.skin.label);
                    style.fontStyle = FontStyle.Bold;
                    return style;
                }
            }

            public static GUIStyle helpBox => GUI.skin.box;
        }
    }
}
