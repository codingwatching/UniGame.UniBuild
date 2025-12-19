namespace UniGame.UniBuild.Editor
{
    public interface IUnityBuildCommandValidator
    {
        bool Validate(IUniBuilderConfiguration config);
    }
}