namespace UniGame.BuildCommands.Editor
{
    using System;
    using global::UniGame.UniBuild.Editor;
    using UniBuild.Editor.Inspector;
    using UnityEngine;
    using UnityEngine.Scripting.APIUpdating;

    [Serializable]
    [MovedFrom(sourceNamespace:"UniModules.UniGame.BuildCommands.Editor.Addressables")]
    [BuildCommandMetadata(
        displayName: "Addressables Update Group Template",
        description: "Updates Addressables asset group templates and reapplies group schema settings, synchronizing group configurations with updated template definitions.",
        category: "Addressables"
    )]
    public class ApplyAddressablesGroupsTemplateCommand : SerializableBuildCommand
    {
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.InlineProperty]
        [Sirenix.OdinInspector.HideLabel]
#endif
        [SerializeField]
        private ApplyAddressablesTemplatesCommand command = new ApplyAddressablesTemplatesCommand();

        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            command.Execute();
        }

        
    }
}
