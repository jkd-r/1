using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProtocolEMR.Core.Dialogue;

namespace ProtocolEMR.UI
{
    /// <summary>
    /// Lightweight HUD overlay for Unknown dialogue beats. Presents queued
    /// messages, handles auto-dismiss timers, and exposes visibility events so
    /// the UIManager can manage focus between gameplay and overlays.
    /// </summary>
    [DisallowMultipleComponent]
    public class DialogueUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup dialogueCanvas;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TextMeshProUGUI speakerText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Button continueButton;

        [Header("Behavior")]
        [SerializeField] private float defaultDisplayDuration = 4f;
        [SerializeField] private bool requireManualAdvanceWhenDurationZero = true;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip messagePopSfx;

        private readonly Queue<UnknownMessage> pendingMessages = new Queue<UnknownMessage>();
        private Coroutine autoHideRoutine;
        private bool isVisible;
        private UnknownMessage activeMessage;

        public bool IsVisible => isVisible;
        public event Action<bool> OnDialogueVisibilityChanged;

        private void Awake()
        {
            if (dialogueCanvas == null)
            {
                dialogueCanvas = GetComponentInChildren<CanvasGroup>();
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(ShowNextMessage);
            }

            SetCanvasVisible(false, true);
        }

        private void OnEnable()
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.OnMessageDisplay += HandleMessageDisplay;
            }
        }

        private void OnDisable()
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.OnMessageDisplay -= HandleMessageDisplay;
            }
        }

        private void HandleMessageDisplay(UnknownMessage message, MessageDisplayMode mode)
        {
            if (mode == MessageDisplayMode.Phone)
            {
                return;
            }

            pendingMessages.Enqueue(message);
            if (!isVisible)
            {
                ShowNextMessage();
            }
        }

        public void ShowNextMessage()
        {
            if (pendingMessages.Count == 0)
            {
                HideDialogue();
                return;
            }

            activeMessage = pendingMessages.Dequeue();
            ApplyMessage(activeMessage);
            PlayPopSound();
            SetCanvasVisible(true);
            RestartAutoHide(activeMessage);
        }

        public void HideDialogue()
        {
            activeMessage = null;
            StopAutoHide();
            SetCanvasVisible(false);
        }

        private void ApplyMessage(UnknownMessage message)
        {
            if (dialogueText != null)
            {
                dialogueText.text = message.text;
            }

            if (speakerText != null)
            {
                speakerText.text = "Unknown";
                speakerText.gameObject.SetActive(!string.IsNullOrEmpty(speakerText.text));
            }

            if (portraitImage != null)
            {
                portraitImage.enabled = false;
                portraitImage.sprite = null;
            }
        }

        private void RestartAutoHide(UnknownMessage message)
        {
            StopAutoHide();

            float duration = message.displayDuration > 0 ? message.displayDuration : defaultDisplayDuration;
            if (message.displayDuration <= 0f && requireManualAdvanceWhenDurationZero)
            {
                return;
            }

            if (duration <= 0f)
            {
                return;
            }

            autoHideRoutine = StartCoroutine(AutoHideAfter(duration));
        }

        private IEnumerator AutoHideAfter(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);

            if (pendingMessages.Count > 0)
            {
                ShowNextMessage();
            }
            else
            {
                HideDialogue();
            }
        }

        private void StopAutoHide()
        {
            if (autoHideRoutine != null)
            {
                StopCoroutine(autoHideRoutine);
                autoHideRoutine = null;
            }
        }

        private void PlayPopSound()
        {
            if (audioSource != null && messagePopSfx != null)
            {
                audioSource.PlayOneShot(messagePopSfx);
            }
        }

        private void SetCanvasVisible(bool visible, bool instant = false)
        {
            if (dialogueCanvas == null)
            {
                return;
            }

            if (instant)
            {
                dialogueCanvas.alpha = visible ? 1f : 0f;
            }
            else
            {
                dialogueCanvas.alpha = visible ? 1f : 0f;
            }

            dialogueCanvas.blocksRaycasts = visible;
            dialogueCanvas.interactable = visible;

            if (isVisible != visible)
            {
                isVisible = visible;
                OnDialogueVisibilityChanged?.Invoke(isVisible);
            }
        }
    }
}
