using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;
using System.Reflection;
using TB.ComponentModel;

namespace PA.DesktopWebApp
{

    static class ControllerExtension
    {
        internal static void From(this IReflectableType ctrl, object comObj)
        {
            ParseFrom(ctrl, comObj);
        }

        internal static void To(this IReflectableType ctrl, object comObj)
        {
            ParseTo(ctrl, comObj);
        }

        internal static void Call(this IReflectableType ctrl, ScriptCallEventArgs e)
        {
            var method = ctrl.GetType().GetMethod(e.FunctionName);

            if (method != null)
            {
                var args = method.GetParameters().Select(p => e.As(p.ParameterType)).ToArray();
                e.Reply(method.Invoke(ctrl, args));
            }
        }

        private static void ParseFrom(object o, object comObj)
        {
            foreach (var pi in o.GetType().GetProperties())
            {
                var p_obj = GetProperty(comObj, pi.Name);
                var p_val = pi.GetValue(o);

                if (p_obj != null && p_val != null && pi.CanWrite)
                {
                    if (p_obj.CanConvert(pi.PropertyType))
                    {
                        pi.SetValue(o, p_obj.Convert(pi.PropertyType));
                    }
                    else if (pi.CanWrite && typeof(string).IsAssignableFrom(p_obj.GetType()))
                    {
                        pi.SetValue(o, Newtonsoft.Json.JsonConvert.DeserializeObject(p_obj.ToString(), pi.PropertyType));
                    }
                    else if (pi.CanWrite && pi.PropertyType.IsArray)
                    {
                        var l = (int)p_obj.GetType().InvokeMember("length", BindingFlags.GetProperty, null, p_obj, new object[] { });
                        var a = Array.CreateInstance(typeof(object), l);

                        for (int i = 0; i < l; i++)
                        {
                            var v = p_obj.GetType().InvokeMember(i.ToString(), BindingFlags.GetProperty, null, p_obj, null);
                            a.SetValue(v, i);
                        }

                        pi.SetValue(o, a.Convert(pi.PropertyType));

                    }
                    else
                    {
                        ParseFrom(p_val, p_obj);
                    }
                }
            }
        }

        private static void ParseTo(object o, object comObj)
        {
            foreach (var pi in o.GetType().GetProperties())
            {
                var p_obj = GetProperty(comObj, pi.Name);
                var p_val = pi.GetValue(o);

                if (p_obj != null && p_val != null && pi.CanRead)
                {
                    if (p_val.CanConvert(p_obj.GetType()))
                    {
                        SetProperty(comObj, pi.Name, p_val.Convert(p_obj.GetType()));
                    }
                    else if (p_obj.GetType().IsSerializable)
                    {
                        SetProperty(comObj, pi.Name, Newtonsoft.Json.JsonConvert.SerializeObject(p_val));
                    }
                    else if (pi.PropertyType.IsArray)
                    {
                        var l = (int)p_obj.GetType().InvokeMember("length", BindingFlags.GetProperty, null, p_obj, new object[] { });
                        var a = (Array)p_val;

                        // Add data
                        for (int i = 0; i < a.Length; i++)
                        {
                            p_obj.GetType().InvokeMember("push", BindingFlags.InvokeMethod, null, p_obj, new object[] { a.GetValue(i) });
                        }

                        // Cleanup
                        var r = p_obj.GetType().InvokeMember("splice", BindingFlags.InvokeMethod, null, p_obj, new object[] { 0, l });
                    }
                    else
                    {
                        ParseTo(p_val, p_obj);
                    }
                }
            }
        }

        private static object SetProperty(object comObj, string name, params object[] values)
        {
            object o;

            try
            {
                o = comObj.GetType().InvokeMember(name, BindingFlags.SetProperty, null, comObj, values);
            }
            catch
            {
                o = null;
            }

            return o;
        }

        private static object GetProperty(object comObj, string name)
        {
            object o;

            try
            {
                o = comObj.GetType().InvokeMember(name, BindingFlags.GetProperty, null, comObj, null);
            }
            catch
            {
                o = null;
            }

            return o;
        }

    }

}
