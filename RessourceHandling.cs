using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PA.DesktopWebApp
{
    internal static class RessourceHandling
    {

        internal static void LoadRessource(this HtmlAgilityPack.HtmlDocument htmlDoc, string name)
        {
            htmlDoc.LoadHtml(GetResourceString(name));
        }

        internal static Uri GetAbsoluteUrl(this HtmlAgilityPack.HtmlDocument htmlDoc, string uri)
        {
            if (Uri.IsWellFormedUriString(uri, UriKind.Absolute) && !uri.StartsWith("res://"))
            {
                return new Uri(uri);
            }

            var baseurl = htmlDoc.DocumentNode.SelectSingleNode("//base[@href]");

            if (baseurl != null)
            {
                return new Uri(baseurl.Attributes["href"].Value.TrimEnd('/') + '/' + uri.TrimStart('/'));
            }

            return new Uri("res://" + Path.GetFileName(Assembly.GetEntryAssembly().CodeBase) + '/' + uri.TrimStart('/'));

        }

        internal static void EmbedLinks(this HtmlAgilityPack.HtmlDocument htmlDoc)
        {
            foreach (var link in htmlDoc.DocumentNode.SelectNodes("//link[@rel='stylesheet']"))
            {
                if (link != null)
                {
                    Uri href = htmlDoc.GetAbsoluteUrl(link.Attributes["href"].Value);

                    if (href.Scheme.Contains("res") && Exists(href.LocalPath))
                    {
                        var node = htmlDoc.CreateElement("style");
                        node.Attributes.Add("type", link.Attributes["type"].Value);
                        node.Attributes.Add("media", link.Attributes.Contains("media") ? link.Attributes["media"].Value : "screen");

                        var css = FixDataUrl(GetResourceString(href.LocalPath), u => (new Uri(href, u)).PathAndQuery);
                        node.InnerHtml = "\n<!--\n" + css + "\n-->\n";

                        link.ParentNode.ReplaceChild(node, link);
                    }
                }
            }
        }

        internal static void EmbedScripts(this HtmlAgilityPack.HtmlDocument htmlDoc)
        {
            var scripts = htmlDoc.DocumentNode.SelectNodes("//script[@src]");

            if (scripts != null)
            {
                foreach (var script in scripts)
                {
                    if (script != null)
                    {
                        Uri href = htmlDoc.GetAbsoluteUrl(script.Attributes["src"].Value);

                        if (href.Scheme.Contains("res") && Exists(href.LocalPath))
                        {
                            var node = htmlDoc.CreateTextNode();

                            if (href.LocalPath.Contains(".min."))
                            {
                                node.InnerHtml = "\n" + GetResourceString(href.LocalPath) + "\n";
                            }
                            else
                            {
                                var data = GetResourceString(href.LocalPath);


                                var rules = new string[] {
                                            @"/\*(.*?)\*/",
                                            @"//(.*?)\r?\n",
                                            @"""((\\[^\n]|[^""\n])*)",
                                            @"'((\\[^\n]|[^'\n])*)",
                                            @"/((\\[^\n]|[^/\n])*)",
                                            @"@(""[^""]*"")+"
                                    };

                                data = Regex.Replace(data, rules.Aggregate((s1, s2) => s1 + "|" + s2), me =>
                                {
                                    if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                                    {

                                        return me.Value.StartsWith("//") ? Environment.NewLine : "";
                                    }
                                    // Keep the literal strings
                                    return me.Value;
                                },
                                       RegexOptions.Singleline);

                                node.InnerHtml = data;
                            }

                            script.Attributes.Remove("src");
                            script.AppendChild(node);
                        }
                    }
                }
            }
        }

        internal static Bitmap GetFavicon(this HtmlAgilityPack.HtmlDocument htmlDoc)
        {
            var icon = htmlDoc.DocumentNode.SelectSingleNode(@"//link[contains(@rel, ""icon"")]");

            if (icon != null)
            {
                var name = icon.Attributes["href"].Value;
                if (Exists(name))
                {
                    return new Bitmap(GetResourceStream(name));
                }
            }

            if (Exists("favicon.ico"))
            {
                return new Bitmap(GetResourceStream("favicon.ico"));
            }

            return null;
        }

        private static string FixDataUrl(string css, Func<string, string> fix)
        {
            foreach (Match m in Regex.Matches(css, @"url\(([^\)]+)\)"))
            {
                css = css.Replace(m.Groups[1].Value, fix(m.Groups[1].Value.Trim('\'', '"')));
            }

            return css;
        }

        internal static void FixDataUrl(this HtmlAgilityPack.HtmlDocument htmlDoc, Func<string, string> fix)
        {
            var texts = htmlDoc.DocumentNode.SelectNodes("//text()|//comment()");

            if (texts != null)
            {
                foreach (var text in texts)
                {
                    var innertext = FixDataUrl(text.InnerText, fix);

                    if (text.InnerText != innertext)
                    {
                        text.ParentNode.ReplaceChild(HtmlTextNode.CreateNode(innertext), text);
                    }
                }
            }
        }

        internal static void ImportDataUrl(this HtmlAgilityPack.HtmlDocument htmlDoc)
        {
            htmlDoc.FixDataUrl(url =>
                {
                    string mime = "";
                    Uri href = htmlDoc.GetAbsoluteUrl(url);

                    if (href.Scheme.Contains("res") && Exists(href.LocalPath))
                    {
                        switch (Path.GetExtension(href.LocalPath))
                        {
                            case ".png":
                                mime = "image/png";
                                break;
                            case ".jpg":
                            case ".jpeg":
                                mime = "image/jpeg";
                                break;
                            case ".bmp":
                                mime = "image/bmp";
                                break;
                            case ".gif":
                                mime = "image/gif";
                                break;
                            case ".eot":
                                mime = "application/vnd.ms-fontobject";
                                break;
                            case ".otf":
                                mime = "font/opentype";
                                break;
                            case ".ttf":
                                mime = "application/x-font-ttf";
                                break;
                            case ".woff":
                                mime = "application/font-woff";
                                break;
                            default:
                                break;
                        }

                        if (mime.Length > 0)
                        {
                            return "data:" + mime + ";base64," + Convert.ToBase64String(GetResourceBytes(href.LocalPath));
                        }
                    }

                    return url;
                });

        }

        #region Ressources

        public static byte[] GetResourceBytes(string scriptFile)
        {
            if (Exists(scriptFile))
            {
                return ReadToEnd(GetResourceStream(scriptFile));
            }

            return new byte[0];
        }

        public static string GetResourceName(string scriptFile)
        {
            return Assembly.GetEntryAssembly().GetName().Name + '.' + scriptFile.Replace('/', '.').TrimStart('.');
        }

        public static bool Exists(string scriptFile)
        {
            return Assembly.GetEntryAssembly().GetManifestResourceNames().Contains(GetResourceName(scriptFile));
        }

        public static string GetResourceString(string scriptFile)
        {
            if (Exists(scriptFile))
            {
                StreamReader sr = new StreamReader(GetResourceStream(scriptFile), Encoding.ASCII);
                return sr.ReadToEnd();
            }

            return string.Empty;
        }

        public static Stream GetResourceStream(string scriptFile)
        {
            return Assembly.GetEntryAssembly().GetManifestResourceStream(GetResourceName(scriptFile));
        }

        public static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        #endregion
    }

}
