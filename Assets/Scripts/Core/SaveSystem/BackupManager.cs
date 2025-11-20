using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ProtocolEMR.Core.SaveSystem
{
    /// <summary>
    /// Manages automatic backup creation and rotation for save files.
    /// Keeps the last 3 backups for each save file to prevent data loss.
    /// </summary>
    public static class BackupManager
    {
        private const int MAX_BACKUPS_PER_SAVE = 3;
        private static string backupDirectory;

        static BackupManager()
        {
            backupDirectory = Path.Combine(Application.persistentDataPath, "Backups");
            Directory.CreateDirectory(backupDirectory);
        }

        /// <summary>
        /// Creates a backup of the specified save file before overwriting.
        /// </summary>
        public static bool CreateBackup(string saveFilePath)
        {
            try
            {
                if (!File.Exists(saveFilePath))
                {
                    Debug.LogWarning($"Cannot create backup: Source file does not exist: {saveFilePath}");
                    return false;
                }

                string fileName = Path.GetFileNameWithoutExtension(saveFilePath);
                string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                string backupFileName = $"{fileName}_backup_{timestamp}.sav";
                string backupPath = Path.Combine(backupDirectory, backupFileName);

                File.Copy(saveFilePath, backupPath, true);

                RotateBackups(fileName);

                Debug.Log($"Backup created: {backupFileName}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create backup: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Rotates backups, keeping only the most recent MAX_BACKUPS_PER_SAVE.
        /// </summary>
        private static void RotateBackups(string saveFileName)
        {
            try
            {
                string[] backupFiles = Directory.GetFiles(backupDirectory, $"{saveFileName}_backup_*.sav");
                
                if (backupFiles.Length > MAX_BACKUPS_PER_SAVE)
                {
                    var sortedBackups = backupFiles
                        .Select(f => new { Path = f, CreationTime = File.GetCreationTime(f) })
                        .OrderByDescending(f => f.CreationTime)
                        .ToList();

                    for (int i = MAX_BACKUPS_PER_SAVE; i < sortedBackups.Count; i++)
                    {
                        File.Delete(sortedBackups[i].Path);
                        Debug.Log($"Old backup deleted: {Path.GetFileName(sortedBackups[i].Path)}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to rotate backups: {e.Message}");
            }
        }

        /// <summary>
        /// Gets all available backups for a specific save file.
        /// </summary>
        public static List<string> GetBackupsForSave(string saveFileName)
        {
            try
            {
                string[] backupFiles = Directory.GetFiles(backupDirectory, $"{saveFileName}_backup_*.sav");
                
                return backupFiles
                    .Select(f => new { Path = f, CreationTime = File.GetCreationTime(f) })
                    .OrderByDescending(f => f.CreationTime)
                    .Select(f => f.Path)
                    .ToList();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get backups: {e.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Restores a save file from the most recent backup.
        /// </summary>
        public static bool RestoreFromBackup(string saveFilePath)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(saveFilePath);
                List<string> backups = GetBackupsForSave(fileName);

                if (backups.Count == 0)
                {
                    Debug.LogError($"No backups found for: {fileName}");
                    return false;
                }

                string mostRecentBackup = backups[0];
                File.Copy(mostRecentBackup, saveFilePath, true);

                Debug.Log($"Restored from backup: {Path.GetFileName(mostRecentBackup)}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to restore from backup: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Deletes all backups for a specific save file.
        /// </summary>
        public static void DeleteBackupsForSave(string saveFileName)
        {
            try
            {
                string[] backupFiles = Directory.GetFiles(backupDirectory, $"{saveFileName}_backup_*.sav");
                
                foreach (string backupFile in backupFiles)
                {
                    File.Delete(backupFile);
                }

                Debug.Log($"Deleted {backupFiles.Length} backups for: {saveFileName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete backups: {e.Message}");
            }
        }

        /// <summary>
        /// Creates a manual backup with a custom name.
        /// </summary>
        public static bool CreateManualBackup(string saveFilePath, string customName)
        {
            try
            {
                if (!File.Exists(saveFilePath))
                {
                    Debug.LogError($"Cannot create manual backup: Source file does not exist");
                    return false;
                }

                string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                string backupFileName = $"{customName}_manual_{timestamp}.sav";
                string backupPath = Path.Combine(backupDirectory, backupFileName);

                File.Copy(saveFilePath, backupPath, true);

                Debug.Log($"Manual backup created: {backupFileName}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create manual backup: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the total size of all backups in MB.
        /// </summary>
        public static float GetTotalBackupSize()
        {
            try
            {
                string[] backupFiles = Directory.GetFiles(backupDirectory, "*.sav");
                long totalBytes = backupFiles.Sum(f => new FileInfo(f).Length);
                return totalBytes / (1024f * 1024f);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to calculate backup size: {e.Message}");
                return 0f;
            }
        }

        /// <summary>
        /// Cleans up all old backups, keeping only recent ones.
        /// </summary>
        public static void CleanupOldBackups(int daysToKeep = 30)
        {
            try
            {
                string[] backupFiles = Directory.GetFiles(backupDirectory, "*.sav");
                DateTime cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
                int deletedCount = 0;

                foreach (string backupFile in backupFiles)
                {
                    if (File.GetCreationTime(backupFile) < cutoffDate)
                    {
                        File.Delete(backupFile);
                        deletedCount++;
                    }
                }

                Debug.Log($"Cleaned up {deletedCount} old backups");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to cleanup old backups: {e.Message}");
            }
        }
    }
}
