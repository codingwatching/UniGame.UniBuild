namespace UniGame.UniBuild.Editor.Inspector.Editors
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UniModules.UniGame.UniBuild;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Renders a single build command step with all its properties
    /// Handles both regular commands and command groups
    /// </summary>
    public class PipelineStepRenderer
    {
        public delegate void OnStepInteraction(BuildCommandStep step, List<BuildCommandStep> stepsList);
        public delegate void OnRemoveStep(BuildCommandStep step);
        public delegate void OnExecuteStep(IUnityBuildCommand command);
        public delegate void OnMouseDown(MouseDownEvent evt, BuildCommandStep step, List<BuildCommandStep> stepsList);
        public delegate void OnMouseMove(MouseEventBase<MouseMoveEvent> evt, VisualElement target, BuildCommandStep step, List<BuildCommandStep> stepsList);
        public delegate void OnMouseEnter(MouseEnterEvent evt, VisualElement target, BuildCommandStep step, List<BuildCommandStep> stepsList);
        public delegate void OnMouseLeave(MouseLeaveEvent evt, VisualElement target, BuildCommandStep step, int stepIndex);
        public delegate void OnMouseUp(MouseUpEvent evt, BuildCommandStep step, List<BuildCommandStep> stepsList, VisualElement target);

        private BuildCommandStep _step;
        private int _stepIndex;
        private List<BuildCommandStep> _stepsList;
        private UniBuildPipeline _selectedPipeline;
        private bool _isPreBuild;

        public event OnStepInteraction OnMoveUp;
        public event OnStepInteraction OnMoveDown;
        public event OnRemoveStep OnRemove;
        public event OnExecuteStep OnExecute;
        public event OnMouseDown OnStepMouseDown;
        public event OnMouseMove OnStepMouseMove;
        public event OnMouseEnter OnStepMouseEnter;
        public event OnMouseLeave OnStepMouseLeave;
        public event OnMouseUp OnStepMouseUp;

        public PipelineStepRenderer(BuildCommandStep step, int stepIndex, List<BuildCommandStep> stepsList, 
            UniBuildPipeline selectedPipeline, bool isPreBuild)
        {
            _step = step;
            _stepIndex = stepIndex;
            _stepsList = stepsList;
            _selectedPipeline = selectedPipeline;
            _isPreBuild = isPreBuild;
        }

        /// <summary>
        /// Create the visual representation of the step
        /// </summary>
        public VisualElement Render()
        {
            var container = UIElementFactory.CreateAlternatingBgContainer(_stepIndex);
            container.style.borderBottomWidth = UIThemeConstants.Sizes.BorderWidth;
            container.style.borderBottomColor = new StyleColor(UIThemeConstants.Colors.BorderDefault);
            container.style.paddingBottom = UIThemeConstants.Spacing.SmallPadding;

            // Get all commands from the step
            var commands = _step.GetCommands().ToList();

            // Create step title
            string stepTitle = GetStepTitle(commands);

            // Create foldout
            var stepFoldout = new Foldout
            {
                text = stepTitle,
                value = true
            };
            stepFoldout.style.fontSize = UIThemeConstants.FontSizes.Large;
            stepFoldout.style.unityFontStyleAndWeight = FontStyle.Bold;
            stepFoldout.style.paddingLeft = 0;
            stepFoldout.style.marginBottom = 0;

            // Add header if not a group
            bool isGroupCommand = commands.Count > 0 && commands[0] is PipelineCommandsGroup;
            if (!isGroupCommand)
            {
                var headerRow = CreateStepHeaderRow(commands);
                stepFoldout.Add(headerRow);
            }

            // Register drag-drop events
            container.RegisterCallback<MouseDownEvent>(evt => OnStepMouseDown?.Invoke(evt, _step, _stepsList));
            container.RegisterCallback<MouseMoveEvent>(evt => OnStepMouseMove?.Invoke(evt, container, _step, _stepsList));
            container.RegisterCallback<MouseEnterEvent>(evt => OnStepMouseEnter?.Invoke(evt, container, _step, _stepsList));
            container.RegisterCallback<MouseLeaveEvent>(evt => OnStepMouseLeave?.Invoke(evt, container, _step, _stepIndex));
            container.RegisterCallback<MouseUpEvent>(evt => OnStepMouseUp?.Invoke(evt, _step, _stepsList, container));

            // Add content
            var contentContainer = CreateCommandsContent(commands);
            stepFoldout.Add(contentContainer);
            
            container.Add(stepFoldout);
            return container;
        }

        /// <summary>
        /// Get the title for the step based on its commands
        /// </summary>
        private string GetStepTitle(List<IUnityBuildCommand> commands)
        {
            if (commands.Count == 0)
                return "Step (no commands)";

            var firstCommand = commands[0];
            if (firstCommand is PipelineCommandsGroup group)
            {
                var nestedCount = group.commands.Commands.Count();
                return $"{firstCommand.Name} ({nestedCount} command{(nestedCount != 1 ? "s" : "")})";
            }

            return firstCommand.Name;
        }

        /// <summary>
        /// Create the header row with control buttons
        /// </summary>
        private VisualElement CreateStepHeaderRow(List<IUnityBuildCommand> commands)
        {
            var buttons = new List<(string, System.Action, int)>
            {
                ("↑", () => OnMoveUp?.Invoke(_step, _stepsList), UIThemeConstants.Sizes.ButtonSmall),
                ("↓", () => OnMoveDown?.Invoke(_step, _stepsList), UIThemeConstants.Sizes.ButtonSmall),
                ("Run", () => OnExecute?.Invoke(commands.FirstOrDefault()), UIThemeConstants.Sizes.ButtonMedium),
                ("Remove Step", () => OnRemove?.Invoke(_step), UIThemeConstants.Sizes.ButtonXLarge),
            };

            var headerRow = UIElementFactory.CreateButtonRow(buttons.ToArray());
            return headerRow;
        }

        /// <summary>
        /// Create the content area with commands
        /// </summary>
        private VisualElement CreateCommandsContent(List<IUnityBuildCommand> commands)
        {
            var contentContainer = new VisualElement();
            contentContainer.style.flexDirection = FlexDirection.Column;
            contentContainer.style.paddingLeft = 12;

            if (commands.Count == 0)
            {
                contentContainer.Add(UIElementFactory.CreateDimmedLabel(UIThemeConstants.NoCommandsMessage));
            }
            else if (commands.Count == 1 && !(commands[0] is PipelineCommandsGroup))
            {
                // Single regular command
                var command = commands[0];
                var renderer = new CommandPropertyRenderer(command, _selectedPipeline);
                contentContainer.Add(renderer.Render());
            }
            else
            {
                // Multiple commands or groups
                for (int i = 0; i < commands.Count; i++)
                {
                    var command = commands[i];
                    
                    if (command is PipelineCommandsGroup group)
                    {
                        // Render group
                        var groupRenderer = new CommandGroupRenderer(group, _step, _selectedPipeline);
                        groupRenderer.OnRemoveCommand += (g, c) => { /* Handle remove */ };
                        groupRenderer.OnAddCommand += (g) => { /* Handle add */ };
                        contentContainer.Add(groupRenderer.Render());
                    }
                    else
                    {
                        // Render regular command
                        var renderer = new CommandPropertyRenderer(command, _selectedPipeline);
                        contentContainer.Add(renderer.Render());
                    }
                }
            }

            return contentContainer;
        }
    }
}
