namespace UniGame.UniBuild.Editor.Inspector
{
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using UniModules.UniGame.UniBuild;
    using UniGame.UniBuild.Editor;
    using UnityEditor;
    using UnityEngine;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// Pipeline executor with logging and error handling
    /// Manages the execution of pipelines and individual steps
    /// </summary>
    public class PipelineExecutor
    {
        private PipelineExecutionState _currentExecution;
        private bool _isExecuting;
        private bool _enableDetailedLogging;

        public event Action<PipelineExecutionState> OnExecutionStarted;
        public event Action<PipelineExecutionState> OnExecutionCompleted;
        public event Action<string> OnStepExecuted;
        public event Action<string, Exception> OnStepFailed;

        public bool IsExecuting => _isExecuting;
        public PipelineExecutionState CurrentExecution => _currentExecution;

        public PipelineExecutor(bool enableDetailedLogging = false)
        {
            _enableDetailedLogging = enableDetailedLogging;
        }

        /// <summary>
        /// Execute a complete pipeline
        /// </summary>
        public PipelineExecutionState ExecutePipeline(ScriptableCommandsGroup pipeline)
        {
            if (pipeline == null)
            {
                throw new ArgumentNullException(nameof(pipeline));
            }

            if (_isExecuting)
            {
                throw new InvalidOperationException("Another pipeline is already executing");
            }

            _isExecuting = true;
            _currentExecution = new PipelineExecutionState(pipeline.name);
            var startTime = EditorApplication.timeSinceStartup;

            try
            {
                OnExecutionStarted?.Invoke(_currentExecution);

                if (_enableDetailedLogging)
                {
                    Debug.Log($"[Build Pipeline] Starting execution of '{pipeline.name}'");
                }

                // Create a simple configuration object for command execution
                var config = new EditorBuildConfiguration(null, null);

                foreach (var step in pipeline.commands.commands)
                {
                    // Execute each command in the step
                    foreach (var command in step.GetCommands())
                    {
                        if (!command.IsActive)
                        {
                            if (_enableDetailedLogging)
                            {
                                Debug.Log($"[Build Pipeline] Skipping inactive step: {command.Name}");
                            }
                            continue;
                        }

                        ExecuteStep(command, config, _currentExecution);
                    }
                }

                var executionTime = (float)(EditorApplication.timeSinceStartup - startTime);
                _currentExecution.SetExecutionTime(executionTime);
                _currentExecution.SetResult(true);

                if (_enableDetailedLogging)
                {
                    Debug.Log($"[Build Pipeline] Pipeline '{pipeline.name}' completed successfully in {executionTime:F2}s");
                }

                return _currentExecution;
            }
            catch (Exception ex)
            {
                var executionTime = (float)(EditorApplication.timeSinceStartup - startTime);
                _currentExecution.SetExecutionTime(executionTime);
                _currentExecution.SetResult(false, ex.Message);

                Debug.LogError($"[Build Pipeline] Pipeline execution failed: {ex}", pipeline);

                return _currentExecution;
            }
            finally
            {
                OnExecutionCompleted?.Invoke(_currentExecution);
                _isExecuting = false;
            }
        }

        /// <summary>
        /// Execute a single step
        /// </summary>
        public bool ExecuteStep(SerializableBuildCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var config = new EditorBuildConfiguration(null, null);
            var tempExecution = new PipelineExecutionState("Single Step");

            try
            {
                ExecuteStep(command, config, tempExecution);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Build Pipeline] Step execution failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Internal method to execute a single step
        /// </summary>
        private void ExecuteStep(IUnityBuildCommand command, IUniBuilderConfiguration config, PipelineExecutionState execution)
        {
            var stepName = command.Name;
            var stepStartTime = EditorApplication.timeSinceStartup;

            try
            {
                if (_enableDetailedLogging)
                {
                    Debug.Log($"[Build Pipeline] Executing step: {stepName}");
                }

                // Validate the command
                if (!command.Validate(config))
                {
                    throw new InvalidOperationException($"Validation failed for step: {stepName}");
                }

                // Execute the command
                command.Execute(config);

                execution.AddStepExecution(stepName, true, null);
                OnStepExecuted?.Invoke(stepName);

                if (_enableDetailedLogging)
                {
                    var stepTime = EditorApplication.timeSinceStartup - stepStartTime;
                    Debug.Log($"[Build Pipeline] Step completed: {stepName} ({stepTime:F2}s)");
                }
            }
            catch (Exception ex)
            {
                execution.AddStepExecution(stepName, false, ex.Message ?? ex.GetType().Name);
                OnStepFailed?.Invoke(stepName, ex);

                Debug.LogError($"[Build Pipeline] Step failed: {stepName}\n{ex.Message}");

                throw;
            }
        }

        /// <summary>
        /// Cancel current execution
        /// </summary>
        public void Cancel()
        {
            _isExecuting = false;
        }
    }
}
