using UnityEngine;
using System;
using System.Collections;

namespace ProtocolEMR.UI
{
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        public enum NotificationType
        {
            ItemPickup,
            MissionUpdate,
            AchievementUnlock,
            QuestComplete,
            SystemMessage,
            Warning,
            Error
        }

        [Serializable]
        public class NotificationConfig
        {
            public float defaultDuration = 3f;
            public float itemPickupDuration = 2f;
            public float missionUpdateDuration = 3f;
            public float achievementDuration = 5f;
            public float questCompleteDuration = 5f;
            public float warningDuration = 4f;
        }

        [SerializeField] private NotificationConfig config;
        private Queue<(string, NotificationType)> notificationQueue;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            notificationQueue = new Queue<(string, NotificationType)>();
        }

        public void ShowNotification(string message, NotificationType type = NotificationType.SystemMessage)
        {
            if (HUDManager.Instance == null)
            {
                Debug.LogWarning($"HUDManager not found. Cannot show notification: {message}");
                return;
            }

            float duration = GetDurationForType(type);
            HUDManager.Instance.AddNotification(message, duration);
        }

        public void ShowItemPickup(string itemName, int quantity = 1)
        {
            string message = quantity > 1 ? $"Picked up {quantity}x {itemName}" : $"Picked up {itemName}";
            ShowNotification(message, NotificationType.ItemPickup);
        }

        public void ShowMissionUpdate(string missionText)
        {
            ShowNotification($"New objective: {missionText}", NotificationType.MissionUpdate);
        }

        public void ShowAchievementUnlock(string achievementName, string description = "")
        {
            string message = string.IsNullOrEmpty(description) 
                ? $"Achievement Unlocked: {achievementName}" 
                : $"Achievement Unlocked: {achievementName}\n{description}";
            ShowNotification(message, NotificationType.AchievementUnlock);
        }

        public void ShowQuestComplete(string questName)
        {
            ShowNotification($"Objective Complete: {questName}", NotificationType.QuestComplete);
        }

        public void ShowWarning(string warningText)
        {
            ShowNotification($"⚠ {warningText}", NotificationType.Warning);
        }

        public void ShowError(string errorText)
        {
            ShowNotification($"✗ {errorText}", NotificationType.Error);
        }

        private float GetDurationForType(NotificationType type)
        {
            return type switch
            {
                NotificationType.ItemPickup => config.itemPickupDuration,
                NotificationType.MissionUpdate => config.missionUpdateDuration,
                NotificationType.AchievementUnlock => config.achievementDuration,
                NotificationType.QuestComplete => config.questCompleteDuration,
                NotificationType.Warning => config.warningDuration,
                NotificationType.Error => config.warningDuration,
                _ => config.defaultDuration
            };
        }
    }
}
