
namespace UniGame.UniBuild.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Abstract;
    using UnityEngine;
    using UnityEngine.Scripting.APIUpdating;

#if ODIN_INSPECTOR
     using Sirenix.OdinInspector;
#endif

#if TRI_INSPECTOR
    using TriInspector;
#endif
    
    [Serializable]
    [MovedFrom(sourceNamespace:"UniModules.UniGame.UniBuild")]
    public class BuildCommands : IBuildCommands,IEnumerable<IUnityBuildCommand>
    {
        private static Color _oddColor = new Color(0.2f, 0.4f, 0.3f);
        
        #region inspector
    
#if ODIN_INSPECTOR
        [Searchable]
        [ListDrawerSettings(ElementColor = nameof(GetElementColor))]//ListElementLabelName = "GroupLabel"
#endif
        [Space]
        public List<BuildCommandStep> commands = new List<BuildCommandStep>();

        #endregion

        public IEnumerable<IUnityBuildCommand> Commands => FilterActiveCommands(commands);

        private IEnumerable<IUnityBuildCommand> FilterActiveCommands(IEnumerable<BuildCommandStep> filteredCommands)
        {
            var items = new List<IUnityBuildCommand>();

            foreach (var command in filteredCommands) {
                items.AddRange(command.GetCommands());
            }
            
            return items;
        }

        private Color GetElementColor(int index, Color defaultColor)
        {
            var result = index % 2 == 0 
                ? _oddColor : defaultColor;
            return result;
        }

        public IEnumerator<IUnityBuildCommand> GetEnumerator()
        {
            foreach (var buildCommand in Commands)
            {
                yield return  buildCommand;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var buildCommand in Commands)
            {
                yield return  buildCommand;
            }
        }
    }
}
