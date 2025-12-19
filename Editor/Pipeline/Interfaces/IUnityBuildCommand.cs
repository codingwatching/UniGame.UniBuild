namespace UniGame.UniBuild.Editor
{
    public interface IUnityBuildCommand : IUnityBuildCommandValidator, IUnityBuildCommandInfo
    {
        void Execute(IUniBuilderConfiguration configuration);
    }
}
