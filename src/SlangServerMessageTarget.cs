using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;
using System;
using System.Threading.Tasks;

namespace SlangClient
{
    public class SlangServerMessageTarget
    {
        internal readonly static SlangServerMessageTarget Instance = new SlangServerMessageTarget();

        [JsonRpcMethod(Methods.WorkspaceConfigurationName)]
        public Task<object> OnWorkspaceConfigurationAsync(JToken arg)
        {
            JArray arr = new JArray();
            if (SlangLanguageClient.Instance?.WorkspaceOptions != null )
            {
                arr = JArray.Parse(SlangLanguageClient.Instance.WorkspaceOptions.RootElement.ToString());
            }
            return Task.FromResult(arr as object);
        }

        //[JsonRpcMethod("workspace/inlayHint/refresh")]
        //public Task<object> OnWorkspaceInlayHintRefresh()
        //{
        //    int a = 0;
        //    JObject obj = new JObject();
        //    return Task.FromResult(obj as object);
        //}

        //[JsonRpcMethod("workspace/semanticTokens/refresh")]
        //public Task<object> OnWorkspaceSemanticTokensRefresh()
        //{
        //    int a = 0;
        //    JObject obj = new JObject();
        //    return Task.FromResult(obj as object);
        //}

    }
}

