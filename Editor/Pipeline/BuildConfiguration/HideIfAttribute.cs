namespace UniGame.UniBuild.Editor.ClientBuild.BuildConfiguration
{
    using System;

    /// <summary>
    /// Attribute to conditionally hide a field based on a condition
    /// Used by PipelineSettingsRenderer to determine field visibility
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class HideIfAttribute : Attribute
    {
        /// <summary>
        /// Name of a bool field, property, or method to check for visibility
        /// </summary>
        public string Condition { get; }

        public HideIfAttribute(string condition)
        {
            Condition = condition;
        }
    }
}
