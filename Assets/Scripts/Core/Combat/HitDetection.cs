using UnityEngine;
using System.Collections.Generic;

namespace ProtocolEMR.Core.Combat
{
    /// <summary>
    /// Handles hit detection for melee and ranged attacks using raycasts, overlaps, and collision detection.
    /// Tracks hit targets to prevent duplicate hits on single attacks.
    /// </summary>
    public class HitDetection : MonoBehaviour
    {
        [Header("Hit Detection Settings")]
        [SerializeField] private float meleeHitBoxSize = 1.5f;
        [SerializeField] private float raycastDistance = 100f;
        [SerializeField] private LayerMask targetLayer = -1;

        [Header("Debug")]
        [SerializeField] private bool debugDrawHits = false;
        [SerializeField] private float debugDrawDuration = 0.5f;

        private HashSet<Collider> hitTargetsThisAttack = new HashSet<Collider>();

        /// <summary>
        /// Performs a melee hit detection using overlap sphere.
        /// </summary>
        /// <param name="position">Center of hit detection sphere</param>
        /// <param name="radius">Radius of detection</param>
        /// <param name="targetLayers">Layers to check for hits</param>
        /// <returns>Array of colliders hit</returns>
        public Collider[] DetectMeleeHits(Vector3 position, float radius, LayerMask targetLayers)
        {
            Collider[] hits = Physics.OverlapSphere(position, radius, targetLayers);

            if (debugDrawHits)
            {
                Debug.DrawLine(position, position + Vector3.up * 0.5f, Color.red, debugDrawDuration);
            }

            return hits;
        }

        /// <summary>
        /// Performs a raycast hit detection.
        /// </summary>
        /// <param name="origin">Origin of raycast</param>
        /// <param name="direction">Direction of raycast</param>
        /// <param name="maxDistance">Maximum distance of raycast</param>
        /// <param name="targetLayers">Layers to check for hits</param>
        /// <param name="hitInfo">Output hit information</param>
        /// <returns>True if hit detected</returns>
        public bool DetectRaycastHit(Vector3 origin, Vector3 direction, float maxDistance, LayerMask targetLayers, out RaycastHit hitInfo)
        {
            bool hit = Physics.Raycast(origin, direction, out hitInfo, maxDistance, targetLayers);

            if (debugDrawHits && hit)
            {
                Debug.DrawLine(origin, hitInfo.point, Color.green, debugDrawDuration);
            }

            return hit;
        }

        /// <summary>
        /// Performs multiple raycasts in a cone pattern for shotgun-like effects.
        /// </summary>
        /// <param name="origin">Origin of raycasts</param>
        /// <param name="direction">Center direction of cone</param>
        /// <param name="spreadAngle">Angle of spread in degrees</param>
        /// <param name="rayCount">Number of rays to cast</param>
        /// <param name="maxDistance">Maximum distance of raycasts</param>
        /// <param name="targetLayers">Layers to check for hits</param>
        /// <returns>Array of hit information</returns>
        public RaycastHit[] DetectConeHits(Vector3 origin, Vector3 direction, float spreadAngle, int rayCount, float maxDistance, LayerMask targetLayers)
        {
            List<RaycastHit> hits = new List<RaycastHit>();

            for (int i = 0; i < rayCount; i++)
            {
                float angle = (i / (float)(rayCount - 1)) * spreadAngle - (spreadAngle * 0.5f);
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                Vector3 rayDirection = rotation * direction;

                if (Physics.Raycast(origin, rayDirection, out RaycastHit hitInfo, maxDistance, targetLayers))
                {
                    hits.Add(hitInfo);

                    if (debugDrawHits)
                    {
                        Debug.DrawLine(origin, hitInfo.point, Color.yellow, debugDrawDuration);
                    }
                }
            }

            return hits.ToArray();
        }

        /// <summary>
        /// Resets the hit targets tracking for a new attack.
        /// </summary>
        public void ResetHitTargets()
        {
            hitTargetsThisAttack.Clear();
        }

        /// <summary>
        /// Checks if a collider has already been hit in this attack.
        /// </summary>
        /// <param name="collider">Collider to check</param>
        /// <returns>True if already hit</returns>
        public bool HasAlreadyHit(Collider collider)
        {
            return hitTargetsThisAttack.Contains(collider);
        }

        /// <summary>
        /// Registers a hit on a collider.
        /// </summary>
        /// <param name="collider">Collider that was hit</param>
        public void RegisterHit(Collider collider)
        {
            hitTargetsThisAttack.Add(collider);
        }

        /// <summary>
        /// Performs box cast hit detection.
        /// </summary>
        /// <param name="center">Center of detection box</param>
        /// <param name="halfExtents">Half extents of detection box</param>
        /// <param name="direction">Direction of detection</param>
        /// <param name="rotation">Rotation of detection box</param>
        /// <param name="maxDistance">Maximum distance to detect</param>
        /// <param name="targetLayers">Layers to check for hits</param>
        /// <returns>Array of colliders hit</returns>
        public Collider[] DetectBoxHits(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion rotation, float maxDistance, LayerMask targetLayers)
        {
            RaycastHit[] boxHits = Physics.BoxCastAll(center, halfExtents, direction, rotation, maxDistance, targetLayers);

            if (debugDrawHits && boxHits.Length > 0)
            {
                Debug.DrawLine(center, center + direction * maxDistance, Color.magenta, debugDrawDuration);
            }

            Collider[] colliders = new Collider[boxHits.Length];
            for (int i = 0; i < boxHits.Length; i++)
            {
                colliders[i] = boxHits[i].collider;
            }

            return colliders;
        }

        /// <summary>
        /// Performs capsule cast hit detection.
        /// </summary>
        /// <param name="point1">Start point of capsule</param>
        /// <param name="point2">End point of capsule</param>
        /// <param name="radius">Radius of capsule</param>
        /// <param name="direction">Direction of detection</param>
        /// <param name="maxDistance">Maximum distance to detect</param>
        /// <param name="targetLayers">Layers to check for hits</param>
        /// <returns>Array of colliders hit</returns>
        public Collider[] DetectCapsuleHits(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance, LayerMask targetLayers)
        {
            RaycastHit[] capsuleHits = Physics.CapsuleCastAll(point1, point2, radius, direction, maxDistance, targetLayers);

            if (debugDrawHits && capsuleHits.Length > 0)
            {
                Debug.DrawLine(point1, point1 + direction * maxDistance, Color.cyan, debugDrawDuration);
            }

            Collider[] colliders = new Collider[capsuleHits.Length];
            for (int i = 0; i < capsuleHits.Length; i++)
            {
                colliders[i] = capsuleHits[i].collider;
            }

            return colliders;
        }

        /// <summary>
        /// Sets debug drawing state.
        /// </summary>
        /// <param name="enabled">Whether to draw debug visuals</param>
        public void SetDebugDraw(bool enabled)
        {
            debugDrawHits = enabled;
        }
    }
}
