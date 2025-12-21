using UniGame.Runtime.Extension;

namespace UniGame.BuildCommands.Editor
{
    using System;
    using global::UniGame.Utils;
    using global::UniGame.UniBuild.Editor;
    using UniBuild.Editor.Inspector;
    using UnityEngine;
    using UnityEngine.Networking;
    using UnityEngine.Scripting.APIUpdating;

    [Serializable]
    [MovedFrom(sourceNamespace:"UniModules.UniGame.BuildCommands.Editor.WebRequests")]
    [BuildCommandMetadata(
        displayName: "Web Request POST",
        description: "Sends an HTTP POST request to a specified API endpoint with customizable headers, parameters, and body content. Useful for notifying external services, uploading build data, or triggering remote processes.",
        category: "Distribution"
    )]
    public class WebRequestPostCommand : SerializableBuildCommand
    {
        public string apiUrl = "";

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.BoxGroup("Parameters")]
#endif
        public WebRequestParameters header = new WebRequestParameters() {
            {"Content-Type","application/json"},
            {"Accept","application/json"},
        };
        
        [Space(4)]
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.BoxGroup("Parameters")]
#endif
        public WebRequestParameters parameters = new WebRequestParameters();
        
        public override void Execute(IUniBuilderConfiguration configuration) => Execute();

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        public void Execute()
        {
            var targetUrl = apiUrl.CombineUrlParameters(parameters);

#if UNITY_2022_2_OR_NEWER
            var webRequest = UnityWebRequest.PostWwwForm(targetUrl,string.Empty);
#else
            var webRequest = UnityWebRequest.Post(targetUrl,string.Empty);
#endif
            foreach (var headerParameter in header) {
                webRequest.SetRequestHeader(headerParameter.Key,headerParameter.Value);
            }
            
            Debug.Log($"Send Post to : {webRequest.uri}");
            
            var requestAsyncOperation = webRequest.SendWebRequest();
            requestAsyncOperation.completed += x => {

                if (webRequest.result ==UnityWebRequest.Result.ConnectionError || 
                    webRequest.result ==UnityWebRequest.Result.ProtocolError || 
                    webRequest.result ==UnityWebRequest.Result.DataProcessingError) {
                    Debug.Log(webRequest.error);
                }
                else {
                    Debug.Log($"Request to {apiUrl} complete. Code: {webRequest.responseCode}");
                }

                webRequest.Cancel();
            };
        }
    }
}
