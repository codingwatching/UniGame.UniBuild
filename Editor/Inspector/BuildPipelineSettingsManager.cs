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
        /// <summary>
        /// Get or create the singleton instance
        /// </summary>
        public static BuildPipelineInspectorSettings GetSettings()
        {
            return BuildPipelineInspectorSettings.instance;;
        }

        /// <summary>
        /// Save settings to project settings
        /// </summary>
        public static void SaveSettings(BuildPipelineInspectorSettings settings)
        {
            settings.Save();
        }
    }
}
