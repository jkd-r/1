using UnityEngine;
using UnityEngine.AI;

namespace ProtocolEMR.Core.AI
{
    /// <summary>
    /// Perception system for NPCs - handles sight, hearing, and awareness.
    /// </summary>
    [System.Serializable]
    public class NPCPerception
    {
        private NPCController npc;
        private PerceptionData perceptionData;
        
        [Header("Perception Settings")]
        [SerializeField] private LayerMask visionLayerMask = -1;
        [SerializeField] private LayerMask soundLayerMask = -1;
        [SerializeField] private float alertnessDecayRate = 0.5f;
        [SerializeField] private float memoryDuration = 10.0f;
        
        public PerceptionData Data => perceptionData;
        public bool CanSeePlayer => perceptionData.canSeePlayer;
        public bool CanHearPlayer => perceptionData.canHearPlayer;
        public Vector3 LastKnownPlayerPosition => perceptionData.lastKnownPlayerPosition;
        public float Alertness => perceptionData.alertness;

        public NPCPerception(NPCController controller)
        {
            npc = controller;
            perceptionData = new PerceptionData();
        }

        /// <summary>
        /// Updates perception systems including sight and hearing.
        /// </summary>
        public void UpdatePerception()
        {
            UpdateVision();
            UpdateHearing();
            UpdateAlertness();
            UpdateMemory();
        }

        /// <summary>
        /// Updates visual perception - cone of vision and line of sight.
        /// </summary>
        private void UpdateVision()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                perceptionData.canSeePlayer = false;
                return;
            }

            Vector3 playerPosition = player.transform.position;
            Vector3 npcPosition = npc.transform.position;
            Vector3 directionToPlayer = (playerPosition - npcPosition).normalized;
            
            float distanceToPlayer = Vector3.Distance(npcPosition, playerPosition);
            
            // Check if player is within perception range
            if (distanceToPlayer > npc.Parameters.perceptionRange)
            {
                perceptionData.canSeePlayer = false;
                return;
            }

            // Check if player is within field of view
            float angleToPlayer = Vector3.Angle(npc.transform.forward, directionToPlayer);
            if (angleToPlayer > npc.Parameters.fieldOfView * 0.5f)
            {
                perceptionData.canSeePlayer = false;
                return;
            }

            // Check line of sight using raycast
            Vector3 eyePosition = npcPosition + Vector3.up * 1.6f; // NPC eye height
            Vector3 playerHeadPosition = playerPosition + Vector3.up * 1.7f; // Player head height
            
            if (Physics.Linecast(eyePosition, playerHeadPosition, visionLayerMask))
            {
                perceptionData.canSeePlayer = false;
                return;
            }

            // Player is visible!
            perceptionData.canSeePlayer = true;
            perceptionData.lastKnownPlayerPosition = playerPosition;
            perceptionData.timeSinceLastSighting = 0f;
            perceptionData.alertness = Mathf.Min(100f, perceptionData.alertness + 10f);
        }

        /// <summary>
        /// Updates hearing perception - detects footsteps, gunshots, etc.
        /// </summary>
        private void UpdateHearing()
        {
            // Simple implementation - detect player movement sounds
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                perceptionData.canHearPlayer = false;
                return;
            }

            Vector3 playerPosition = player.transform.position;
            Vector3 npcPosition = npc.transform.position;
            float distanceToPlayer = Vector3.Distance(npcPosition, playerPosition);

            // Check if player is within hearing range
            if (distanceToPlayer <= npc.Parameters.hearingRange)
            {
                // Check if player is moving (simplified)
                Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();
                if (playerRigidbody != null && playerRigidbody.velocity.magnitude > 1.0f)
                {
                    perceptionData.canHearPlayer = true;
                    perceptionData.soundSource = playerPosition;
                    perceptionData.timeSinceSound = 0f;
                    perceptionData.alertness = Mathf.Min(100f, perceptionData.alertness + 5f);
                }
            }
            else
            {
                perceptionData.canHearPlayer = false;
            }
        }

        /// <summary>
        /// Updates alertness level over time.
        /// </summary>
        private void UpdateAlertness()
        {
            // Decay alertness if no recent stimuli
            if (!perceptionData.canSeePlayer && !perceptionData.canHearPlayer)
            {
                perceptionData.alertness = Mathf.Max(0f, perceptionData.alertness - alertnessDecayRate * Time.deltaTime);
            }
        }

        /// <summary>
        /// Updates memory of player position.
        /// </summary>
        private void UpdateMemory()
        {
            if (!perceptionData.canSeePlayer)
            {
                perceptionData.timeSinceLastSighting += Time.deltaTime;
                
                // Forget player position after memory duration
                if (perceptionData.timeSinceLastSighting > memoryDuration)
                {
                    perceptionData.lastKnownPlayerPosition = Vector3.zero;
                }
            }
        }

        /// <summary>
        /// Registers a sound at a specific position.
        /// </summary>
        public void RegisterSound(Vector3 soundPosition, float soundIntensity)
        {
            float distance = Vector3.Distance(npc.transform.position, soundPosition);
            float effectiveRange = npc.Parameters.hearingRange * soundIntensity;
            
            if (distance <= effectiveRange)
            {
                perceptionData.soundSource = soundPosition;
                perceptionData.timeSinceSound = 0f;
                perceptionData.alertness = Mathf.Min(100f, perceptionData.alertness + soundIntensity * 10f);
            }
        }

        /// <summary>
        /// Gets the threat level based on perception data.
        /// </summary>
        public float GetThreatLevel()
        {
            float threat = 0f;
            
            if (perceptionData.canSeePlayer)
                threat += 70f;
            
            if (perceptionData.canHearPlayer)
                threat += 30f;
            
            threat += perceptionData.alertness * 0.3f;
            
            return Mathf.Min(100f, threat);
        }

        /// <summary>
        /// Checks if NPC should be alert based on perception.
        /// </summary>
        public bool ShouldBeAlert()
        {
            return perceptionData.alertness > 30f || perceptionData.canSeePlayer || perceptionData.canHearPlayer;
        }

        /// <summary>
        /// Draws debug visualization for perception cone.
        /// </summary>
        public void DrawDebugGizmos()
        {
            if (npc == null) return;

            Vector3 position = npc.transform.position;
            
            // Draw perception range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, npc.Parameters.perceptionRange);
            
            // Draw field of view cone
            Gizmos.color = perceptionData.canSeePlayer ? Color.red : Color.green;
            Vector3 leftDirection = Quaternion.Euler(0, -npc.Parameters.fieldOfView * 0.5f, 0) * npc.transform.forward;
            Vector3 rightDirection = Quaternion.Euler(0, npc.Parameters.fieldOfView * 0.5f, 0) * npc.transform.forward;
            
            Gizmos.DrawLine(position, position + leftDirection * npc.Parameters.perceptionRange);
            Gizmos.DrawLine(position, position + rightDirection * npc.Parameters.perceptionRange);
            
            // Draw lines to last known player position
            if (perceptionData.lastKnownPlayerPosition != Vector3.zero)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(position, perceptionData.lastKnownPlayerPosition);
            }
            
            // Draw sound source
            if (perceptionData.timeSinceSound < 2.0f)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(perceptionData.soundSource, 1.0f);
            }
        }
    }
}