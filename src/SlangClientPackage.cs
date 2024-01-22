using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

using Task = System.Threading.Tasks.Task;

namespace SlangClient
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    /// 
    [ProvideToolWindow(typeof(SlangToolWindow), Style = VsDockStyle.Tabbed, DockedWidth = 300, Window = "DocumentWell", Orientation = ToolWindowOrientation.Left)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(SlangClientPackage.PackageGuidString)]
    [ProvideFileExtensionMapping("{E23E32ED-3467-4401-A364-1352666A3502}", "Slang Editor", typeof(IVsEditorFactory), SlangClientPackage.PackageGuidString, 100)]
    public sealed class SlangClientPackage : AsyncPackage
    {
        /// <summary>
        /// SlangClientPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "4b6eface-6d4b-42a0-9322-a2785c0935a7";

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        /// 
        protected override Task OnAfterPackageLoadedAsync(CancellationToken cancellationToken)
        {
            return base.OnAfterPackageLoadedAsync(cancellationToken);
        }

        protected override void OnLoadOptions(string key, Stream stream)
        {
            base.OnLoadOptions(key, stream);
        }
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await SlangToolWindowCommand.InitializeAsync(this);
        }
        #endregion
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    class ProvideFileExtensionMapping : RegistrationAttribute
    {
        private readonly string _name, _id, _editorGuid, _package;
        private readonly int _sortPriority;

        public ProvideFileExtensionMapping(string id, string name, object editorGuid, string package, int sortPriority)
        {
            _id = id;
            _name = name;
            if (editorGuid is Type)
            {
                _editorGuid = ((Type)editorGuid).GUID.ToString("B");
            }
            else
            {
                _editorGuid = editorGuid.ToString();
            }
            _package = package;
            _sortPriority = sortPriority;
        }

        public override void Register(RegistrationContext context)
        {
            using (Key mappingKey = context.CreateKey("FileExtensionMapping\\" + _id))
            {
                mappingKey.SetValue("", _name);
                mappingKey.SetValue("DisplayName", _name);
                mappingKey.SetValue("EditorGuid", _editorGuid);
                mappingKey.SetValue("LogViewID", _editorGuid);
                mappingKey.SetValue("Package", _package);
                mappingKey.SetValue("SortPriority", _sortPriority);
            }
        }

        public override void Unregister(RegistrationAttribute.RegistrationContext context)
        {
        }
    }

}
