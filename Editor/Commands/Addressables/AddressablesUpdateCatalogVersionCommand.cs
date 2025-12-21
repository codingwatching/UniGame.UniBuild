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
        displayName: "Addressables Update Catalog Version",
        description: "Updates the Addressables catalog version identifier using the application version or a custom version string, enabling incremental content updates and version tracking.",
        category: "Addressables"
    )]
    public class AddressablesUpdateCatalogVersionCommand : SerializableBuildCommand
    {
        public bool   useAppVersion = true;
        public bool   useBuildNumber = false;
        public string manualVersion = Application.version;
        
        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            var version = Application.version;
        }
    }
}
