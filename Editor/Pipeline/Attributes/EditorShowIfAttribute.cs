namespace Editor.Pipeline.Attributes
{
    using System;

    /// <summary>
    /// Attribute to conditionally show a field based on a condition
    /// Used by PipelineSettingsRenderer to determine field visibility
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EditorShowIfAttribute : Attribute
    {
        /// <summary>
        /// Name of a bool field, property, or method to check for visibility
        /// </summary>
        public string Condition { get; }

        public EditorShowIfAttribute(string condition)
        {
            Condition = condition;
        }
    }
}