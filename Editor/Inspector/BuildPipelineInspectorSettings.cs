namespace UniGame.UniBuild.Editor.Inspector
{
    using System;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    /// <summary>
    /// Settings for the Build Pipeline Inspector editor
    /// Stored as ScriptableSingleton to persist user preferences in project settings
    /// </summary>
    public class BuildPipelineInspectorSettings : ScriptableSingleton<BuildPipelineInspectorSettings>
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

        private BuildPipelineInspectorSettings()
        {
            // Private constructor to prevent direct instantiation
            // ScriptableSingleton handles creation internally
        }

        /// <summary>
        /// Default path for creating new pipelines
        /// </summary>
        public string PipelineCreationPath
        {
            get => pipelineCreationPath;
            set
            {
                if (pipelineCreationPath != value)
                {
                    pipelineCreationPath = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }

        /// <summary>
        /// Whether to automatically refresh pipeline list when files change
        /// </summary>
        public bool AutoRefreshPipelines
        {
            get => autoRefreshPipelines;
            set
            {
                if (autoRefreshPipelines != value)
                {
                    autoRefreshPipelines = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }

        /// <summary>
        /// Maximum number of execution history items to keep
        /// </summary>
        public int MaxHistorySize
        {
            get => maxHistorySize;
            set
            {
                int newValue = Mathf.Max(10, value);
                if (maxHistorySize != newValue)
                {
                    maxHistorySize = newValue;
                    EditorUtility.SetDirty(this);
                }
            }
        }

        /// <summary>
        /// Whether to show detailed command descriptions in the inspector
        /// </summary>
        public bool ShowCommandDescriptions
        {
            get => showCommandDescriptions;
            set
            {
                if (showCommandDescriptions != value)
                {
                    showCommandDescriptions = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }

        /// <summary>
        /// Whether to enable detailed logging during pipeline execution
        /// </summary>
        public bool EnableDetailedLogging
        {
            get => enableDetailedLogging;
            set
            {
                if (enableDetailedLogging != value)
                {
                    enableDetailedLogging = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }

        /// <summary>
        /// Get the singleton instance (automatically created if needed)
        /// </summary>
        public static BuildPipelineInspectorSettings GetSettings()
        {
            // Access instance property to ensure singleton is loaded
            // This is safe to call after initialization
            return instance;
        }

        /// <summary>
        /// Check if the singleton has been initialized
        /// </summary>
        public static bool IsInitialized()
        {
            // Check if instance has been created without triggering lazy initialization
            return instance != null;
        }
    }
}
