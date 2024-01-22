using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SlangClient
{
    [Guid(WindowGuidString)]
    public class SlangToolWindow : ToolWindowPane
    {
        public const string WindowGuidString = "A1E053F3-6F4F-4F83-A72E-D517C6ED4D7C"; 
        public const string Title = "Slang";

        
        public SlangToolWindow() : base()
        {
            Caption = Title;
            Content = new SlangClient.Controls.SlangToolsControl();
        }
    }
}
