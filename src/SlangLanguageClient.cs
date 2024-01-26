using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using Newtonsoft.Json;
using StreamJsonRpc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace SlangClient
{
    [ContentType("slang")]
    [Export(typeof(ILanguageClient))]
    public class SlangLanguageClient : ILanguageClient, ILanguageClientCustomMessage2
    {
        [Import]
        ITextDocumentFactoryService DocumentFactoryService { get; set; }
        [Import]
        ITextEditorFactoryService textEditorFactoryService { get; set; }

        FileSystemWatcher ConfigFileWatcher { get; set; }

        private JoinableTaskFactory joinableTaskFactory;

        public delegate void DiagnosticsReadyEvent(string path);

        public event DiagnosticsReadyEvent DiagnoticsReady;

        public Connection _connection = null;
        public JsonRpc _rpc = null;
        public static SlangLanguageClient Instance = null;

        public string Name => "Slang Language Client Extension";

        public IEnumerable<string> ConfigurationSections
        {
            get
            {
                yield return "slang.predefinedMacros";
                yield return "slang.additionalSearchPaths";
                yield return "slang.enableCommitCharactersInAutoCompletion";
            }
        }

        public object InitializationOptions => null;

        public IEnumerable<string> FilesToWatch => null;

        bool ILanguageClient.ShowNotificationOnInitializeFailed => true;

        object ILanguageClientCustomMessage2.MiddleLayer => SlangMiddleLayer.Instance;

        object ILanguageClientCustomMessage2.CustomMessageTarget => SlangServerMessageTarget.Instance;

        public event AsyncEventHandler<EventArgs> StartAsync;
        public event AsyncEventHandler<EventArgs> StopAsync;

        public object WorkspaceOptions = null;
        public string ClangFormatLocation = null;

        public ConcurrentDictionary<string, PublishDiagnosticParams> diagnostics = new ConcurrentDictionary<string, PublishDiagnosticParams>(Environment.ProcessorCount * 2, 32);
        public ConcurrentDictionary<string, ITextView> textViews = new ConcurrentDictionary<string, ITextView>(Environment.ProcessorCount * 2, 32);
        System.Diagnostics.Process process;

        private async Task<string> FindClangFormatAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsShell shellService = (IVsShell)Package.GetGlobalService(typeof(SVsShell));
            if (shellService != null)
            {
                object installDirObj;
                shellService.GetProperty((int)__VSSPROPID.VSSPROPID_InstallDirectory, out installDirObj);

                if (installDirObj != null)
                {
                    var clangformatDir = Path.GetFullPath(Path.Combine(installDirObj.ToString(), @"../../VC/Tools/Llvm/x64/bin/clang-format.exe"));
                    if (File.Exists(clangformatDir))
                    {
                        return clangformatDir;
                    }
                }
            }
            return null;
        }

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            joinableTaskFactory = ThreadHelper.JoinableTaskFactory;
            textEditorFactoryService.TextViewCreated += OnTextViewCreated;
            Instance = this;
            await Task.Yield();
            
            ClangFormatLocation = await FindClangFormatAsync();

            if (SlangWorkspace.Instance != null)
            {
                await SlangWorkspace.Instance.DoSettingsAsync();
            }

            string languageServerPath = Path.GetDirectoryName(typeof(SlangLanguageClient).Assembly.Location);
            languageServerPath = Path.Combine(languageServerPath, "SlangServer", "slangd.exe");
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = languageServerPath;
            info.Arguments = "-vs";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.RedirectStandardError = true;

            process = new System.Diagnostics.Process();
            process.StartInfo = info;
            if (process.Start())
            {
#if DEBUG
                _connection = new Connection(new DebugStream(process.StandardOutput.BaseStream), new DebugStream(process.StandardInput.BaseStream));
#else
                _connection = new Connection(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);
#endif
                return _connection;
            }
            return null;
        }

        public string UriToLocalPath(Uri uri)
        {
            if (uri.IsFile && uri.IsLoopback)
            {
                string localPath = uri.LocalPath;
                if (uri.IsAbsoluteUri)
                {
                    localPath = localPath.TrimStart('/');
                }
                return localPath;
            }
            return null;
        }

        public string GetNewConfigurationPath(Uri fileUri)
        {
            if (fileUri.IsFile)
            {
                string configFilePath = FindConfigFile(UriToLocalPath(fileUri));
                if (configFilePath != null)
                {
                    string currentWatcher = ConfigFileWatcher == null ? "" : Path.Combine(ConfigFileWatcher.Path, ConfigFileWatcher.Filter);
                    if (configFilePath != currentWatcher)
                    {
                        return configFilePath;
                    }
                }
            }
            return null;
        }

        public void UpdateConfiguration(Uri fileUri)
        {
            string configFilePath = GetNewConfigurationPath(fileUri);
            if (configFilePath != null)
            {
                ConfigFileWatcher = new FileSystemWatcher(Path.GetDirectoryName(configFilePath), Path.GetFileName(configFilePath));
                ConfigFileWatcher.Changed += ConfigFileWatcher_Changed;
                ConfigFileWatcher.EnableRaisingEvents = true;
                ReadAndNotifyConfigurationChange();
            }
        }

        private void ReadAndNotifyConfigurationChange()
        {
            try
            {
                dynamic config = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(Path.Combine(ConfigFileWatcher.Path, ConfigFileWatcher.Filter)));
                if (config == null)
                {
                    WorkspaceOptions = null;
                    return;
                }

                // Convert relative paths in searchPath to absolute paths.
                dynamic searchPath = config?["slang.additionalSearchPaths"];
                if (searchPath != null && searchPath.Count != 0)
                {
                    for (int i = 0; i < searchPath.Count; i++)
                    {
                        try
                        {
                            var pathStr = searchPath[i].ToString();
                            if (!Path.IsPathRooted(pathStr))
                            {
                                pathStr = Path.GetFullPath(Path.Combine(ConfigFileWatcher.Path, pathStr));
                                searchPath[i] = pathStr;
                            }
                        }
                        catch (Exception)
                        { }
                    }
                    config["slang.additionalSearchPaths"] = searchPath;
                }

                dynamic clangFormatLocation = config?["slang.format.clangFormatLocation"];
                if (clangFormatLocation == null && ClangFormatLocation!= null)
                {
                    // Use clang-format that comes with visual studio.
                    config["slang.format.clangFormatLocation"] = ClangFormatLocation;
                }
                WorkspaceOptions = config;

                // Send the config to language server.
                DidChangeConfigurationParams configParams = new DidChangeConfigurationParams();
                configParams.Settings = WorkspaceOptions;
                Task task = Task.Run(async () => await _rpc.NotifyAsync(Methods.WorkspaceDidChangeConfigurationName, configParams));
            }
            catch (Exception)
            {
                WorkspaceOptions = null;
            }
        }

        private void ConfigFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            ReadAndNotifyConfigurationChange();
        }

        private string FindConfigFile(string path)
        {
            string currentPath = Path.GetDirectoryName(path);
            string filePath = Path.Combine(currentPath, "slangdconfig.json");
            if (File.Exists(filePath))
            {
                return filePath;
            }
            if (currentPath == Path.GetPathRoot(currentPath))
            {
                return null;
            }
            return FindConfigFile(currentPath);
        }

        public ITextBuffer GetTextBufferFromPath(string path)
        {
            foreach (var x in textViews)
            {
                if (x.Key == path)
                {
                    return x.Value.TextBuffer;
                }
            }
            return null;
        }

        public string GetPathFromView(ITextView view)
        {
            ITextDocument textDocument = null;
            if (DocumentFactoryService.TryGetTextDocument(view.TextBuffer, out textDocument))
            {
                Uri fileUri;
                if (Uri.TryCreate(textDocument.FilePath, UriKind.Absolute, out fileUri))
                {
                    return Uri.UnescapeDataString(fileUri.AbsoluteUri);
                }
            }
            return null;
        }

        public void RaiseDiagnosticsReady(string path)
        {
            DiagnoticsReady?.Invoke(path);
        }


        private void OnTextViewCreated(object sender, TextViewCreatedEventArgs args)
        {
            ITextDocument textDocument = null;
            if (DocumentFactoryService.TryGetTextDocument(args.TextView.TextBuffer, out textDocument))
            {
                Uri fileUri;
                if (Uri.TryCreate(textDocument.FilePath, UriKind.Absolute, out fileUri))
                {
                    textViews.TryAdd(Uri.UnescapeDataString(fileUri.AbsoluteUri), args.TextView);

                    args.TextView.Closed += TextView_Closed;
                }
            }
        }

        private void TextView_Closed(object sender, EventArgs e)
        {
            ITextView view = (ITextView)sender;
            view.Closed -= TextView_Closed;
            var item = textViews.FirstOrDefault(kvp => kvp.Value == view);
            if ( item.Value != null)
            { 
                ITextView removeView;
                textViews.TryRemove(item.Key, out removeView);
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            return;
        }

        public async Task OnLoadedAsync()
        {
            await StartAsync.InvokeAsync(this, EventArgs.Empty);
        }

        public Task OnServerInitializeFailedAsync(Exception e)
        {
            return Task.CompletedTask;
        }

        public Task OnServerInitializedAsync()
        {
            return Task.CompletedTask;
        }

        Task<InitializationFailureContext> ILanguageClient.OnServerInitializeFailedAsync(ILanguageClientInitializationInfo initializationState)
        {
            throw new NotImplementedException();
        }

        Task ILanguageClientCustomMessage2.AttachForCustomMessageAsync(JsonRpc rpc)
        {
            _rpc = rpc;
            return Task.CompletedTask;
        }
    }
}
