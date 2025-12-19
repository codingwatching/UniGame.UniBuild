namespace UniGame.UniBuild.Editor
{
    using global::UniGame.UniBuild.Editor;
    using UnityEditor.Build.Reporting;

    public interface IUnityPlayerBuilder
    {
        BuildReport Build(IUniBuilderConfiguration configuration);
    }
}