namespace UniGame.UniBuild.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine.Scripting.APIUpdating;
    using Object = UnityEngine.Object;

    /// <summary>
    /// reimport target assets locations
    /// or target assets
    /// </summary>
    [MovedFrom(sourceNamespace:"UniGame.UniBuild.Editor.Commands.AssetsCommands")]
    [Serializable]
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
