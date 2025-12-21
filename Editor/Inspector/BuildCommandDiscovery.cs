namespace UniGame.UniBuild.Editor.Inspector
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;

    /// <summary>
    /// Utility class for finding and managing build commands
    /// Provides reflection-based discovery of available commands
    /// </summary>
    public static class BuildCommandDiscovery
    {
        /// <summary>
        /// Find all available build command types
        /// </summary>
        public static IEnumerable<Type> FindAllCommands()
        {
            return TypeCache.GetTypesDerivedFrom<SerializableBuildCommand>();
        }

        /// <summary>
        /// Find all commands in a specific category
        /// </summary>
        public static IEnumerable<Type> FindCommandsByCategory(string category)
        {
            foreach (var commandType in FindAllCommands())
            {
                var metadata = commandType.GetCustomAttribute<BuildCommandMetadataAttribute>();
                if (metadata != null && metadata.Category == category)
                {
                    yield return commandType;
                }
            }
        }

        /// <summary>
        /// Get all command categories
        /// </summary>
        public static IEnumerable<string> GetAllCategories()
        {
            var categories = new HashSet<string>();

            foreach (var commandType in FindAllCommands())
            {
                var metadata = commandType.GetCustomAttribute<BuildCommandMetadataAttribute>();
                if (metadata != null && !string.IsNullOrEmpty(metadata.Category))
                {
                    categories.Add(metadata.Category);
                }
            }

            return categories;
        }

        /// <summary>
        /// Get metadata for a command type
        /// </summary>
        public static BuildCommandMetadataAttribute GetMetadata(Type commandType)
        {
            return commandType.GetCustomAttribute<BuildCommandMetadataAttribute>() 
                ?? new BuildCommandMetadataAttribute(commandType.Name);
        }

        /// <summary>
        /// Search commands by name or description
        /// </summary>
        public static IEnumerable<(Type, BuildCommandMetadataAttribute)> SearchCommands(string searchQuery)
        {
            var query = searchQuery.ToLower();

            foreach (var commandType in FindAllCommands())
            {
                var metadata = GetMetadata(commandType);
                var displayName = metadata.DisplayName ?? commandType.Name;
                var description = metadata.Description ?? string.Empty;

                if (displayName.ToLower().Contains(query) || 
                    description.ToLower().Contains(query) ||
                    commandType.Name.ToLower().Contains(query))
                {
                    yield return (commandType, metadata);
                }
            }
        }
    }
}
