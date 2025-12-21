namespace UniGame.UniBuild.Editor.Inspector.Editors
{
    using System.Linq;
    using System.Reflection;
    using UniModules.UniGame.UniBuild;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Renders a nested command (command inside a group)
    /// </summary>
    public class NestedCommandRenderer
    {
        public delegate void OnRemoveDelegate(BuildCommandStep step);
        public delegate void OnMouseDownDelegate(MouseDownEvent evt, BuildCommandStep step, PipelineCommandsGroup group);
        public delegate void OnMouseMoveDelegate(MouseEventBase<MouseMoveEvent> evt, VisualElement target, BuildCommandStep step, PipelineCommandsGroup group);
        public delegate void OnMouseEnterDelegate(MouseEnterEvent evt, VisualElement target, BuildCommandStep step, PipelineCommandsGroup group);
        public delegate void OnMouseLeaveDelegate(MouseLeaveEvent evt, VisualElement target, BuildCommandStep step, PipelineCommandsGroup group);
        public delegate void OnMouseUpDelegate(MouseUpEvent evt, BuildCommandStep step, PipelineCommandsGroup group, VisualElement target);

        private BuildCommandStep _step;
        private IUnityBuildCommand _command;
        private int _index;
        private PipelineCommandsGroup _group;
        private BuildCommandStep _parentStep;
        private UniBuildPipeline _selectedPipeline;

        public event OnRemoveDelegate OnRemove;
        public event OnMouseDownDelegate OnMouseDown;
        public event OnMouseMoveDelegate OnMouseMove;
        public event OnMouseEnterDelegate OnMouseEnter;
        public event OnMouseLeaveDelegate OnMouseLeave;
        public event OnMouseUpDelegate OnMouseUp;

        public NestedCommandRenderer(BuildCommandStep step, int index, PipelineCommandsGroup group, BuildCommandStep parentStep, UniBuildPipeline selectedPipeline)
        {
            _step = step;
            _command = step.GetCommands().FirstOrDefault();
            _index = index;
            _group = group;
            _parentStep = parentStep;
            _selectedPipeline = selectedPipeline;
            PropertyEditorFactory.SetPipelineReference(selectedPipeline);
        }

        /// <summary>
        /// Render the nested command
        /// </summary>
        public VisualElement Render()
        {
            if (_command == null)
                return new VisualElement();

            var itemContainer = new VisualElement();
            itemContainer.style.flexDirection = FlexDirection.Column;
            itemContainer.style.marginBottom = UIThemeConstants.Spacing.Padding;
            itemContainer.style.paddingBottom = UIThemeConstants.Spacing.Padding;
            itemContainer.style.borderBottomWidth = UIThemeConstants.Sizes.BorderWidth;
            itemContainer.style.borderBottomColor = new StyleColor(UIThemeConstants.Colors.BorderDefault);

            // Nested command header
            var headerRow = CreateNestedCommandHeader();
            itemContainer.Add(headerRow);

            // Register drag-drop events
            itemContainer.RegisterCallback<MouseDownEvent>(evt => OnMouseDown?.Invoke(evt, _step, _group));
            itemContainer.RegisterCallback<MouseMoveEvent>(evt => OnMouseMove?.Invoke(evt, itemContainer, _step, _group));
            itemContainer.RegisterCallback<MouseEnterEvent>(evt => OnMouseEnter?.Invoke(evt, itemContainer, _step, _group));
            itemContainer.RegisterCallback<MouseLeaveEvent>(evt => OnMouseLeave?.Invoke(evt, itemContainer, _step, _group));
            itemContainer.RegisterCallback<MouseUpEvent>(evt => OnMouseUp?.Invoke(evt, _step, _group, itemContainer));

            // Nested command properties
            var propsContainer = new VisualElement();
            propsContainer.style.flexDirection = FlexDirection.Column;
            propsContainer.style.marginLeft = UIThemeConstants.Spacing.SmallPadding;
            propsContainer.style.paddingLeft = UIThemeConstants.Spacing.SmallPadding;
            propsContainer.style.paddingRight = UIThemeConstants.Spacing.SmallPadding;

            DisplayCommandProperties(propsContainer);

            itemContainer.Add(propsContainer);
            return itemContainer;
        }

        /// <summary>
        /// Create the header row for nested command
        /// </summary>
        private VisualElement CreateNestedCommandHeader()
        {
            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.justifyContent = Justify.SpaceBetween;
            headerRow.style.marginBottom = 6;

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

            var nameLabel = new Label(_command.Name);
            nameLabel.style.fontSize = UIThemeConstants.FontSizes.Normal;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.flexGrow = 1;
            headerRow.Add(nameLabel);

            var typeLabel = UIElementFactory.CreateLabel(_command.GetType().Name, UIThemeConstants.FontSizes.Small);
            typeLabel.style.marginRight = UIThemeConstants.Spacing.Padding;
            headerRow.Add(typeLabel);

            // Buttons
            var moveUpBtn = UIElementFactory.CreateSmallButton("↑", () => { /* Move up */ });
            headerRow.Add(moveUpBtn);

            var moveDownBtn = UIElementFactory.CreateSmallButton("↓", () => { /* Move down */ });
            headerRow.Add(moveDownBtn);

            var runBtn = UIElementFactory.CreateButton("Run", () => { /* Execute */ }, UIThemeConstants.Sizes.ButtonMedium);
            headerRow.Add(runBtn);

            var delBtn = UIElementFactory.CreateDangerButton("Del", () => OnRemove?.Invoke(_step), UIThemeConstants.Sizes.ButtonSmall);
            headerRow.Add(delBtn);

            return headerRow;
        }

        /// <summary>
        /// Display command properties
        /// </summary>
        private void DisplayCommandProperties(VisualElement container)
        {
            var commandType = _command.GetType();
            var fields = commandType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            int propertyCount = 0;

            foreach (var fieldInfo in fields)
            {
                if (fieldInfo.Name.StartsWith("<") || fieldInfo.Name.StartsWith("m_"))
                    continue;

                if (fieldInfo.Name == "isActive")
                    continue;

                propertyCount++;

                var fieldContainer = UIElementFactory.CreateLabeledRow(fieldInfo.Name, out var contentArea);
                
                var fieldEditor = PropertyEditorFactory.CreateFieldEditor(fieldInfo, _command);
                if (fieldEditor != null)
                {
                    fieldEditor.style.minWidth = UIThemeConstants.Sizes.EditorMinWidth;
                    contentArea.Add(fieldEditor);
                }

                container.Add(fieldContainer);
            }

            if (propertyCount == 0)
            {
                container.Add(UIElementFactory.CreateDimmedLabel(UIThemeConstants.NoPublicFieldsMessage));
            }
        }
    }
}
