namespace UniGame.UniBuild.Editor.Inspector
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Settings manager using ScriptableSingleton for persistent storage
    /// Stores settings in project settings folder
    /// </summary>
    internal class BuildPipelineSettingsManager
    {
        private static readonly string SETTINGS_PATH = Path.Combine(
            "ProjectSettings",
            "BuildPipelineEditorSettings.asset"
        );

        private static BuildPipelineInspectorSettings _instance;

        /// <summary>
        /// Get or create the singleton instance
        /// </summary>
        public static BuildPipelineInspectorSettings GetSettings()
        {
            if (_instance != null)
                return _instance;

            _instance = LoadSettings();
            if (_instance == null)
            {
                _instance = ScriptableObject.CreateInstance<BuildPipelineInspectorSettings>();
                SaveSettings(_instance);
            }

            return _instance;
        }

        /// <summary>
        /// Load settings from project settings
        /// </summary>
        public static BuildPipelineInspectorSettings LoadSettings()
        {
            try
            {
                var fullPath = Path.Combine(Application.dataPath, "..", SETTINGS_PATH);

                if (!File.Exists(fullPath))
                    return null;

                var json = File.ReadAllText(fullPath);
                var settings = ScriptableObject.CreateInstance<BuildPipelineInspectorSettings>();

                // Simple JSON deserialization using Unity's JsonUtility
                JsonUtility.FromJsonOverwrite(json, settings);
                return settings;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to load Build Pipeline settings: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Save settings to project settings
        /// </summary>
        public static void SaveSettings(BuildPipelineInspectorSettings settings)
        {
            try
            {
                var fullPath = Path.Combine(Application.dataPath, "..", SETTINGS_PATH);
                var directory = Path.GetDirectoryName(fullPath);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var json = JsonUtility.ToJson(settings, true);
                File.WriteAllText(fullPath, json);

                EditorUtility.SetDirty(settings);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to save Build Pipeline settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Reset settings to defaults
        /// </summary>
        public static void ResetSettings()
        {
            _instance = ScriptableObject.CreateInstance<BuildPipelineInspectorSettings>();
            SaveSettings(_instance);
        }
    }
}
