using UnityEngine;

namespace UniBuild.Commands.Editor
{
    using global::UniGame.UniBuild.Editor;
    using global::UniGame.UniBuild.Editor.Commands;
    using UniGame.UniBuild.Editor.Inspector;
    using UnityEngine.Scripting.APIUpdating;

    [CreateAssetMenu(menuName = "UniBuild/Commands/RemoveDirectory",fileName = nameof(RemoveDirectoryAssetCommand))]
    [MovedFrom(sourceNamespace:"UniModules.UniBuild.Commands.Editor.PathCommands")]
    [BuildCommandMetadata(
        displayName: "Remove Directory (Asset)",
        description: "ScriptableObject wrapper for Remove Directory command, allowing reusable directory deletion configurations to be stored as assets and shared across multiple build pipelines.",
        category: "File Management"
    )]
    public class RemoveDirectoryAssetCommand : UnityBuildCommand
    {
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.HideLabel]
        [Sirenix.OdinInspector.InlineProperty]
#endif
        public RemoveDirectoryCommand command = new RemoveDirectoryCommand();
        
        public override void Execute(IUniBuilderConfiguration configuration)
        {
            command.Execute();
        }
    }
}
