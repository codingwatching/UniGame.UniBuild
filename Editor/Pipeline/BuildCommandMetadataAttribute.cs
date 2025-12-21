namespace UniGame.UniBuild.Editor.Inspector
{
    using System;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    /// <summary>
    /// Attribute to provide command metadata for the Build Pipeline Inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class BuildCommandMetadataAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string IconPath { get; set; }

        public BuildCommandMetadataAttribute(
            string displayName = "",
            string description = "",
            string category = "Misc",
            string iconPath = "")
        {
            DisplayName = displayName;
            Description = description;
            Category = category;
            IconPath = iconPath;
        }
    }
}
