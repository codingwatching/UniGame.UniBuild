namespace UniGame.BuildCommands.Editor
{
    using System;
    using global::UniGame.UniBuild.Editor;
    using global::UniGame.UniBuild.Editor.Commands;
    using UniBuild.Editor.Inspector;
    using UnityEngine;

    [Serializable]
    [CreateAssetMenu(menuName = "UniBuild/Commands/Addressables/ApplyGroupsTemplateCommand",fileName = nameof(AddressablesApplyGroupsTemplateCommand))]
    [BuildCommandMetadata(
        displayName: "Addressables Apply Groups Template",
        description: "ScriptableObject wrapper for applying Addressables group templates, enabling reusable configuration templates for asset group organization and bundling strategies.",
        category: "Addressables"
    )]
    public class AddressablesApplyGroupsTemplateCommand : UnityBuildCommand
    {
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.InlineProperty]
        [Sirenix.OdinInspector.HideLabel]
#endif
        [SerializeField]
        private ApplyAddressablesTemplatesCommand command = new ApplyAddressablesTemplatesCommand();

        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            Execute();
        }
        
        public void Execute()
        {
            command.Execute();
        }
        
    }
}
