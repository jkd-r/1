using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProtocolEMR.Core.AI
{
    /// <summary>
    /// Alert data structure for NPC communication.
    /// </summary>
    [System.Serializable]
    public struct AlertData
    {
        public Vector3 alertPosition;
        public GameObject alertSource;
        public float alertLevel;
        public AlertType alertType;
        public float timestamp;
        public int alertingNPCId;
    }

    /// <summary>
    /// Types of alerts that NPCs can communicate.
    /// </summary>
    public enum AlertType
    {
        VisualSighting,
        SoundHeard,
        AttackDetected,
        AllyDown,
        AlarmTriggered,
        BackupNeeded
    }

    /// <summary>
    /// Group behavior configuration for NPC coordination.
    /// </summary>
    [System.Serializable]
    public class NPCGroup
    {
        [Header("Group Configuration")]
        public string groupName = "Default Group";
        public NPCType groupType = NPCType.Guard;
        public List<NPCController> members = new List<NPCController>();
        
        [Header("Communication")]
        public float communicationRange = 20f;
        public float alertPropagationDelay = 0.5f;
        public bool enableGroupCoordination = true;
        
        [Header("Tactics")]
        public bool enableFlanking = true;
        public bool enableCovering = true;
        public bool enableRetreatCoordination = true;
        public float minimumGroupSize = 2;
        
        [Header("Morale")]
        public float groupMorale = 100f;
        public float moraleLossPerCasualty = 20f;
        public float moraleRecoveryRate = 5f;
        public bool enableMoraleSystem = true;

        // Runtime data
        private Vector3 lastKnownPlayerPosition;
        private float lastAlertTime;
        private NPCController groupLeader;
        private List<AlertData> activeAlerts = new List<AlertData>();

        public NPCController GroupLeader
        {
            get
            {
                if (groupLeader == null || !members.Contains(groupLeader))
                {
                    SelectNewLeader();
                }
                return groupLeader;
            }
        }

        public Vector3 LastKnownPlayerPosition => lastKnownPlayerPosition;
        public float GroupMorale => groupMorale;
        public int MemberCount => members.Count;
        public List<AlertData> ActiveAlerts => activeAlerts;

        /// <summary>
        /// Adds NPC to group.
        /// </summary>
        public void AddMember(NPCController npc)
        {
            if (npc != null && !members.Contains(npc))
            {
                members.Add(npc);
                
                if (groupLeader == null)
                {
                    groupLeader = npc;
                }
            }
        }

        /// <summary>
        /// Removes NPC from group.
        /// </summary>
        public void RemoveMember(NPCController npc)
        {
            if (members.Remove(npc))
            {
                // Handle morale loss from casualty
                if (enableMoraleSystem && npc.CurrentState == NPCState.Dead)
                {
                    groupMorale = Mathf.Max(0f, groupMorale - moraleLossPerCasualty);
                    
                    // Alert other members about ally down
                    BroadcastAlert(new AlertData
                    {
                        alertPosition = npc.transform.position,
                        alertSource = npc.gameObject,
                        alertLevel = 50f,
                        alertType = AlertType.AllyDown,
                        timestamp = Time.time,
                        alertingNPCId = npc.GetInstanceID()
                    });
                }

                // Select new leader if needed
                if (groupLeader == npc)
                {
                    SelectNewLeader();
                }
            }
        }

        /// <summary>
        /// Selects new group leader.
        /// </summary>
        private void SelectNewLeader()
        {
            if (members.Count == 0)
            {
                groupLeader = null;
                return;
            }

            // Select NPC with highest intelligence as leader
            groupLeader = members.OrderByDescending(npc => npc.Parameters.intelligence).First();
        }

        /// <summary>
        /// Broadcasts alert to all group members.
        /// </summary>
        public void BroadcastAlert(AlertData alert)
        {
            activeAlerts.Add(alert);
            
            // Clean old alerts
            CleanupOldAlerts();

            // Notify all members
            foreach (var member in members)
            {
                if (member != null && member.gameObject.activeInHierarchy)
                {
                    float distance = Vector3.Distance(member.transform.position, alert.alertPosition);
                    if (distance <= communicationRange)
                    {
                        NotifyMember(member, alert);
                    }
                }
            }

            lastKnownPlayerPosition = alert.alertPosition;
            lastAlertTime = Time.time;
        }

        /// <summary>
        /// Notifies individual member about alert.
        /// </summary>
        private void NotifyMember(NPCController member, AlertData alert)
        {
            switch (alert.alertType)
            {
                case AlertType.VisualSighting:
                    member.Perception.RegisterSound(alert.alertPosition, alert.alertLevel / 50f);
                    break;
                    
                case AlertType.SoundHeard:
                    member.RegisterSound(alert.alertPosition, alert.alertLevel / 50f);
                    break;
                    
                case AlertType.AttackDetected:
                case AlertType.AlarmTriggered:
                    member.SetState(NPCState.Alert);
                    member.Navigation.SetTargetPosition(alert.alertPosition);
                    break;
                    
                case AlertType.AllyDown:
                    // Increase alertness and possibly change tactics
                    if (member.Parameters.intelligence > 50f)
                    {
                        member.SetState(NPCState.Alert);
                    }
                    break;
                    
                case AlertType.BackupNeeded:
                    // Move to assist position
                    member.SetState(NPCState.Chase);
                    member.Navigation.SetTargetPosition(alert.alertPosition);
                    break;
            }
        }

        /// <summary>
        /// Coordinates group attack on player.
        /// </summary>
        public void CoordinateAttack(GameObject target)
        {
            if (!enableGroupCoordination || members.Count < minimumGroupSize)
                return;

            Vector3 targetPosition = target.transform.position;
            
            // Assign different roles to members
            List<NPCController> availableMembers = members.Where(m => m != null && m.CurrentState != NPCState.Dead).ToList();
            
            for (int i = 0; i < availableMembers.Count; i++)
            {
                NPCController member = availableMembers[i];
                Vector3 attackPosition = CalculateAttackPosition(member, targetPosition, i, availableMembers.Count);
                
                member.Navigation.SetTargetPosition(attackPosition);
                member.SetState(NPCState.Chase);
            }
        }

        /// <summary>
        /// Calculates attack position for flanking behavior.
        /// </summary>
        private Vector3 CalculateAttackPosition(NPCController member, Vector3 targetPosition, int index, int totalMembers)
        {
            if (!enableFlanking || totalMembers == 1)
            {
                return targetPosition;
            }

            // Calculate flanking positions around target
            float angle = (360f / totalMembers) * index;
            float distance = 5f; // Distance from target
            
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 flankingPosition = targetPosition + direction * distance;
            
            // Ensure position is on NavMesh
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(flankingPosition, out hit, 3f, UnityEngine.AI.NavMesh.AllAreas))
            {
                return hit.position;
            }
            
            return targetPosition;
        }

        /// <summary>
        /// Coordinates group retreat.
        /// </summary>
        public void CoordinateRetreat(Vector3 retreatPosition)
        {
            if (!enableRetreatCoordination)
                return;

            foreach (var member in members)
            {
                if (member != null && member.CurrentState != NPCState.Dead)
                {
                    member.SetState(NPCState.Flee);
                    member.Navigation.SetTargetPosition(retreatPosition);
                }
            }
        }

        /// <summary>
        /// Updates group behavior and morale.
        /// </summary>
        public void UpdateGroup()
        {
            // Remove dead or null members
            members.RemoveAll(m => m == null);
            
            // Update morale
            if (enableMoraleSystem)
            {
                UpdateMorale();
            }
            
            // Clean old alerts
            CleanupOldAlerts();
        }

        /// <summary>
        /// Updates group morale over time.
        /// </summary>
        private void UpdateMorale()
        {
            // Recover morale slowly when not under attack
            bool underAttack = members.Any(m => m != null && m.CurrentState == NPCState.Attack);
            
            if (!underAttack)
            {
                groupMorale = Mathf.Min(100f, groupMorale + moraleRecoveryRate * Time.deltaTime);
            }
            
            // Check for group break
            if (groupMorale < 30f && members.Count > 0)
            {
                TriggerGroupBreak();
            }
        }

        /// <summary>
        /// Triggers group break (all members flee).
        /// </summary>
        private void TriggerGroupBreak()
        {
            foreach (var member in members)
            {
                if (member != null && member.CurrentState != NPCState.Dead)
                {
                    member.SetState(NPCState.Flee);
                }
            }
        }

        /// <summary>
        /// Cleans old alerts from the list.
        /// </summary>
        private void CleanupOldAlerts()
        {
            float currentTime = Time.time;
            activeAlerts.RemoveAll(alert => currentTime - alert.timestamp > 30f);
        }

        /// <summary>
        /// Gets group cohesion rating.
        /// </summary>
        public float GetGroupCohesion()
        {
            if (members.Count == 0) return 0f;
            
            float totalDistance = 0f;
            int validPairs = 0;
            
            for (int i = 0; i < members.Count; i++)
            {
                for (int j = i + 1; j < members.Count; j++)
                {
                    if (members[i] != null && members[j] != null)
                    {
                        totalDistance += Vector3.Distance(members[i].transform.position, members[j].transform.position);
                        validPairs++;
                    }
                }
            }
            
            if (validPairs == 0) return 0f;
            
            float averageDistance = totalDistance / validPairs;
            return Mathf.Clamp01(1f - (averageDistance / communicationRange));
        }
    }

    /// <summary>
    /// Global NPC manager for group behavior and alert system.
    /// </summary>
    public class NPCManager : MonoBehaviour
    {
        [Header("Global Settings")]
        [SerializeField] private float globalAlertRange = 30f;
        [SerializeField] private bool enableGlobalAlerts = true;
        [SerializeField] private bool enableGroupBehavior = true;
        [SerializeField] private float alertUpdateInterval = 0.2f;
        
        [Header("Group Configuration")]
        [SerializeField] private List<NPCGroup> npcGroups = new List<NPCGroup>();
        [SerializeField] private bool autoAssignGroups = true;
        [SerializeField] private float groupAssignmentRange = 15f;
        
        [Header("Debug")]
        [SerializeField] private bool drawDebugGizmos = true;
        [SerializeField] private bool showGroupConnections = true;

        // Singleton pattern
        public static NPCManager Instance { get; private set; }

        // Runtime data
        private List<NPCController> allNPCs = new List<NPCController>();
        private List<AlertData> globalAlerts = new List<AlertData>();
        private float lastAlertUpdate;

        // Properties
        public List<NPCController> AllNPCs => allNPCs;
        public List<NPCGroup> NPCGroups => npcGroups;
        public List<AlertData> GlobalAlerts => globalAlerts;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeNPCManager();
        }

        private void Update()
        {
            UpdateAlertSystem();
            UpdateGroups();
        }

        /// <summary>
        /// Initializes the NPC manager.
        /// </summary>
        private void InitializeNPCManager()
        {
            // Find all existing NPCs
            FindAllNPCs();
            
            // Auto-assign groups if enabled
            if (autoAssignGroups)
            {
                AutoAssignNPCsToGroups();
            }

            Debug.Log($"NPC Manager initialized with {allNPCs.Count} NPCs and {npcGroups.Count} groups");
        }

        /// <summary>
        /// Finds all NPCs in the scene.
        /// </summary>
        private void FindAllNPCs()
        {
            allNPCs.Clear();
            NPCController[] npcs = FindObjectsOfType<NPCController>();
            allNPCs.AddRange(npcs);
        }

        /// <summary>
        /// Automatically assigns NPCs to groups based on proximity and type.
        /// </summary>
        private void AutoAssignNPCsToGroups()
        {
            if (!autoAssignGroups) return;

            // Clear existing groups
            foreach (var group in npcGroups)
            {
                group.members.Clear();
            }
            npcGroups.Clear();

            // Group NPCs by type and proximity
            var npcsByType = allNPCs.Where(npc => npc != null).GroupBy(npc => npc.Type);

            foreach (var typeGroup in npcsByType)
            {
                List<NPCController> unassignedNPCs = new List<NPCController>(typeGroup);
                
                while (unassignedNPCs.Count > 0)
                {
                    NPCController leader = unassignedNPCs[0];
                    unassignedNPCs.RemoveAt(0);
                    
                    NPCGroup newGroup = new NPCGroup
                    {
                        groupName = $"{typeGroup.Key}_Group_{npcGroups.Count + 1}",
                        groupType = typeGroup.Key,
                        communicationRange = globalAlertRange * 0.7f
                    };
                    
                    newGroup.AddMember(leader);
                    
                    // Find nearby NPCs of same type
                    for (int i = unassignedNPCs.Count - 1; i >= 0; i--)
                    {
                        NPCController npc = unassignedNPCs[i];
                        float distance = Vector3.Distance(leader.transform.position, npc.transform.position);
                        
                        if (distance <= groupAssignmentRange)
                        {
                            newGroup.AddMember(npc);
                            unassignedNPCs.RemoveAt(i);
                        }
                    }
                    
                    npcGroups.Add(newGroup);
                }
            }
        }

        /// <summary>
        /// Updates the alert system.
        /// </summary>
        private void UpdateAlertSystem()
        {
            if (!enableGlobalAlerts || Time.time - lastAlertUpdate < alertUpdateInterval)
                return;

            lastAlertUpdate = Time.time;
            ProcessGlobalAlerts();
        }

        /// <summary>
        /// Processes global alerts and propagates them to NPCs.
        /// </summary>
        private void ProcessGlobalAlerts()
        {
            // Clean old alerts
            float currentTime = Time.time;
            globalAlerts.RemoveAll(alert => currentTime - alert.timestamp > 60f);

            // Process each alert
            foreach (var alert in globalAlerts)
            {
                PropagateAlert(alert);
            }
        }

        /// <summary>
        /// Propagates alert to nearby NPCs.
        /// </summary>
        private void PropagateAlert(AlertData alert)
        {
            foreach (var npc in allNPCs)
            {
                if (npc == null || npc.CurrentState == NPCState.Dead)
                    continue;

                float distance = Vector3.Distance(npc.transform.position, alert.alertPosition);
                if (distance <= globalAlertRange)
                {
                    NotifyNPCOfAlert(npc, alert);
                }
            }
        }

        /// <summary>
        /// Notifies individual NPC about alert.
        /// </summary>
        private void NotifyNPCOfAlert(NPCController npc, AlertData alert)
        {
            switch (alert.alertType)
            {
                case AlertType.VisualSighting:
                    if (Vector3.Distance(npc.transform.position, alert.alertPosition) <= npc.Parameters.perceptionRange)
                    {
                        npc.SetState(NPCState.Alert);
                    }
                    break;
                    
                case AlertType.SoundHeard:
                    npc.RegisterSound(alert.alertPosition, alert.alertLevel / 50f);
                    break;
                    
                case AlertType.AttackDetected:
                case AlertType.AlarmTriggered:
                    npc.SetState(NPCState.Alert);
                    npc.Navigation.SetTargetPosition(alert.alertPosition);
                    break;
            }
        }

        /// <summary>
        /// Updates all NPC groups.
        /// </summary>
        private void UpdateGroups()
        {
            if (!enableGroupBehavior) return;

            foreach (var group in npcGroups)
            {
                group.UpdateGroup();
            }
        }

        /// <summary>
        /// Registers an NPC with the manager.
        /// </summary>
        public void RegisterNPC(NPCController npc)
        {
            if (npc != null && !allNPCs.Contains(npc))
            {
                allNPCs.Add(npc);
                
                // Auto-assign to group if enabled
                if (autoAssignGroups)
                {
                    AssignNPCToGroup(npc);
                }
            }
        }

        /// <summary>
        /// Unregisters an NPC from the manager.
        /// </summary>
        public void UnregisterNPC(NPCController npc)
        {
            allNPCs.Remove(npc);
            
            // Remove from all groups
            foreach (var group in npcGroups)
            {
                group.RemoveMember(npc);
            }
        }

        /// <summary>
        /// Assigns NPC to appropriate group.
        /// </summary>
        private void AssignNPCToGroup(NPCController npc)
        {
            // Find existing group of same type within range
            NPCGroup suitableGroup = npcGroups.FirstOrDefault(g => 
                g.groupType == npc.Type && 
                g.members.Any(m => m != null && Vector3.Distance(m.transform.position, npc.transform.position) <= groupAssignmentRange)
            );

            if (suitableGroup != null)
            {
                suitableGroup.AddMember(npc);
            }
            else
            {
                // Create new group
                NPCGroup newGroup = new NPCGroup
                {
                    groupName = $"{npc.Type}_Group_{npcGroups.Count + 1}",
                    groupType = npc.Type,
                    communicationRange = globalAlertRange * 0.7f
                };
                newGroup.AddMember(npc);
                npcGroups.Add(newGroup);
            }
        }

        /// <summary>
        /// Creates global alert.
        /// </summary>
        public void CreateGlobalAlert(Vector3 position, GameObject source, float level, AlertType type)
        {
            AlertData alert = new AlertData
            {
                alertPosition = position,
                alertSource = source,
                alertLevel = level,
                alertType = type,
                timestamp = Time.time,
                alertingNPCId = source != null ? source.GetInstanceID() : 0
            };

            globalAlerts.Add(alert);

            // Also notify relevant groups
            NotifyNearbyGroups(alert);
        }

        /// <summary>
        /// Notifies nearby groups about alert.
        /// </summary>
        private void NotifyNearbyGroups(AlertData alert)
        {
            foreach (var group in npcGroups)
            {
                if (group.members.Any(m => m != null && Vector3.Distance(m.transform.position, alert.alertPosition) <= group.communicationRange))
                {
                    group.BroadcastAlert(alert);
                }
            }
        }

        /// <summary>
        /// Gets NPCs near position.
        /// </summary>
        public List<NPCController> GetNPCsNearPosition(Vector3 position, float radius)
        {
            return allNPCs.Where(npc => npc != null && Vector3.Distance(npc.transform.position, position) <= radius).ToList();
        }

        /// <summary>
        /// Gets NPCs of specific type.
        /// </summary>
        public List<NPCController> GetNPCsOfType(NPCType npcType)
        {
            return allNPCs.Where(npc => npc != null && npc.Type == npcType).ToList();
        }

        private void OnDrawGizmos()
        {
            if (!drawDebugGizmos) return;

            // Draw group connections
            if (showGroupConnections)
            {
                foreach (var group in npcGroups)
                {
                    Gizmos.color = GetGroupColor(group.groupType);
                    
                    for (int i = 0; i < group.members.Count; i++)
                    {
                        for (int j = i + 1; j < group.members.Count; j++)
                        {
                            if (group.members[i] != null && group.members[j] != null)
                            {
                                Vector3 pos1 = group.members[i].transform.position;
                                Vector3 pos2 = group.members[j].transform.position;
                                
                                if (Vector3.Distance(pos1, pos2) <= group.communicationRange)
                                {
                                    Gizmos.DrawLine(pos1 + Vector3.up * 0.1f, pos2 + Vector3.up * 0.1f);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets color for NPC type visualization.
        /// </summary>
        private Color GetGroupColor(NPCType npcType)
        {
            switch (npcType)
            {
                case NPCType.Guard: return Color.red;
                case NPCType.Scientist: return Color.blue;
                case NPCType.Civilian: return Color.green;
                case NPCType.Beast: return Color.magenta;
                default: return Color.white;
            }
        }
    }
}