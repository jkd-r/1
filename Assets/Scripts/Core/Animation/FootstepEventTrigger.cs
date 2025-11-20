using UnityEngine;
using ProtocolEMR.Core.Audio;

namespace ProtocolEMR.Core.Animation
{
    /// <summary>
    /// This script should be placed on the player's animation model and called from animation events.
    /// It triggers footstep audio at the correct animation frames during walk/run/sprint cycles.
    /// Animation event should be placed at the frame where the foot contacts the ground (approximately 40-50% into step cycle).
    /// </summary>
    public class FootstepEventTrigger : MonoBehaviour
    {
        [SerializeField] private bool isSprinting = false;

        public void PlayFootstepSound(int footIndex)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayFootstep(transform.position, isSprinting);
            }
        }

        public void SetSprinting(bool sprinting)
        {
            isSprinting = sprinting;
        }
    }
}
