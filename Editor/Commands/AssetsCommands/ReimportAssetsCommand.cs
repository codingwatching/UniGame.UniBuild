namespace UniGame.UniBuild.Editor
{
    using System;
    using System.Collections.Generic;
    using Inspector;
    using UnityEditor;
    using UnityEngine.Scripting.APIUpdating;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Reimports target assets locations or specific assets to refresh their state.
    /// </summary>
    [MovedFrom(sourceNamespace:"UniGame.UniBuild.Editor.Commands.AssetsCommands")]
    [Serializable]
    [BuildCommandMetadata(
        displayName: "Reimport Assets",
        description: "Refreshes the import state of specified assets or asset folders, reprocessing them with current import settings. Useful for updating assets after changing import configuration.",
        category: "Asset Management"
    )]
    public class ReimportAssetsCommand : SerializableBuildCommand
    {
        public List<Object> assets = new();

        public ImportAssetOptions ImportAssetOptions = ImportAssetOptions.ImportRecursive;
        
        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            foreach (var asset in assets)
            {
                if(!asset) continue;
                var assetPath = AssetDatabase.GetAssetPath(asset);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions);
            }       
        }
    }
}
