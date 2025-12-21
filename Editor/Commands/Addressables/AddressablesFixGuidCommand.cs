using System;
using UniGame.AddressableTools.Editor;
using global::UniGame.UniBuild.Editor;

namespace UniBuild.Commands.Editor
{
    using UniGame.UniBuild.Editor.Inspector;
    using UnityEngine.Scripting.APIUpdating;

    [Serializable]
    [MovedFrom(sourceNamespace:"UniModules.UniBuild.Commands.Editor.Addressables")]
    [BuildCommandMetadata(
        displayName: "Addressables Fix GUID",
        description: "Fixes Addressables GUID references and resolves common addressable asset errors, revalidating and correcting broken asset references and configuration issues.",
        category: "Addressables"
    )]
    public class AddressablesFixGuidCommand : SerializableBuildCommand
    {
        public override void Execute(IUniBuilderConfiguration buildParameters) => Execute();

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        private void Execute() => AddressablesAssetsFix.FixAddressablesErrors();
    }
}
