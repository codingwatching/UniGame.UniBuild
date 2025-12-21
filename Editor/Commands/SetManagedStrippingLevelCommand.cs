namespace UniGame.UniBuild.Editor.Commands
{
    using System;
    using Editor;
    using Inspector;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Scripting.APIUpdating;

    /// <summary>
    /// Sets the managed code stripping level for IL2CPP compilation
    /// </summary>
    [Serializable]
    [MovedFrom(sourceNamespace:"UniModules.UniGame.UniBuild.Editor.ClientBuild.Commands.PreBuildCommands")]
    [BuildCommandMetadata(
        displayName: "Set Managed Stripping Level",
        description: "Configures the managed code stripping level (Disabled, Low, Medium, High) for IL2CPP builds to reduce final build size.",
        category: "Build Optimization"
    )]
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
