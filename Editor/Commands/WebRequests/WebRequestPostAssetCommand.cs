using UnityEngine;

namespace UniGame.BuildCommands.Editor
{
    using global::UniGame.UniBuild.Editor;
    using global::UniGame.UniBuild.Editor.Commands;
    using UniBuild.Editor.Inspector;
    using UnityEngine.Scripting.APIUpdating;

    [CreateAssetMenu(menuName = "UniBuild//CommandsWeb/WebRequestPost",fileName = nameof(WebRequestPostAssetCommand))]
    [MovedFrom(sourceNamespace:"UniModules.UniGame.BuildCommands.Editor.WebRequests")]
    [BuildCommandMetadata(
        displayName: "Web Request POST (Asset)",
        description: "ScriptableObject wrapper for Web Request POST command, allowing reusable POST request configurations to be stored as assets and shared across multiple build pipelines.",
        category: "Distribution"
    )]
    public class WebRequestPostAssetCommand : UnityBuildCommand
    {
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.HideLabel]
        [Sirenix.OdinInspector.InlineProperty]
#endif
        public WebRequestPostCommand command = new WebRequestPostCommand();
        
        public override void Execute(IUniBuilderConfiguration configuration)
        {
            command.Execute();
        }
    }
}
