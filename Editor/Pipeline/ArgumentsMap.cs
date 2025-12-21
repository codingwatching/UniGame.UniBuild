namespace UniModules.UniGame.UniBuild
{
    using System;
    using global::UniGame.UniBuild.Editor.Utils;

    [Serializable]
    public class ArgumentsMap
    {
        public bool isEnable = true;
        public SerializableDictionary<string, BuildArgumentValue> arguments = new();
    }
}