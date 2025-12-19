namespace UniGame.UniBuild.Editor.Commands
{
    using System;
    using Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Scripting.APIUpdating;

    /// <summary>
    /// update current project version
    /// </summary>
    [Serializable]
    [MovedFrom(sourceNamespace:"UniModules.UniGame.UniBuild.Editor.ClientBuild.Commands.PreBuildCommands")]
    public class SetManagedStrippingLevelCommand : SerializableBuildCommand
    {
        [SerializeField]
        private ManagedStrippingLevel _managedStrippingLevel = ManagedStrippingLevel.Disabled;

        public override void Execute(IUniBuilderConfiguration configuration)
        {
            var buildParameters = configuration.BuildParameters;

            PlayerSettings.SetManagedStrippingLevel(buildParameters.buildTargetGroup,_managedStrippingLevel);
        }
        
    }
}
