using UnityEngine;
using UnityEngine.UI;
using System;
using ProtocolEMR.Core.Inventory;

namespace ProtocolEMR.UI
{
    public class QuickSlotUI : MonoBehaviour
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private Text slotNumberText;
        [SerializeField] private Image slotBackground;
        [SerializeField] private Color emptyColor = Color.gray;
        [SerializeField] private Color filledColor = Color.white;

        private Button button;
        private ItemData item;
        private int slotIndex;
        private Action<QuickSlotUI, int> onClicked;

        public ItemData Item => item;
        public int SlotIndex => slotIndex;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnClick);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClick);
            }
        }

        public void Initialize(int index, Action<QuickSlotUI, int> onClickedCallback)
        {
            slotIndex = index;
            onClicked = onClickedCallback;

            if (slotNumberText != null)
            {
                slotNumberText.text = (index + 1).ToString();
            }

            Clear();
        }

        public void SetItem(ItemData itemData)
        {
            item = itemData;

            if (item != null)
            {
                if (itemIcon != null && !string.IsNullOrEmpty(item.iconPath))
                {
                    itemIcon.sprite = Resources.Load<Sprite>(item.iconPath);
                    itemIcon.enabled = true;
                }

                if (slotBackground != null)
                {
                    slotBackground.color = filledColor;
                }
            }
            else
            {
                Clear();
            }
        }

        public void Clear()
        {
            item = null;

            if (itemIcon != null)
            {
                itemIcon.sprite = null;
                itemIcon.enabled = false;
            }

            if (slotBackground != null)
            {
                slotBackground.color = emptyColor;
            }
        }

        private void OnClick()
        {
            onClicked?.Invoke(this, slotIndex);
        }
    }
}
