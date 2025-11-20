using UnityEngine;

namespace ProtocolEMR.Core.Player
{
    /// <summary>
    /// Handles surface-specific movement physics and effects.
    /// Modifies movement speed, acceleration, and other movement properties based on terrain type.
    /// Supports mud, ice, sand, metal, wood, and water surfaces with unique movement characteristics.
    /// </summary>
    public class SurfacePhysics : MonoBehaviour
    {
        [Header("Mud Settings")]
        [SerializeField] private float mudSpeedMultiplier = 0.6f;
        [SerializeField] private float mudAccelerationMultiplier = 0.5f;
        [SerializeField] private float mudDragMultiplier = 1.5f;

        [Header("Ice Settings")]
        [SerializeField] private float iceSpeedMultiplier = 1.3f;
        [SerializeField] private float iceAccelerationMultiplier = 0.3f;
        [SerializeField] private float iceDragMultiplier = 0.1f;
        [SerializeField] private float iceSlipperyness = 0.2f;

        [Header("Sand Settings")]
        [SerializeField] private float sandSpeedMultiplier = 0.75f;
        [SerializeField] private float sandAccelerationMultiplier = 0.6f;
        [SerializeField] private float sandDragMultiplier = 1.2f;

        [Header("Metal Settings")]
        [SerializeField] private float metalSpeedMultiplier = 1.1f;
        [SerializeField] private float metalAccelerationMultiplier = 1.1f;
        [SerializeField] private float metalDragMultiplier = 0.9f;

        [Header("Wood Settings")]
        [SerializeField] private float woodSpeedMultiplier = 0.95f;
        [SerializeField] private float woodAccelerationMultiplier = 0.95f;
        [SerializeField] private float woodDragMultiplier = 1.0f;

        [Header("Water Settings")]
        [SerializeField] private float waterSpeedMultiplier = 0.5f;
        [SerializeField] private float waterAccelerationMultiplier = 0.4f;
        [SerializeField] private float waterDragMultiplier = 2.0f;
        [SerializeField] private float waterJumpMultiplier = 0.7f;

        private GroundDetection groundDetection;

        private void Awake()
        {
            groundDetection = GetComponent<GroundDetection>();
            if (groundDetection == null)
            {
                Debug.LogError("SurfacePhysics requires a GroundDetection component");
            }
        }

        /// <summary>
        /// Gets the speed multiplier for the current surface.
        /// Values > 1.0 increase speed, < 1.0 decrease speed.
        /// </summary>
        public float GetSpeedMultiplier()
        {
            if (groundDetection == null || !groundDetection.IsGrounded)
                return 1.0f;

            return GetSurfaceMultiplier(groundDetection.CurrentSurface, MultiplierType.Speed);
        }

        /// <summary>
        /// Gets the acceleration multiplier for the current surface.
        /// Lower values mean slower acceleration.
        /// </summary>
        public float GetAccelerationMultiplier()
        {
            if (groundDetection == null || !groundDetection.IsGrounded)
                return 1.0f;

            return GetSurfaceMultiplier(groundDetection.CurrentSurface, MultiplierType.Acceleration);
        }

        /// <summary>
        /// Gets the drag/friction multiplier for the current surface.
        /// Higher values mean more friction/drag.
        /// </summary>
        public float GetDragMultiplier()
        {
            if (groundDetection == null || !groundDetection.IsGrounded)
                return 1.0f;

            return GetSurfaceMultiplier(groundDetection.CurrentSurface, MultiplierType.Drag);
        }

        /// <summary>
        /// Gets jump height multiplier for the current surface.
        /// Typically used to reduce jump height in water or mud.
        /// </summary>
        public float GetJumpMultiplier()
        {
            if (groundDetection == null || !groundDetection.IsGrounded)
                return 1.0f;

            return groundDetection.CurrentSurface switch
            {
                GroundDetection.Surface.Water => waterJumpMultiplier,
                GroundDetection.Surface.Mud => 0.8f,
                GroundDetection.Surface.Sand => 0.9f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Gets friction/traction for steering on current surface.
        /// Higher values mean better traction for direction changes.
        /// </summary>
        public float GetTraction()
        {
            if (groundDetection == null || !groundDetection.IsGrounded)
                return 1.0f;

            return groundDetection.CurrentSurface switch
            {
                GroundDetection.Surface.Mud => 0.7f,
                GroundDetection.Surface.Ice => 0.2f,
                GroundDetection.Surface.Sand => 0.65f,
                GroundDetection.Surface.Metal => 1.1f,
                GroundDetection.Surface.Wood => 0.95f,
                GroundDetection.Surface.Water => 0.5f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Applies surface-specific damping to velocity.
        /// Used to simulate friction and surface resistance.
        /// </summary>
        public Vector3 ApplySurfaceDamping(Vector3 velocity, float deltaTime)
        {
            if (groundDetection == null || !groundDetection.IsGrounded)
                return velocity;

            float dragMultiplier = GetDragMultiplier();
            float dampingFactor = Mathf.Clamp01(dragMultiplier * deltaTime * 5f);

            velocity.x = Mathf.Lerp(velocity.x, 0, dampingFactor);
            velocity.z = Mathf.Lerp(velocity.z, 0, dampingFactor);

            return velocity;
        }

        /// <summary>
        /// Gets the sound effect category for the current surface.
        /// Useful for playing appropriate footstep sounds.
        /// </summary>
        public string GetSurfaceSoundCategory()
        {
            if (groundDetection == null || !groundDetection.IsGrounded)
                return "Concrete";

            return groundDetection.CurrentSurface switch
            {
                GroundDetection.Surface.Mud => "Mud",
                GroundDetection.Surface.Ice => "Ice",
                GroundDetection.Surface.Sand => "Sand",
                GroundDetection.Surface.Metal => "Metal",
                GroundDetection.Surface.Wood => "Wood",
                GroundDetection.Surface.Water => "Water",
                _ => "Concrete"
            };
        }

        private float GetSurfaceMultiplier(GroundDetection.Surface surface, MultiplierType type)
        {
            return (surface, type) switch
            {
                (GroundDetection.Surface.Mud, MultiplierType.Speed) => mudSpeedMultiplier,
                (GroundDetection.Surface.Mud, MultiplierType.Acceleration) => mudAccelerationMultiplier,
                (GroundDetection.Surface.Mud, MultiplierType.Drag) => mudDragMultiplier,

                (GroundDetection.Surface.Ice, MultiplierType.Speed) => iceSpeedMultiplier,
                (GroundDetection.Surface.Ice, MultiplierType.Acceleration) => iceAccelerationMultiplier,
                (GroundDetection.Surface.Ice, MultiplierType.Drag) => iceDragMultiplier,

                (GroundDetection.Surface.Sand, MultiplierType.Speed) => sandSpeedMultiplier,
                (GroundDetection.Surface.Sand, MultiplierType.Acceleration) => sandAccelerationMultiplier,
                (GroundDetection.Surface.Sand, MultiplierType.Drag) => sandDragMultiplier,

                (GroundDetection.Surface.Metal, MultiplierType.Speed) => metalSpeedMultiplier,
                (GroundDetection.Surface.Metal, MultiplierType.Acceleration) => metalAccelerationMultiplier,
                (GroundDetection.Surface.Metal, MultiplierType.Drag) => metalDragMultiplier,

                (GroundDetection.Surface.Wood, MultiplierType.Speed) => woodSpeedMultiplier,
                (GroundDetection.Surface.Wood, MultiplierType.Acceleration) => woodAccelerationMultiplier,
                (GroundDetection.Surface.Wood, MultiplierType.Drag) => woodDragMultiplier,

                (GroundDetection.Surface.Water, MultiplierType.Speed) => waterSpeedMultiplier,
                (GroundDetection.Surface.Water, MultiplierType.Acceleration) => waterAccelerationMultiplier,
                (GroundDetection.Surface.Water, MultiplierType.Drag) => waterDragMultiplier,

                _ => 1.0f
            };
        }

        private enum MultiplierType
        {
            Speed,
            Acceleration,
            Drag
        }
    }
}
