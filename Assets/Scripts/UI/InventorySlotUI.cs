using UnityEngine;
using UnityEngine.UI;
using System;
using ProtocolEMR.Core.Inventory;

namespace ProtocolEMR.UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private Text quantityText;
        [SerializeField] private Image highlightBorder;
        [SerializeField] private Color hoverColor = Color.yellow;
        [SerializeField] private Color normalColor = Color.white;

        private Button button;
        private ItemData item;
        private int slotIndex;
        private Action<InventorySlotUI> onClicked;

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

        public void Initialize(int index, Action<InventorySlotUI> onClickedCallback)
        {
            slotIndex = index;
            onClicked = onClickedCallback;

            if (button != null)
            {
                EventTrigger trigger = gameObject.AddComponent<EventTrigger>();

                EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry();
                pointerEnterEntry.eventID = EventTriggerType.PointerEnter;
                pointerEnterEntry.callback.AddListener((data) => OnPointerEnter());
                trigger.triggers.Add(pointerEnterEntry);

                EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
                pointerExitEntry.eventID = EventTriggerType.PointerExit;
                pointerExitEntry.callback.AddListener((data) => OnPointerExit());
                trigger.triggers.Add(pointerExitEntry);
            }
        }

        public void SetItem(ItemData itemData)
        {
            item = itemData;

            if (item != null)
            {
                if (itemIcon != null && !string.IsNullOrEmpty(item.iconPath))
                {
                    itemIcon.sprite = Resources.Load<Sprite>(item.iconPath);
                    itemIcon.color = normalColor;
                }

                if (quantityText != null)
                {
                    if (item.isStackable && item.quantity > 1)
                    {
                        quantityText.text = item.quantity.ToString();
                        quantityText.enabled = true;
                    }
                    else
                    {
                        quantityText.enabled = false;
                    }
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
            }

            if (quantityText != null)
            {
                quantityText.enabled = false;
            }
        }

        private void OnClick()
        {
            onClicked?.Invoke(this);
        }

        private void OnPointerEnter()
        {
            if (highlightBorder != null && item != null)
            {
                highlightBorder.color = hoverColor;
            }
        }

        private void OnPointerExit()
        {
            if (highlightBorder != null)
            {
                highlightBorder.color = normalColor;
            }
        }
    }
}
