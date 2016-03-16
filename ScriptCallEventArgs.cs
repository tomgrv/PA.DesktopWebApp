using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PA.DesktopWebApp
{
    public class ScriptCallEventArgs : EventArgs
    {

        private Action<string, string[]> Callback;
        public string ControllerName { get; private set; }
        public string FunctionName { get; private set; }
        public string[] Parameters { get; private set; }

        internal ScriptCallEventArgs(string ControllerName, string FunctionName, string[] param, Action<string, string[]> callback)
        {
            this.ControllerName = ControllerName;

            if (FunctionName.Length > 0)
            {
                char[] a = FunctionName.ToCharArray();
                a[0] = char.ToUpper(a[0]);
                this.FunctionName = new string(a);
            }
            else
            {
                this.FunctionName = string.Empty;
            }
         

            this.Parameters = param;
            this.Callback = callback;
        }

        public object As(Type t, int index = 0)
        {
            if (t.Equals(typeof(string)))
            {
                return this.Parameters[index];
            }

            return this.Parameters != null && this.Parameters.Length > index ? JsonConvert.DeserializeObject(this.Parameters[index], t) : null;
        }

        public void Reply(params object[] result)
        {
            if (this.FunctionName.Length > 0)
            {
                this.Callback("on" + this.FunctionName, result.Select(o => JsonConvert.SerializeObject(o)).ToArray());
            }
        }
    }
}
