namespace UniGame.UniBuild.Editor.Inspector
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UniModules.UniGame.UniBuild;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Utility class for managing build pipelines
    /// Handles loading, searching, and creating pipelines
    /// </summary>
    public static class PipelineManager
    {
        /// <summary>
        /// Load all available pipelines in the project
        /// </summary>
        public static List<ScriptableCommandsGroup> LoadAllPipelines()
        {
            var pipelines = new List<ScriptableCommandsGroup>();
            var guids = AssetDatabase.FindAssets("t:ScriptableCommandsGroup");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var pipeline = AssetDatabase.LoadAssetAtPath<ScriptableCommandsGroup>(path);

                if (pipeline != null)
                {
                    pipelines.Add(pipeline);
                }
            }

            return pipelines;
        }

        /// <summary>
        /// Search pipelines by name
        /// </summary>
        public static List<ScriptableCommandsGroup> SearchPipelines(string searchQuery, List<ScriptableCommandsGroup> pipelines)
        {
            var query = searchQuery.ToLower();
            return pipelines
                .Where(p => p.name.ToLower().Contains(query))
                .OrderBy(p => p.name)
                .ToList();
        }

        /// <summary>
        /// Create a new pipeline at the specified path
        /// </summary>
        public static ScriptableCommandsGroup CreatePipeline(string folderPath, string pipelineName)
        {
            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                var parts = folderPath.Split('/');
                var currentPath = string.Empty;

                foreach (var part in parts)
                {
                    if (string.IsNullOrEmpty(part)) continue;

                    var newPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath}/{part}";

                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        var parentPath = Path.GetDirectoryName(newPath);
                        var folderName = Path.GetFileName(newPath);
                        AssetDatabase.CreateFolder(parentPath, folderName);
                    }

                    currentPath = newPath;
                }
            }

            // Create the pipeline asset
            var pipeline = ScriptableObject.CreateInstance<ScriptableCommandsGroup>();
            var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{pipelineName}.asset");

            AssetDatabase.CreateAsset(pipeline, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return pipeline;
        }

        /// <summary>
        /// Delete a pipeline
        /// </summary>
        public static bool DeletePipeline(ScriptableCommandsGroup pipeline)
        {
            if (pipeline == null) return false;

            var path = AssetDatabase.GetAssetPath(pipeline);
            if (string.IsNullOrEmpty(path)) return false;

            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return true;
        }

        /// <summary>
        /// Duplicate a pipeline
        /// </summary>
        public static ScriptableCommandsGroup DuplicatePipeline(ScriptableCommandsGroup source)
        {
            if (source == null) return null;

            var sourcePath = AssetDatabase.GetAssetPath(source);
            var sourceFolder = Path.GetDirectoryName(sourcePath);
            var sourceNameWithoutExtension = Path.GetFileNameWithoutExtension(sourcePath);

            var newPath = AssetDatabase.GenerateUniqueAssetPath(
                $"{sourceFolder}/{sourceNameWithoutExtension}_Copy.asset"
            );

            AssetDatabase.CopyAsset(sourcePath, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return AssetDatabase.LoadAssetAtPath<ScriptableCommandsGroup>(newPath);
        }

        /// <summary>
        /// Get pipeline path relative to Assets folder
        /// </summary>
        public static string GetPipelinePath(ScriptableCommandsGroup pipeline)
        {
            return AssetDatabase.GetAssetPath(pipeline);
        }

        /// <summary>
        /// Get pipeline statistics
        /// </summary>
        public static (int totalCount, int activeCount) GetPipelineStats(ScriptableCommandsGroup pipeline)
        {
            if (pipeline?.commands?.commands == null)
                return (0, 0);

            int totalCount = 0;
            int activeCount = 0;

            foreach (var step in pipeline.commands.commands)
            {
                foreach (var cmd in step.GetCommands())
                {
                    totalCount++;
                    if (cmd.IsActive)
                        activeCount++;
                }
            }

            return (totalCount, activeCount);
        }
    }
}
