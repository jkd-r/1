using UnityEngine;

namespace ProtocolEMR.Combat.Core
{
    public class WeaponPickup : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] private WeaponType weaponType;
        [SerializeField] private string weaponDisplayName;

        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        [SerializeField] private bool showPrompt = true;

        [Header("Visual Settings")]
        [SerializeField] private GameObject weaponModel;
        [SerializeField] private bool rotateWeapon = true;
        [SerializeField] private float rotationSpeed = 50f;
        [SerializeField] private bool bobWeapon = true;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.2f;

        [Header("UI")]
        [SerializeField] private GameObject interactionPromptUI;
        [SerializeField] private UnityEngine.UI.Text promptText;

        private Transform playerTransform;
        private bool isPlayerInRange = false;
        private bool hasBeenPickedUp = false;
        private Vector3 startPosition;

        void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;

            startPosition = transform.position;

            if (interactionPromptUI != null)
                interactionPromptUI.SetActive(false);

            if (promptText != null)
                promptText.text = $"Press [E] to pick up {weaponDisplayName}";
        }

        void Update()
        {
            if (hasBeenPickedUp)
                return;

            if (playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                isPlayerInRange = distance <= interactionRange;

                if (showPrompt && interactionPromptUI != null)
                    interactionPromptUI.SetActive(isPlayerInRange);
            }

            if (isPlayerInRange && Input.GetKeyDown(interactionKey))
            {
                TryPickup();
            }

            AnimateWeapon();
        }

        private void AnimateWeapon()
        {
            if (rotateWeapon && weaponModel != null)
            {
                weaponModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }

            if (bobWeapon)
            {
                float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
        }

        private void TryPickup()
        {
            if (playerTransform == null)
                return;

            WeaponManager weaponManager = playerTransform.GetComponent<WeaponManager>();
            if (weaponManager != null)
            {
                bool success = weaponManager.PickupWeapon(weaponType);
                if (success)
                {
                    hasBeenPickedUp = true;

                    if (interactionPromptUI != null)
                        interactionPromptUI.SetActive(false);

                    Destroy(gameObject);
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
