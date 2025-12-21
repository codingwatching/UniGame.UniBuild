namespace UniModules.UniGame.UniBuild
{
    using System;
    using System.Reflection;
    using global::UniGame.UniBuild.Editor;
    using global::UniGame.UniBuild.Editor.Inspector;
    using UnityEditor;
    using UnityEngine;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    [Serializable]
    [BuildCommandMetadata(
        displayName: "Audio Notification",
        description: "Plays an audio notification when the build process completes, with customizable title, message, and notification sound.",
        category: "Notifications"
    )]
    public class AudioNotificationCommand : SerializableBuildCommand
    {
        public string title = "Build Finished";
        public string message = "The build process has completed";
        
        public AudioClip notificationClip;
        public bool loop = true;
        
        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            Execute();
        }

        
#if ODIN_INSPECTOR
        [Button]
#endif
        public void Execute()
        {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
            AudioPreview.PlayClip(notificationClip, 0, loop);
            var result = EditorUtility.DisplayDialog(title, message, "OK");
            AudioPreview.StopAll();
#endif
        }


        public static class AudioPreview
        {
            static Assembly unityEditorAssembly = typeof(UnityEditor.Editor).Assembly;
            static Type audioUtilType = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

            public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false)
            {
                if (clip == null) return;

                var method = audioUtilType.GetMethod(
                    "PlayPreviewClip",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                    null);

                if (method != null)
                    method.Invoke(null, new object[] { clip, startSample, loop });
            }

            public static void StopAll()
            {
                MethodInfo method = audioUtilType.GetMethod(
                    "StopAllPreviewClips",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                if (method != null)
                    method.Invoke(null, null);
            }
        }
    }
}