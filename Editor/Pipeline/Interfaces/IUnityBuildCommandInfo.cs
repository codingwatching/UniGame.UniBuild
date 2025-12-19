namespace UniGame.UniBuild.Editor 
{
    public interface IUnityBuildCommandInfo {

        bool IsActive { get; }

        string Name { get; }
    }
}