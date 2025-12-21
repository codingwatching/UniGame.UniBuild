namespace UniGame.UniBuild.Editor.Commands
{
    using System;
    using Editor;
    using Inspector;
    using UnityEditor;
    using UnityEngine.Scripting.APIUpdating;

    [Serializable]
    [MovedFrom(sourceNamespace:"UniModules.UniGame.UniBuild.Editor.ClientBuild.Commands.PreBuildCommands")]
    [BuildCommandMetadata(
        displayName: "Switch Active Build Target",
        description: "Switches the active build target to a different platform (Android, iOS, WebGL, etc.) for multi-platform builds.",
        category: "Build Target"
    )]
    public class SwitchActiveBuildTargetCommand : SerializableBuildCommand
    {
        
        public BuildTargetGroup BuildTargetGroup = BuildTargetGroup.Android;
        public BuildTarget BuildTarget = BuildTarget.Android;
        
        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup,BuildTarget);
        }
    }
}
