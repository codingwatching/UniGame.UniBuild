namespace UniGame.UniBuild.Editor.Inspector.Editors
{
    using System;
    using System.Collections.Generic;
    using UniModules.UniGame.UniBuild;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Manages drag-drop operations for pipeline steps
    /// Centralizes all drag-drop logic and event handling
    /// </summary>
    public class DragDropManager
    {
        private BuildCommandStep _draggedStep;
        private List<BuildCommandStep> _draggedFromList;
        private PipelineCommandsGroup _draggedFromGroup;
        private VisualElement _draggedElementClone;
        private VisualElement _root;

        public event Action<BuildCommandStep, List<BuildCommandStep>, List<BuildCommandStep>> OnStepSwapped;
        public event Action<BuildCommandStep, PipelineCommandsGroup, BuildCommandStep, BuildCommandStep> OnNestedCommandSwapped;

        public bool IsDragging => _draggedStep != null;
        public BuildCommandStep DraggedStep => _draggedStep;

        public DragDropManager(VisualElement rootElement)
        {
            _root = rootElement;
            RegisterGlobalMouseUp();
        }

        /// <summary>
        /// Register the global mouse up handler
        /// </summary>
        private void RegisterGlobalMouseUp()
        {
            _root?.RegisterCallback<MouseUpEvent>(evt => OnGlobalMouseUp(), TrickleDown.NoTrickleDown);
        }

        /// <summary>
        /// Start dragging a step
        /// </summary>
        public void StartDragStep(BuildCommandStep step, List<BuildCommandStep> stepsList, VisualElement sourceElement)
        {
            if (step == null || stepsList == null) return;

            _draggedStep = step;
            _draggedFromList = stepsList;
            _draggedFromGroup = null;
            
            CreateDragVisual(sourceElement);
        }

        /// <summary>
        /// Start dragging a nested command
        /// </summary>
        public void StartDragNestedCommand(BuildCommandStep step, PipelineCommandsGroup group, VisualElement sourceElement)
        {
            if (step == null || group == null) return;

            _draggedStep = step;
            _draggedFromGroup = group;
            _draggedFromList = null;
            
            CreateDragVisual(sourceElement);
        }

        /// <summary>
        /// Update drag visual position
        /// </summary>
        public void UpdateDragVisualPosition(Vector2 mousePosition)
        {
            if (_draggedElementClone != null)
            {
                _draggedElementClone.style.left = mousePosition.x - 50;
                _draggedElementClone.style.top = mousePosition.y - 20;
            }
        }

        /// <summary>
        /// Complete step drag and swap
        /// </summary>
        public bool TrySwapSteps(BuildCommandStep targetStep, List<BuildCommandStep> targetList)
        {
            if (_draggedStep == null || _draggedStep == targetStep || _draggedFromList == null)
                return false;

            if (_draggedFromList != targetList)
                return false;

            int draggedIndex = targetList.IndexOf(_draggedStep);
            int targetIndex = targetList.IndexOf(targetStep);

            if (draggedIndex < 0 || targetIndex < 0 || draggedIndex == targetIndex)
                return false;

            // Perform swap
            var temp = targetList[draggedIndex];
            targetList[draggedIndex] = targetList[targetIndex];
            targetList[targetIndex] = temp;

            OnStepSwapped?.Invoke(_draggedStep, _draggedFromList, targetList);
            return true;
        }

        /// <summary>
        /// Complete nested command drag and swap
        /// </summary>
        public bool TrySwapNestedCommands(BuildCommandStep targetStep, PipelineCommandsGroup targetGroup)
        {
            if (_draggedStep == null || _draggedStep == targetStep || _draggedFromGroup == null)
                return false;

            if (_draggedFromGroup != targetGroup)
                return false;

            int draggedIndex = targetGroup.commands.commands.IndexOf(_draggedStep);
            int targetIndex = targetGroup.commands.commands.IndexOf(targetStep);

            if (draggedIndex < 0 || targetIndex < 0 || draggedIndex == targetIndex)
                return false;

            // Perform swap
            var temp = targetGroup.commands.commands[draggedIndex];
            targetGroup.commands.commands[draggedIndex] = targetGroup.commands.commands[targetIndex];
            targetGroup.commands.commands[targetIndex] = temp;

            OnNestedCommandSwapped?.Invoke(_draggedStep, targetGroup, 
                targetGroup.commands.commands[draggedIndex], 
                targetGroup.commands.commands[targetIndex]);
            return true;
        }

        /// <summary>
        /// Create visual drag clone
        /// </summary>
        private void CreateDragVisual(VisualElement sourceElement)
        {
            CleanupDragVisual();

            if (sourceElement == null || _root == null)
                return;

            _draggedElementClone = UIElementFactory.CreateDragVisualClone(sourceElement);
            _root.Add(_draggedElementClone);
        }

        /// <summary>
        /// Clean up drag visual
        /// </summary>
        private void CleanupDragVisual()
        {
            if (_draggedElementClone != null && _draggedElementClone.parent != null)
            {
                _draggedElementClone.RemoveFromHierarchy();
            }
            _draggedElementClone = null;
        }

        /// <summary>
        /// Global mouse up - always cleanup
        /// </summary>
        private void OnGlobalMouseUp()
        {
            CleanupDragVisual();
            _draggedStep = null;
            _draggedFromList = null;
            _draggedFromGroup = null;
        }
    }
}
