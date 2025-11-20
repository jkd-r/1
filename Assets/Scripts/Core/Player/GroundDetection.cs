using UnityEngine;

namespace ProtocolEMR.Core.Player
{
    /// <summary>
    /// Detects surface properties and terrain information for the player.
    /// Provides surface detection, raycasting, and ground state management.
    /// Works with CharacterController to determine grounded state and surface properties.
    /// </summary>
    public class GroundDetection : MonoBehaviour
    {
        [Header("Ground Detection")]
        [SerializeField] private float raycastDistance = 0.2f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float skinWidth = 0.01f;

        [Header("Surface Detection")]
        [SerializeField] private float checkRadius = 0.3f;
        [SerializeField] private int raycastCount = 4;

        private CharacterController characterController;
        private RaycastHit groundHit;
        private bool isGrounded;
        private Surface currentSurface = Surface.Normal;
        private Vector3 groundNormal = Vector3.up;

        public bool IsGrounded => isGrounded;
        public Surface CurrentSurface => currentSurface;
        public Vector3 GroundNormal => groundNormal;
        public RaycastHit GroundHit => groundHit;

        public enum Surface
        {
            Normal = 0,
            Mud = 1,
            Ice = 2,
            Sand = 3,
            Metal = 4,
            Wood = 5,
            Water = 6
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                Debug.LogError("GroundDetection requires a CharacterController component");
            }
        }

        /// <summary>
        /// Update ground detection state and surface properties.
        /// Should be called each frame during movement calculations.
        /// </summary>
        public void UpdateGroundDetection()
        {
            if (characterController == null)
                return;

            DetectGround();
            DetectSurface();
        }

        private void DetectGround()
        {
            Vector3 rayStart = transform.position + Vector3.up * (characterController.center.y - characterController.height / 2f);
            float rayDistance = raycastDistance + skinWidth;

            isGrounded = Physics.Raycast(
                rayStart,
                Vector3.down,
                out groundHit,
                rayDistance,
                groundLayer,
                QueryTriggerInteraction.Ignore
            );

            if (isGrounded)
            {
                groundNormal = groundHit.normal;
            }
            else
            {
                groundNormal = Vector3.up;
                // Also check if CharacterController reports grounded
                isGrounded = characterController.isGrounded;
            }
        }

        private void DetectSurface()
        {
            currentSurface = Surface.Normal;

            if (!isGrounded)
                return;

            // Check material or tag for surface type
            if (groundHit.collider != null)
            {
                string tag = groundHit.collider.tag;
                currentSurface = GetSurfaceFromTag(tag);

                // If no tag match, try material name
                if (currentSurface == Surface.Normal && groundHit.collider.GetComponent<Renderer>() != null)
                {
                    Material mat = groundHit.collider.GetComponent<Renderer>().material;
                    if (mat != null)
                    {
                        currentSurface = GetSurfaceFromMaterial(mat.name);
                    }
                }
            }
        }

        /// <summary>
        /// Gets surface type from collision tag.
        /// Tags should be: "Surface_Mud", "Surface_Ice", etc.
        /// </summary>
        private Surface GetSurfaceFromTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return Surface.Normal;

            return tag switch
            {
                "Surface_Mud" => Surface.Mud,
                "Surface_Ice" => Surface.Ice,
                "Surface_Sand" => Surface.Sand,
                "Surface_Metal" => Surface.Metal,
                "Surface_Wood" => Surface.Wood,
                "Surface_Water" => Surface.Water,
                _ => Surface.Normal
            };
        }

        /// <summary>
        /// Gets surface type from material name.
        /// Material names should contain "Mud", "Ice", etc.
        /// </summary>
        private Surface GetSurfaceFromMaterial(string materialName)
        {
            if (string.IsNullOrEmpty(materialName))
                return Surface.Normal;

            string lower = materialName.ToLower();

            if (lower.Contains("mud"))
                return Surface.Mud;
            if (lower.Contains("ice"))
                return Surface.Ice;
            if (lower.Contains("sand"))
                return Surface.Sand;
            if (lower.Contains("metal"))
                return Surface.Metal;
            if (lower.Contains("wood"))
                return Surface.Wood;
            if (lower.Contains("water"))
                return Surface.Water;

            return Surface.Normal;
        }

        /// <summary>
        /// Gets the steepness of the ground surface in degrees.
        /// </summary>
        public float GetGroundSteepness()
        {
            if (!isGrounded)
                return 0f;

            return Vector3.Angle(groundNormal, Vector3.up);
        }

        /// <summary>
        /// Checks if the ground slope is too steep to stand on.
        /// </summary>
        public bool IsGroundTooSteep(float maxSlopeAngle = 45f)
        {
            return GetGroundSteepness() > maxSlopeAngle;
        }

        /// <summary>
        /// Gets the velocity of the ground surface (for moving platforms).
        /// </summary>
        public Vector3 GetGroundVelocity()
        {
            if (!isGrounded || groundHit.collider == null)
                return Vector3.zero;

            Rigidbody rb = groundHit.collider.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                return rb.velocity;
            }

            return Vector3.zero;
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
                return;

            CharacterController cc = GetComponent<CharacterController>();
            if (cc == null)
                return;

            Vector3 rayStart = transform.position + Vector3.up * (cc.center.y - cc.height / 2f);
            float rayDistance = raycastDistance + skinWidth;

            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(rayStart, rayStart + Vector3.down * rayDistance);

            if (isGrounded && groundHit.collider != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(groundHit.point, groundNormal * 0.5f);
            }
        }
    }
}
