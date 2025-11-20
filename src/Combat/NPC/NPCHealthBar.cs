using UnityEngine;
using UnityEngine.UI;

namespace ProtocolEMR.Combat.Core
{
    public class NPCHealthBar : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image healthFillImage;
        [SerializeField] private Image healthBackgroundImage;
        [SerializeField] private Canvas canvas;

        [Header("Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0, 2.5f, 0);
        [SerializeField] private float hideDistance = 20f;
        [SerializeField] private bool alwaysVisible = false;

        [Header("Colors")]
        [SerializeField] private Color fullHealthColor = Color.green;
        [SerializeField] private Color midHealthColor = Color.yellow;
        [SerializeField] private Color lowHealthColor = Color.red;

        private float maxHealth;
        private float currentHealth;
        private Camera mainCamera;

        void Awake()
        {
            mainCamera = Camera.main;

            if (canvas == null)
                canvas = GetComponent<Canvas>();

            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = mainCamera;
            }
        }

        void Update()
        {
            if (mainCamera != null)
            {
                transform.LookAt(mainCamera.transform);
                transform.Rotate(0, 180, 0);

                if (!alwaysVisible)
                {
                    float distance = Vector3.Distance(transform.position, mainCamera.transform.position);
                    canvas.enabled = distance <= hideDistance;
                }
            }
        }

        public void Initialize(float maxHealthValue)
        {
            maxHealth = maxHealthValue;
            currentHealth = maxHealthValue;
            UpdateHealthBar();
        }

        public void UpdateHealth(float newHealth, float maxHealthValue)
        {
            currentHealth = newHealth;
            maxHealth = maxHealthValue;
            UpdateHealthBar();
        }

        private void UpdateHealthBar()
        {
            if (healthFillImage == null)
                return;

            float healthPercentage = currentHealth / maxHealth;
            healthFillImage.fillAmount = healthPercentage;

            healthFillImage.color = GetHealthColor(healthPercentage);
        }

        private Color GetHealthColor(float percentage)
        {
            if (percentage > 0.6f)
                return Color.Lerp(midHealthColor, fullHealthColor, (percentage - 0.6f) / 0.4f);
            else if (percentage > 0.3f)
                return Color.Lerp(lowHealthColor, midHealthColor, (percentage - 0.3f) / 0.3f);
            else
                return lowHealthColor;
        }

        public void SetAlwaysVisible(bool visible)
        {
            alwaysVisible = visible;
            if (canvas != null)
                canvas.enabled = visible;
        }

        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }
    }
}
