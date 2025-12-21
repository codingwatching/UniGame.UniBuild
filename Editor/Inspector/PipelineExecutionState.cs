namespace UniGame.UniBuild.Editor.Inspector
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    /// <summary>
    /// Runtime data for pipeline execution tracking
    /// </summary>
    [Serializable]
    public class PipelineExecutionState
    {
        [SerializeField]
        private string pipelineName;

        [SerializeField]
        private float executionTime;

        [SerializeField]
        private bool success;

        [SerializeField]
        private string errorMessage;

        [SerializeField]
        private List<StepExecutionState> stepStates = new List<StepExecutionState>();

        [SerializeField]
        private long executionTimestamp;

        public string PipelineName => pipelineName;
        public float ExecutionTime => executionTime;
        public bool Success => success;
        public string ErrorMessage => errorMessage;
        public IReadOnlyList<StepExecutionState> StepStates => stepStates.AsReadOnly();
        public DateTime ExecutionDateTime => UnixTimeStampToDateTime(executionTimestamp);

        public PipelineExecutionState(string name)
        {
            pipelineName = name;
            executionTimestamp = DateTime.UtcNow.Ticks;
        }

        public void SetResult(bool success, string error = "")
        {
            this.success = success;
            errorMessage = error;
        }

        public void SetExecutionTime(float time)
        {
            executionTime = time;
        }

        public void AddStepExecution(string stepName, bool stepSuccess, string stepError = "")
        {
            stepStates.Add(new StepExecutionState
            {
                stepName = stepName,
                success = stepSuccess,
                errorMessage = stepError
            });
        }

        private static DateTime UnixTimeStampToDateTime(long ticks)
        {
            return new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();
        }

        [Serializable]
        public class StepExecutionState
        {
            public string stepName;
            public bool success;
            public string errorMessage;
        }
    }
}
