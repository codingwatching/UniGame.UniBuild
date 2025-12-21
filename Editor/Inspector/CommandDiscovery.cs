namespace UniGame.UniBuild.Editor.Inspector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Commands;
    using UniModules.UniGame.UniBuild;
    using UnityEditor;

    /// <summary>
    /// Utility class for discovering and managing available build commands
    /// Provides a single source of truth for command discovery
    /// </summary>
    public static class CommandDiscovery
    {
        private static Dictionary<Type, BuildCommandMetadataAttribute> _cachedMetadata;
        private static bool _isInitialized = false;

        /// <summary>
        /// Get all available commands with their metadata
        /// </summary>
        public static Dictionary<Type, BuildCommandMetadataAttribute> GetAllCommandsWithMetadata()
        {
            if (!_isInitialized || _cachedMetadata == null)
            {
                CollectCommandMetadata();
            }
            return new Dictionary<Type, BuildCommandMetadataAttribute>(_cachedMetadata);
        }

        /// <summary>
        /// Get commands grouped by category
        /// </summary>
        public static IEnumerable<IGrouping<string, KeyValuePair<Type, BuildCommandMetadataAttribute>>> GetCommandsGroupedByCategory()
        {
            var metadata = GetAllCommandsWithMetadata();
            return metadata
                .GroupBy(x => x.Value.Category)
                .OrderBy(x => x.Key);
        }

        /// <summary>
        /// Refresh the command cache (call this after domain reload or when commands are added)
        /// </summary>
        public static void RefreshCache()
        {
            _isInitialized = false;
            _cachedMetadata = null;
            CollectCommandMetadata();
        }

        private static void CollectCommandMetadata()
        {
            _cachedMetadata = new Dictionary<Type, BuildCommandMetadataAttribute>();

            // Get both SerializableBuildCommand and UnityBuildCommand types
            var unityCommands = TypeCache.GetTypesDerivedFrom<IUnityBuildCommand>();
            
            foreach (var type in unityCommands)
            {
                if (type.IsAbstract || type.IsInterface) continue;

                var metadata = type.GetCustomAttribute<BuildCommandMetadataAttribute>();
                if (metadata != null)
                {
                    _cachedMetadata[type] = metadata;
                }
            }

            _isInitialized = true;
        }
    }
}
