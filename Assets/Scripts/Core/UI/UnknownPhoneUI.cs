using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using ProtocolEMR.Core.Dialogue;
using ProtocolEMR.Core.Input;

namespace ProtocolEMR.Core.UI
{
    /// <summary>
    /// Phone-style chat interface for Unknown entity communication.
    /// Displays message history, typing animations, and player response options.
    /// </summary>
    public class UnknownPhoneUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject phonePanel;
        [SerializeField] private ScrollRect messageScrollRect;
        [SerializeField] private Transform messageContainer;
        [SerializeField] private GameObject messagePrefab;
        [SerializeField] private TextMeshProUGUI unreadCountText;
        [SerializeField] private GameObject unreadBadge;

        [Header("Settings")]
        [SerializeField] private float typingSpeed = 0.05f;
        [SerializeField] private bool enableTypingAnimation = true;
        [SerializeField] private int maxDisplayedMessages = 50;

        [Header("Audio")]
        [SerializeField] private AudioClip messageReceivedSound;
        [SerializeField] private AudioClip typingSoundLoop;
        [SerializeField] private AudioSource audioSource;

        private bool isPhoneOpen = false;
        private int unreadMessageCount = 0;
        private List<ChatMessageUI> displayedMessages = new List<ChatMessageUI>();
        private bool isTyping = false;

        public bool IsPhoneOpen => isPhoneOpen;

        private void Awake()
        {
            if (phonePanel != null)
            {
                phonePanel.SetActive(false);
            }

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        private void Start()
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.OnMessageDisplay += HandleMessageDisplay;
            }

            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnPhone += TogglePhone;
            }

            UpdateUnreadBadge();
        }

        private void OnDestroy()
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.OnMessageDisplay -= HandleMessageDisplay;
            }

            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnPhone -= TogglePhone;
            }
        }

        private void HandleMessageDisplay(UnknownMessage message, MessageDisplayMode displayMode)
        {
            if (displayMode == MessageDisplayMode.Phone || displayMode == MessageDisplayMode.Both)
            {
                AddMessage(message);
            }
        }

        public void TogglePhone()
        {
            SetPhoneOpen(!isPhoneOpen);
        }

        public void SetPhoneOpen(bool open)
        {
            isPhoneOpen = open;
            
            if (phonePanel != null)
            {
                phonePanel.SetActive(open);
            }

            if (open)
            {
                unreadMessageCount = 0;
                UpdateUnreadBadge();
                ScrollToBottom();
            }

            Time.timeScale = open ? 0f : 1f;
        }

        public void AddMessage(UnknownMessage message)
        {
            if (messagePrefab == null || messageContainer == null)
            {
                Debug.LogWarning("UnknownPhoneUI: Missing message prefab or container");
                return;
            }

            if (!isPhoneOpen)
            {
                unreadMessageCount++;
                UpdateUnreadBadge();
                PlayMessageReceivedSound();
            }

            GameObject messageObj = Instantiate(messagePrefab, messageContainer);
            ChatMessageUI chatMessage = messageObj.GetComponent<ChatMessageUI>();

            if (chatMessage == null)
            {
                chatMessage = messageObj.AddComponent<ChatMessageUI>();
            }

            displayedMessages.Add(chatMessage);

            if (enableTypingAnimation && !isPhoneOpen)
            {
                StartCoroutine(AnimateTyping(chatMessage, message.text));
            }
            else
            {
                chatMessage.SetMessage(message.text, "Unknown", System.DateTime.Now);
            }

            if (displayedMessages.Count > maxDisplayedMessages)
            {
                ChatMessageUI oldMessage = displayedMessages[0];
                displayedMessages.RemoveAt(0);
                if (oldMessage != null)
                {
                    Destroy(oldMessage.gameObject);
                }
            }

            ScrollToBottom();
        }

        private System.Collections.IEnumerator AnimateTyping(ChatMessageUI chatMessage, string fullText)
        {
            isTyping = true;
            chatMessage.SetMessage("", "Unknown", System.DateTime.Now);

            if (typingSoundLoop != null && audioSource != null)
            {
                audioSource.clip = typingSoundLoop;
                audioSource.loop = true;
                audioSource.Play();
            }

            for (int i = 0; i <= fullText.Length; i++)
            {
                string displayText = fullText.Substring(0, i);
                chatMessage.UpdateMessageText(displayText);
                yield return new WaitForSecondsRealtime(typingSpeed);
            }

            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            isTyping = false;
        }

        private void PlayMessageReceivedSound()
        {
            if (messageReceivedSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(messageReceivedSound);
            }
        }

        private void UpdateUnreadBadge()
        {
            if (unreadBadge != null)
            {
                unreadBadge.SetActive(unreadMessageCount > 0);
            }

            if (unreadCountText != null)
            {
                unreadCountText.text = unreadMessageCount.ToString();
            }
        }

        private void ScrollToBottom()
        {
            if (messageScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                messageScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        public void ClearMessages()
        {
            foreach (ChatMessageUI msg in displayedMessages)
            {
                if (msg != null)
                {
                    Destroy(msg.gameObject);
                }
            }
            displayedMessages.Clear();
            unreadMessageCount = 0;
            UpdateUnreadBadge();
        }

        public void LoadHistoryFromManager()
        {
            if (UnknownDialogueManager.Instance == null)
                return;

            ClearMessages();

            List<MessageHistory> history = UnknownDialogueManager.Instance.GetMessageHistory();
            
            foreach (MessageHistory entry in history)
            {
                if (messagePrefab != null && messageContainer != null)
                {
                    GameObject messageObj = Instantiate(messagePrefab, messageContainer);
                    ChatMessageUI chatMessage = messageObj.GetComponent<ChatMessageUI>();

                    if (chatMessage == null)
                    {
                        chatMessage = messageObj.AddComponent<ChatMessageUI>();
                    }

                    System.DateTime timestamp = System.DateTime.Now.AddSeconds(-(Time.time - entry.timestamp));
                    chatMessage.SetMessage(entry.messageText, "Unknown", timestamp);
                    displayedMessages.Add(chatMessage);
                }
            }

            ScrollToBottom();
        }
    }

    /// <summary>
    /// Individual chat message UI component.
    /// </summary>
    public class ChatMessageUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI senderText;
        [SerializeField] private TextMeshProUGUI timestampText;

        private void Awake()
        {
            if (messageText == null)
            {
                messageText = transform.Find("MessageText")?.GetComponent<TextMeshProUGUI>();
            }

            if (senderText == null)
            {
                senderText = transform.Find("SenderText")?.GetComponent<TextMeshProUGUI>();
            }

            if (timestampText == null)
            {
                timestampText = transform.Find("TimestampText")?.GetComponent<TextMeshProUGUI>();
            }
        }

        public void SetMessage(string message, string sender, System.DateTime timestamp)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }

            if (senderText != null)
            {
                senderText.text = sender;
            }

            if (timestampText != null)
            {
                timestampText.text = timestamp.ToString("HH:mm");
            }
        }

        public void UpdateMessageText(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }
        }
    }
}
