using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using mshtml;
using System.Reflection;
using System.IO;

using HtmlAgilityPack;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Diagnostics;

namespace PA.DesktopWebApp
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisibleAttribute(true)]
    public partial class WebControl : UserControl
    {
        public string StartPage { get; set; }
        public Bitmap Favicon { get; private set; }

        public event ScriptCallEventHandler ScriptCall;
        public event EventHandler DocumentLoaded;

        public WebControl()
        {
            EmulationControl.SetBrowserEmulationVersion();
            InitializeComponent();
        }

        private void WebControl_Load(object sender, EventArgs e)
        {
            var url = new Uri("file:///" + Path.GetTempFileName() + "?" + this.StartPage);

#if DEBUG
            this.browser.ScriptErrorsSuppressed = false;
            this.browser.IsWebBrowserContextMenuEnabled = true;
#else
            this.browser.ScriptErrorsSuppressed = true;
            this.browser.IsWebBrowserContextMenuEnabled = false;
#endif
            this.browser.AllowWebBrowserDrop = false;
            this.browser.AllowNavigation = true;

            if (!this.DesignMode)
            {
                this.browser.ObjectForScripting = this;
                this.browser.Navigate(url);
                this.browser.ScrollBarsEnabled = true;
            }
        }


        #region Public functions

        private object jsControler;
        private string csControler;

        public object with(object obj)
        {
            this.jsControler = obj;
            return this;
        }

        public object use(string obj)
        {
            this.csControler = obj;
            return this;
        }

        public void call(string function, string p1 = null, string p2 = null, string p3 = null, string p4 = null)
        {
            this.OnCall(function, p1, p2, p3, p4);
        }

        public void sync()
        {
            this.OnCall();
        }

        #endregion

        protected void OnCall(string function = "", params string[] param)
        {
            if (this.ScriptCall != null)
            {
                try
                {
                    this.ScriptCall(this.jsControler, new ScriptCallEventArgs(this.csControler, function, param,
                                               (f, a) => this.jsControler.GetType().InvokeMember(f, BindingFlags.InvokeMethod, null, this.jsControler, a)
                                                )
                                    );
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.Message);
                    MessageBox.Show("ScriptCall error - Please contact support team");
                }
            }
        }

        private void browser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            this.updateControl();

            if (this.DocumentLoaded != null)
            {
                this.DocumentLoaded(this, new EventArgs());
            }
        }

        private void updateControl()
        {
            this.Text = this.browser.DocumentTitle;
            this.Favicon = this.browser.Favicon;
        }


    }
}
