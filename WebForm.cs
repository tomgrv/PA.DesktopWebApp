using mshtml;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace PA.DesktopWebApp
{

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public partial class WebForm : Form
    {
        public IList<IReflectableType> Controllers { get; private set; }

        public WebForm(params IReflectableType[] list)
        {
            this.Controllers = new List<IReflectableType>(list);
            InitializeComponent();
        }

        public WebForm()
        {
            this.Controllers = new List<IReflectableType>();
            InitializeComponent();
        }

        private void mainWebControl_DocumentLoaded(object sender, EventArgs e)
        {
            this.Text = this.mainWebControl.Text;

            if (this.mainWebControl.Favicon != null)
            {
                this.mainWebControl.Favicon.MakeTransparent(Color.White);
                var icH = this.mainWebControl.Favicon.GetHicon();
                this.Icon = Icon.FromHandle(icH);
                this.ShowIcon = true;
            }
            else
            {
                this.ShowIcon = false;
            }
        }

        private void mainWebControl_ScriptCall(object sender, ScriptCallEventArgs e)
        {
            var ctrl = this.Controllers.FirstOrDefault(c => c.GetType().Name == e.ControllerName);

            if (ctrl != null)
            {
                if (e.FunctionName != string.Empty)
                {
                    ctrl.From(sender);
                    ctrl.Call(e);
                }

                ctrl.To(sender);
            }
        }

    }

}
