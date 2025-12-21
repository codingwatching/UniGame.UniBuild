namespace UniGame.UniBuild.Editor.Inspector
{
    using System;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    /// <summary>
    /// Settings for the Build Pipeline Inspector editor
    /// Stored as ScriptableSingleton to persist user preferences
    /// </summary>
    public class BuildPipelineInspectorSettings : ScriptableObject
    {
        [SerializeField]
        private string pipelineCreationPath = "Assets/BuildPipelines";

        [SerializeField]
        private bool autoRefreshPipelines = true;

        [SerializeField]
        private int maxHistorySize = 100;

        [SerializeField]
        private bool showCommandDescriptions = true;

        [SerializeField]
        private bool enableDetailedLogging = false;

        /// <summary>
        /// Default path for creating new pipelines
        /// </summary>
        public string PipelineCreationPath
        {
            get => pipelineCreationPath;
            set => pipelineCreationPath = value;
        }

        /// <summary>
        /// Whether to automatically refresh pipeline list when files change
        /// </summary>
        public bool AutoRefreshPipelines
        {
            get => autoRefreshPipelines;
            set => autoRefreshPipelines = value;
        }

        /// <summary>
        /// Maximum number of execution history items to keep
        /// </summary>
        public int MaxHistorySize
        {
            get => maxHistorySize;
            set => maxHistorySize = Mathf.Max(10, value);
        }

        /// <summary>
        /// Whether to show detailed command descriptions in the inspector
        /// </summary>
        public bool ShowCommandDescriptions
        {
            get => showCommandDescriptions;
            set => showCommandDescriptions = value;
        }

        /// <summary>
        /// Whether to enable detailed logging during pipeline execution
        /// </summary>
        public bool EnableDetailedLogging
        {
            get => enableDetailedLogging;
            set => enableDetailedLogging = value;
        }

        /// <summary>
        /// Get or create the singleton instance
        /// </summary>
        public static BuildPipelineInspectorSettings GetOrCreate()
        {
#if UNITY_EDITOR
            var instance = ScriptableObject.CreateInstance<BuildPipelineInspectorSettings>();
            return instance;
#else
            return null;
#endif
        }
    }
}
