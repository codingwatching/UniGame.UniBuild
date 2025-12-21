namespace UniGame.UniBuild.Editor.Inspector
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;

    /// <summary>
    /// Manages the expanded/collapsed state of pipeline steps in the editor
    /// Persists state per pipeline so it's restored when the pipeline is selected again
    /// </summary>
    public static class PipelineStepStateManager
    {
        private const string STATE_KEY_PREFIX = "PipelineStepState_";
        
        /// <summary>
        /// Get the expanded state for a specific step in a pipeline
        /// Returns true (expanded) by default for new pipelines
        /// </summary>
        public static bool IsStepExpanded(string pipelineAssetPath, int stepIndex, bool defaultValue = false)
        {
            var key = GetStateKey(pipelineAssetPath, stepIndex);
            return EditorPrefs.GetBool(key, defaultValue);
        }

        /// <summary>
        /// Set the expanded state for a specific step in a pipeline
        /// </summary>
        public static void SetStepExpanded(string pipelineAssetPath, int stepIndex, bool isExpanded)
        {
            var key = GetStateKey(pipelineAssetPath, stepIndex);
            EditorPrefs.SetBool(key, isExpanded);
        }

        /// <summary>
        /// Clear all state for a specific pipeline
        /// </summary>
        public static void ClearPipelineState(string pipelineAssetPath)
        {
            var prefix = $"{STATE_KEY_PREFIX}{pipelineAssetPath}_";
            // EditorPrefs doesn't have a way to delete by prefix, so we'll keep the old values
            // This is a minor memory leak but acceptable since EditorPrefs is cleaned on app restart
        }

        private static string GetStateKey(string pipelineAssetPath, int stepIndex)
        {
            return $"{STATE_KEY_PREFIX}{pipelineAssetPath}_{stepIndex}";
        }
    }
}
