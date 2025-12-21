namespace UniGame.UniBuild.Editor.Inspector.Editors
{
    using System.Linq;
    using UniModules.UniGame.UniBuild;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Renders a command group (PipelineCommandsGroup) with nested commands
    /// </summary>
    public class CommandGroupRenderer
    {
        public delegate void OnRemoveCommandDelegate(PipelineCommandsGroup group, BuildCommandStep command);
        public delegate void OnAddCommandDelegate(PipelineCommandsGroup group);
        public delegate void OnMouseDownDelegate(MouseDownEvent evt, PipelineCommandsGroup group, BuildCommandStep step);
        public delegate void OnMouseMoveDelegate(MouseEventBase<MouseMoveEvent> evt, VisualElement target, PipelineCommandsGroup group, BuildCommandStep step);
        public delegate void OnMouseEnterDelegate(MouseEnterEvent evt, VisualElement target, PipelineCommandsGroup group, BuildCommandStep step);
        public delegate void OnMouseLeaveDelegate(MouseLeaveEvent evt, VisualElement target, PipelineCommandsGroup group, BuildCommandStep step);
        public delegate void OnMouseUpDelegate(MouseUpEvent evt, PipelineCommandsGroup group, BuildCommandStep step, VisualElement target);

        private PipelineCommandsGroup _group;
        private BuildCommandStep _step;
        private UniBuildPipeline _selectedPipeline;

        public event OnRemoveCommandDelegate OnRemoveCommand;
        public event OnAddCommandDelegate OnAddCommand;
        public event OnMouseDownDelegate OnNestedMouseDown;
        public event OnMouseMoveDelegate OnNestedMouseMove;
        public event OnMouseEnterDelegate OnNestedMouseEnter;
        public event OnMouseLeaveDelegate OnNestedMouseLeave;
        public event OnMouseUpDelegate OnNestedMouseUp;

        public CommandGroupRenderer(PipelineCommandsGroup group, BuildCommandStep step, UniBuildPipeline selectedPipeline)
        {
            _group = group;
            _step = step;
            _selectedPipeline = selectedPipeline;
            PropertyEditorFactory.SetPipelineReference(selectedPipeline);
        }

        /// <summary>
        /// Render the group with its nested commands
        /// </summary>
        public VisualElement Render()
        {
            var groupWrapper = new VisualElement();
            groupWrapper.style.flexDirection = FlexDirection.Column;
            groupWrapper.style.marginLeft = 0;
            groupWrapper.style.marginRight = 0;
            groupWrapper.style.marginTop = UIThemeConstants.Spacing.Margin;
            groupWrapper.style.marginBottom = UIThemeConstants.Spacing.Margin;

            // Group header
            var groupHeaderRow = CreateGroupHeader();
            groupWrapper.Add(groupHeaderRow);

            // Nested commands foldout
            var groupFoldout = new Foldout { text = "", value = true };
            groupFoldout.style.fontSize = UIThemeConstants.FontSizes.Medium;
            groupFoldout.style.paddingLeft = 0;
            groupFoldout.style.marginLeft = UIThemeConstants.Spacing.Padding;
            groupFoldout.style.marginRight = UIThemeConstants.Spacing.Padding;
            groupFoldout.style.marginTop = 0;
            groupFoldout.style.marginBottom = 0;

            var nestedCommandsList = _group.commands.commands.ToList();
            if (nestedCommandsList.Count > 0)
            {
                var nestedItemsContainer = CreateNestedCommandsContainer(nestedCommandsList);
                groupFoldout.Add(nestedItemsContainer);
            }

            groupWrapper.Add(groupFoldout);
            return groupWrapper;
        }

        /// <summary>
        /// Create the group header with controls
        /// </summary>
        private VisualElement CreateGroupHeader()
        {
            var groupHeaderRow = new VisualElement();
            groupHeaderRow.style.flexDirection = FlexDirection.Row;
            groupHeaderRow.style.alignItems = Align.Center;
            groupHeaderRow.style.justifyContent = Justify.SpaceBetween;
            groupHeaderRow.style.paddingLeft = UIThemeConstants.Spacing.Padding;
            groupHeaderRow.style.paddingRight = UIThemeConstants.Spacing.Padding;
            groupHeaderRow.style.paddingTop = UIThemeConstants.Spacing.SmallMargin;
            groupHeaderRow.style.paddingBottom = UIThemeConstants.Spacing.SmallMargin;

            // Left section: toggle, type, name
            var leftSection = new VisualElement();
            leftSection.style.flexDirection = FlexDirection.Row;
            leftSection.style.alignItems = Align.Center;
            leftSection.style.flexGrow = 1;

            var groupToggle = new Toggle();
            groupToggle.value = _group.IsActive;
            groupToggle.style.marginRight = 6;
            groupToggle.RegisterValueChangedCallback(evt =>
            {
                var field = _group.GetType().GetField("isActive", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(_group, evt.newValue);
                    EditorUtility.SetDirty(_selectedPipeline);
                }
            });
            leftSection.Add(groupToggle);

            var groupTypeLabel = UIElementFactory.CreateLabel(_group.GetType().Name, UIThemeConstants.FontSizes.Small);
            groupTypeLabel.style.marginRight = UIThemeConstants.Spacing.Padding;
            leftSection.Add(groupTypeLabel);

            var groupNameLabel = new Label($"Command Group: {_group.Name}");
            groupNameLabel.style.fontSize = UIThemeConstants.FontSizes.Normal;
            groupNameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            groupNameLabel.style.flexGrow = 1;
            leftSection.Add(groupNameLabel);

            // Right section: buttons
            var rightSection = new VisualElement();
            rightSection.style.flexDirection = FlexDirection.Row;
            rightSection.style.alignItems = Align.Center;

            // Add reorder buttons for the command group itself
            var moveUpBtn = UIElementFactory.CreateButton("↑", () => 
            {
                // Move group up in the parent step
                var parentStep = _step;
                var groupIndex = parentStep.GetCommands().ToList().IndexOf(_group);
                if (groupIndex > 0)
                {
                    var commands = parentStep.GetCommands().ToList();
                    var temp = commands[groupIndex];
                    commands[groupIndex] = commands[groupIndex - 1];
                    commands[groupIndex - 1] = temp;
                    EditorUtility.SetDirty(_selectedPipeline);
                }
            }, UIThemeConstants.Sizes.ButtonSmall);
            rightSection.Add(moveUpBtn);

            var moveDownBtn = UIElementFactory.CreateButton("↓", () => 
            {
                // Move group down in the parent step
                var parentStep = _step;
                var commands = parentStep.GetCommands().ToList();
                var groupIndex = commands.IndexOf(_group);
                if (groupIndex < commands.Count - 1)
                {
                    var temp = commands[groupIndex];
                    commands[groupIndex] = commands[groupIndex + 1];
                    commands[groupIndex + 1] = temp;
                    EditorUtility.SetDirty(_selectedPipeline);
                }
            }, UIThemeConstants.Sizes.ButtonSmall);
            rightSection.Add(moveDownBtn);

            var groupRunBtn = UIElementFactory.CreateButton("Run", () => { /* Execute group */ }, UIThemeConstants.Sizes.ButtonMedium);
            rightSection.Add(groupRunBtn);

            var addCmdBtn = UIElementFactory.CreateButton("Add Cmd", () => OnAddCommand?.Invoke(_group), UIThemeConstants.Sizes.ButtonLarge);
            rightSection.Add(addCmdBtn);

            var removeGroupBtn = UIElementFactory.CreateDangerButton("Remove Step", () => { /* Remove group */ });
            rightSection.Add(removeGroupBtn);

            groupHeaderRow.Add(leftSection);
            groupHeaderRow.Add(rightSection);

            return groupHeaderRow;
        }

        /// <summary>
        /// Create container with nested commands
        /// </summary>
        private VisualElement CreateNestedCommandsContainer(System.Collections.Generic.List<BuildCommandStep> nestedCommandsList)
        {
            var nestedItemsContainer = new VisualElement();
            nestedItemsContainer.style.flexDirection = FlexDirection.Column;
            nestedItemsContainer.style.marginLeft = 12;
            nestedItemsContainer.style.borderLeftWidth = UIThemeConstants.Sizes.BorderWidth;
            nestedItemsContainer.style.borderLeftColor = new StyleColor(new Color(0.4f, 0.6f, 0.8f));
            nestedItemsContainer.style.paddingLeft = UIThemeConstants.Spacing.Padding;

            for (int nestedIdx = 0; nestedIdx < nestedCommandsList.Count; nestedIdx++)
            {
                var nestedCmd = nestedCommandsList[nestedIdx];
                if (nestedCmd == null) continue;

                var renderer = new NestedCommandRenderer(nestedCmd, nestedIdx, _group, _step, _selectedPipeline);
                renderer.OnRemove += (cmd) => OnRemoveCommand?.Invoke(_group, cmd);
                renderer.OnMouseDown += (evt, step, group) => OnNestedMouseDown?.Invoke(evt, group, step);
                renderer.OnMouseMove += (evt, target, step, group) => OnNestedMouseMove?.Invoke(evt, target, group, step);
                renderer.OnMouseEnter += (evt, target, step, group) => OnNestedMouseEnter?.Invoke(evt, target, group, step);
                renderer.OnMouseLeave += (evt, target, step, group) => OnNestedMouseLeave?.Invoke(evt, target, group, step);
                renderer.OnMouseUp += (evt, step, group, target) => OnNestedMouseUp?.Invoke(evt, group, step, target);

                nestedItemsContainer.Add(renderer.Render());
            }

            return nestedItemsContainer;
        }
    }
}
