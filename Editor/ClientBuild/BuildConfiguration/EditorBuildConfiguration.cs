using UnityEditor.Build.Reporting;

namespace UniGame.UniBuild.Editor
{
    using System;

    [Serializable]
    public class EditorBuildConfiguration : IUniBuilderConfiguration
    {
        private readonly IArgumentsProvider arguments;
        private readonly BuildParameters   buildParameters;
        private BuildReport _buildReport;

        public EditorBuildConfiguration(IArgumentsProvider argumentsProvider, BuildParameters parameters)
        {
            arguments       = argumentsProvider;
            buildParameters = parameters;
        }
    
        public IArgumentsProvider Arguments => arguments;

        public BuildParameters BuildParameters => buildParameters;

        public BuildReport BuildReport
        {
            get => _buildReport;
            set => _buildReport = value;
        }
    }
}
