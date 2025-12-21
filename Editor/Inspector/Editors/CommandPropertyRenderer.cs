namespace UniGame.UniBuild.Editor.Inspector.Editors
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Renders command properties (single command inspector)
    /// </summary>
    public class CommandPropertyRenderer
    {
        private IUnityBuildCommand _command;
        private UniBuildPipeline _selectedPipeline;

        public CommandPropertyRenderer(IUnityBuildCommand command, UniBuildPipeline selectedPipeline)
        {
            _command = command;
            _selectedPipeline = selectedPipeline;
            PropertyEditorFactory.SetPipelineReference(selectedPipeline);
        }

        public VisualElement Render()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;
            container.style.paddingLeft = UIThemeConstants.Spacing.SmallPadding;
            container.style.paddingTop = UIThemeConstants.Spacing.SmallPadding;
            container.style.paddingRight = UIThemeConstants.Spacing.SmallPadding;
            container.style.marginLeft = 0;
            container.style.marginRight = 0;

            // Command header with toggle
            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.marginBottom = UIThemeConstants.Spacing.Padding;
            headerRow.style.paddingLeft = UIThemeConstants.Spacing.SmallPadding;
            headerRow.style.paddingRight = UIThemeConstants.Spacing.SmallPadding;

            var toggle = new Toggle();
            toggle.value = _command.IsActive;
            toggle.style.marginRight = 6;
            toggle.RegisterValueChangedCallback(evt =>
            {
                var field = _command.GetType().GetField("isActive", BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(_command, evt.newValue);
                    EditorUtility.SetDirty(_selectedPipeline);
                }
            });
            headerRow.Add(toggle);

            var typeLabel = UIElementFactory.CreateLabel(_command.GetType().Name, UIThemeConstants.FontSizes.Small);
            headerRow.Add(typeLabel);

            container.Add(headerRow);

            // Command properties
            DisplayCommandProperties(container);

            return container;
        }

        /// <summary>
        /// Display all properties of the command
        /// </summary>
        private void DisplayCommandProperties(VisualElement container)
        {
            var commandType = _command.GetType();
            var fields = commandType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            int propertyCount = 0;

            foreach (var fieldInfo in fields)
            {
                // Skip backing fields and internal Unity fields
                if (fieldInfo.Name.StartsWith("<") || fieldInfo.Name.StartsWith("m_"))
                    continue;

                // Skip isActive as it's handled by toggle
                if (fieldInfo.Name == "isActive")
                    continue;

                propertyCount++;

                // Create field container
                var fieldContainer = UIElementFactory.CreateLabeledRow(fieldInfo.Name, out var contentArea);
                
                // Create field editor
                var fieldEditor = PropertyEditorFactory.CreateFieldEditor(fieldInfo, _command);
                if (fieldEditor != null)
                {
                    fieldEditor.style.minWidth = UIThemeConstants.Sizes.EditorMinWidth;
                    contentArea.Add(fieldEditor);
                }

                container.Add(fieldContainer);

                // Handle serializable types with nested properties
                if (IsSerializableType(fieldInfo.FieldType))
                {
                    var fieldValue = fieldInfo.GetValue(_command);
                    if (fieldValue != null)
                    {
                        DisplayNestedProperties(fieldInfo, fieldValue, container);
                    }
                }
            }

            if (propertyCount == 0)
            {
                container.Add(UIElementFactory.CreateDimmedLabel(UIThemeConstants.NoPublicFieldsMessage));
            }
        }

        /// <summary>
        /// Display nested properties of serializable types
        /// </summary>
        private void DisplayNestedProperties(FieldInfo parentField, object parentValue, VisualElement container)
        {
            var nestedFields = parentField.FieldType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            
            if (nestedFields.Length == 0)
                return;

            var nestedContainer = UIElementFactory.CreateNestedPropertiesContainer();

            foreach (var nestedFieldInfo in nestedFields)
            {
                // Skip backing fields
                if (nestedFieldInfo.Name.StartsWith("<") || nestedFieldInfo.Name.StartsWith("m_"))
                    continue;

                var nestedFieldContainer = UIElementFactory.CreateLabeledRow(nestedFieldInfo.Name, out var contentArea);
                
                // Create nested editor
                var nestedValue = nestedFieldInfo.GetValue(parentValue);
                var nestedEditor = PropertyEditorFactory.CreateFieldEditor(nestedFieldInfo, parentValue);
                if (nestedEditor != null)
                {
                    nestedEditor.style.minWidth = UIThemeConstants.Sizes.EditorMinWidth;
                    contentArea.Add(nestedEditor);
                }

                nestedContainer.Add(nestedFieldContainer);
            }

            container.Add(nestedContainer);
        }

        /// <summary>
        /// Check if type is serializable
        /// </summary>
        private bool IsSerializableType(System.Type type)
        {
            if (type.GetCustomAttributes(typeof(System.SerializableAttribute), false).Length > 0)
                return true;

            if (type.IsValueType || type == typeof(string) || type.IsEnum || type.IsArray)
                return false;

            return type.IsClass && !typeof(UnityEngine.Object).IsAssignableFrom(type);
        }
    }
}
