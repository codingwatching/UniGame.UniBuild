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

            var itemContainer = UIElementFactory.CreateAlternatingBgContainer(_index);
            itemContainer.style.borderBottomWidth = UIThemeConstants.Sizes.BorderWidth;
            itemContainer.style.borderBottomColor = new StyleColor(UIThemeConstants.Colors.BorderDefault);
            itemContainer.style.paddingBottom = UIThemeConstants.Spacing.SmallPadding;

            // Nested command header (shown outside foldout, always visible)
            var headerRow = CreateNestedCommandHeader();
            itemContainer.Add(headerRow);

            // Register drag-drop events with TrickleDown
            itemContainer.RegisterCallback<MouseDownEvent>(evt => OnMouseDown?.Invoke(evt, _step, _group), TrickleDown.TrickleDown);
            itemContainer.RegisterCallback<MouseMoveEvent>(evt => OnMouseMove?.Invoke(evt, itemContainer, _step, _group), TrickleDown.TrickleDown);
            itemContainer.RegisterCallback<MouseEnterEvent>(evt => OnMouseEnter?.Invoke(evt, itemContainer, _step, _group), TrickleDown.TrickleDown);
            itemContainer.RegisterCallback<MouseLeaveEvent>(evt => OnMouseLeave?.Invoke(evt, itemContainer, _step, _group), TrickleDown.TrickleDown);
            itemContainer.RegisterCallback<MouseUpEvent>(evt => OnMouseUp?.Invoke(evt, _step, _group, itemContainer), TrickleDown.TrickleDown);

            // Create foldout for properties (collapsed by default)
            var propertiesFoldout = new Foldout
            {
                text = "Properties",
                value = false
            };
            propertiesFoldout.style.fontSize = UIThemeConstants.FontSizes.Normal;
            propertiesFoldout.style.paddingLeft = 0;
            propertiesFoldout.style.marginBottom = 0;

            // Prevent drag from starting when clicking on foldout
            propertiesFoldout.RegisterCallback<MouseDownEvent>(evt => evt.StopPropagation());

            // Nested command properties
            var propsContainer = new VisualElement();
            propsContainer.style.flexDirection = FlexDirection.Column;
            propsContainer.style.paddingLeft = UIThemeConstants.Spacing.SmallPadding;
            propsContainer.style.paddingRight = UIThemeConstants.Spacing.SmallPadding;

            DisplayCommandProperties(propsContainer);

            propertiesFoldout.Add(propsContainer);
            itemContainer.Add(propertiesFoldout);
            
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
            headerRow.style.paddingLeft = UIThemeConstants.Spacing.Padding;
            headerRow.style.paddingRight = UIThemeConstants.Spacing.Padding;
            headerRow.style.paddingTop = UIThemeConstants.Spacing.SmallPadding;
            headerRow.style.paddingBottom = UIThemeConstants.Spacing.SmallPadding;

            // Left section: toggle, type, name
            var leftSection = new VisualElement();
            leftSection.style.flexDirection = FlexDirection.Row;
            leftSection.style.alignItems = Align.Center;
            leftSection.style.flexGrow = 1;

            var toggle = new Toggle();
            toggle.value = _command.IsActive;
            toggle.style.marginRight = 6;
            toggle.RegisterValueChangedCallback(evt =>
            {
                var field = _command.GetType().GetField("isActive", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(_command, evt.newValue);
                    EditorUtility.SetDirty(_selectedPipeline);
                }
            });
            leftSection.Add(toggle);

            var nameLabel = new Label(_command.Name);
            nameLabel.style.fontSize = UIThemeConstants.FontSizes.Normal;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.flexGrow = 1;
            leftSection.Add(nameLabel);

            headerRow.Add(leftSection);

            // Right section: buttons
            var rightSection = new VisualElement();
            rightSection.style.flexDirection = FlexDirection.Row;
            rightSection.style.alignItems = Align.Center;

            // Only show up arrow if not first in list
            if (_index > 0)
            {
                var moveUpBtn = UIElementFactory.CreateButton("↑", () => { /* Move up */ }, UIThemeConstants.Sizes.ButtonSmall);
                rightSection.Add(moveUpBtn);
            }

            // Only show down arrow if not last in list
            var nestedCommandsList = _group.commands.commands.ToList();
            if (_index < nestedCommandsList.Count - 1)
            {
                var moveDownBtn = UIElementFactory.CreateButton("↓", () => { /* Move down */ }, UIThemeConstants.Sizes.ButtonSmall);
                rightSection.Add(moveDownBtn);
            }

            var runBtn = UIElementFactory.CreateButton("Run", () => { /* Execute */ }, UIThemeConstants.Sizes.ButtonMedium);
            rightSection.Add(runBtn);

            var delBtn = UIElementFactory.CreateDangerButton("Del", () => OnRemove?.Invoke(_step), UIThemeConstants.Sizes.ButtonSmall);
            rightSection.Add(delBtn);

            headerRow.Add(rightSection);
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
                fieldContainer.style.marginBottom = UIThemeConstants.Spacing.SmallPadding;
                
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
