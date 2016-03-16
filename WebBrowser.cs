using mshtml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PA.DesktopWebApp
{
    class WebBrowser : System.Windows.Forms.WebBrowser
    {
        public Bitmap Favicon { get; private set; }

        public WebBrowser()
        {

        }

        private void RemoveComments(ref string data)
        {
            var strPattern = @"/\*(?>(?:(?>[^*]+)|\*(?!/))*)\*/";
            data = Regex.Replace(data, strPattern, string.Empty, RegexOptions.Multiline);
        }

        protected override void OnNavigating(System.Windows.Forms.WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.Scheme == "file" && File.Exists(e.Url.LocalPath))
            {
                var error = string.Empty;

                if (RessourceHandling.Exists(e.Url.Query.Substring(1)))
                {
                    try
                    {
                        HtmlAgilityPack.HtmlDocument hd = new HtmlAgilityPack.HtmlDocument();
                        hd.LoadRessource(e.Url.Query.Substring(1));
                        hd.EmbedScripts();
                        hd.EmbedLinks();
                        hd.ImportDataUrl();
                        File.WriteAllText(e.Url.LocalPath, hd.DocumentNode.OuterHtml);
                        this.Favicon = hd.GetFavicon();
                    }
                    catch(Exception ex)
                    {
                        error = "Error during ressource processing: <em>"+ex.Message+"</em>";
                    }
                }
                else
                {
                    error = "Ressource not found";
                }

                if (error.Length > 0)
                {
                    File.WriteAllText(e.Url.LocalPath, "<html><body><h1>" + error + "</h1></hr><em>PA.DesktopWebApp</em></body></html>");
                }
            }

            base.OnNavigating(e);
        }


    }
}
