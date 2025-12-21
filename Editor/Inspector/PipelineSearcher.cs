namespace UniGame.UniBuild.Editor.Inspector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniModules.UniGame.UniBuild;

    /// <summary>
    /// Advanced search functionality for pipelines and commands
    /// Provides filtering and sorting capabilities
    /// </summary>
    public static class PipelineSearcher
    {
        /// <summary>
        /// Search pipelines with advanced filtering
        /// </summary>
        public static List<ScriptableCommandsGroup> SearchPipelinesByName(
            List<ScriptableCommandsGroup> pipelines,
            string searchQuery,
            StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return new List<ScriptableCommandsGroup>(pipelines);

            var query = searchQuery.Trim();
            return pipelines
                .Where(p => p.name.Contains(query, comparison))
                .OrderBy(p => p.name)
                .ToList();
        }

        /// <summary>
        /// Search pipelines by step count
        /// </summary>
        public static List<ScriptableCommandsGroup> SearchPipelinesByStepCount(
            List<ScriptableCommandsGroup> pipelines,
            int minSteps,
            int? maxSteps = null)
        {
            return pipelines
                .Where(p => p.commands.commands.Count >= minSteps &&
                            (maxSteps == null || p.commands.commands.Count <= maxSteps))
                .OrderBy(p => p.commands.commands.Count)
                .ToList();
        }

        /// <summary>
        /// Search pipelines by active status
        /// </summary>
        public static List<ScriptableCommandsGroup> SearchPipelinesByActiveStatus(
            List<ScriptableCommandsGroup> pipelines,
            bool includeInactive = true)
        {
            if (includeInactive)
                return pipelines.ToList();

            return pipelines
                .Where(p => p.commands.Any(c => c.IsActive))
                .ToList();
        }

        /// <summary>
        /// Search steps in a pipeline
        /// </summary>
        public static List<IUnityBuildCommand> SearchSteps(
            ScriptableCommandsGroup pipeline,
            string searchQuery,
            StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (pipeline == null || string.IsNullOrWhiteSpace(searchQuery))
                return new List<IUnityBuildCommand>();

            var query = searchQuery.Trim();
            return pipeline.commands.Commands
                .Where(c => c.Name.Contains(query, comparison))
                .ToList();
        }

        /// <summary>
        /// <summary>
        /// Search steps by active status
        /// </summary>
        public static List<IUnityBuildCommand> SearchStepsByStatus(
            ScriptableCommandsGroup pipeline,
            bool? activeOnly = null)
        {
            if (pipeline == null)
                return new List<IUnityBuildCommand>();

            if (activeOnly == null)
                return pipeline.commands.Commands.ToList();

            return pipeline.commands.Commands
                .Where(c => c.IsActive == activeOnly.Value)
                .ToList();
        }

        /// <summary>
        /// Search commands by type name
        /// </summary>
        public static List<IUnityBuildCommand> SearchStepsByType(
            ScriptableCommandsGroup pipeline,
            string typeName,
            StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (pipeline == null || string.IsNullOrWhiteSpace(typeName))
                return new List<IUnityBuildCommand>();

            return pipeline.commands.Commands
                .Where(c => c.GetType().Name.Contains(typeName, comparison))
                .ToList();
        }

        /// <summary>
        /// Get pipeline statistics for search results
        /// </summary>
        public static PipelineSearchStatistics GetStatistics(List<ScriptableCommandsGroup> pipelines)
        {
            var totalSteps = 0;
            var activeSteps = 0;
            var inactiveSteps = 0;

            foreach (var p in pipelines)
            {
                foreach (var cmd in p.commands.Commands)
                {
                    totalSteps++;
                    if (cmd.IsActive)
                        activeSteps++;
                    else
                        inactiveSteps++;
                }
            }

            return new PipelineSearchStatistics
            {
                TotalPipelines = pipelines.Count,
                TotalSteps = totalSteps,
                ActiveSteps = activeSteps,
                InactiveSteps = inactiveSteps
            };
        }
    }

    /// <summary>
    /// Statistics for search results
    /// </summary>
    public struct PipelineSearchStatistics
    {
        public int TotalPipelines { get; set; }
        public int TotalSteps { get; set; }
        public int ActiveSteps { get; set; }
        public int InactiveSteps { get; set; }

        public override string ToString()
        {
            return $"Pipelines: {TotalPipelines}, Steps: {TotalSteps} ({ActiveSteps} active, {InactiveSteps} inactive)";
        }
    }
}
