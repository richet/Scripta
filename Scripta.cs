using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.Mvc;
using System.Configuration;
using System.IO;

namespace Scripta
{
    public static class ScriptaExtensions
    {
        /// <summary>
        /// Load up a collection of Bundle files
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name">The name of the Bundle section in the Scripta.config</param>
        /// <returns></returns>
        public static ScriptaHelper Scripta(this HtmlHelper source, string name)
        {
            return new ScriptaHelper(source, name);
        }
    }

    public class ScriptaHelper
    {
        private readonly HtmlHelper _htmlHelper;
        private readonly FilesCollection _files;
        private readonly string _version;
        private readonly string _cdn1;
        private readonly string _cdn2;

        public ScriptaHelper(HtmlHelper helper, string name)
        {
            _htmlHelper = helper;

            var debug = false;
            #if DEBUG
            debug = true;
            #endif

            // Load up our config
            var settings = ConfigurationManager.GetSection("scripta/settings") as Scripta.SettingsConfig;
            var scripts = ConfigurationManager.GetSection("scripta/scripts") as Scripta.ScriptsConfig;

            if (settings != null)
            {
                if (!string.IsNullOrEmpty(settings.Version))
                    _version = string.Format("?v={0}", settings.Version);

                // Check if CDNs are in use
                _cdn1 = settings.Cdn1;
                _cdn2 = settings.Cdn2;
            }

            // Search for the Bundle group to add
            var Bundle = (from s in scripts.Bundles.OfType<BundleElement>()
                          where s.Name == name
                          select s).FirstOrDefault();

            // Biff an error if we dont find anything
            if (Bundle == null)
                throw new ArgumentException("Scripta has no idea what you're trying to achieve.");

            // Load up the files based on the current app debug setting
            _files = debug ? Bundle.Debug : Bundle.Prod;
        }

        /// <summary>
        /// Render all of the css files in this section as html links
        /// </summary>
        /// <returns></returns>
        public string Css()
        {
            StringBuilder sb = new StringBuilder();

            foreach (FileElement f in _files)
            {
                if (CheckBrowser(f.Browser))
                    if (f.Src.EndsWith(".css"))
                        sb.AppendFormat("<link href=\"{0}{1}\" rel=\"stylesheet\" type=\"text/css\">", CheckCdn(f.Src), _version);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Render all of js files in this section as html Bundle tags
        /// </summary>
        /// <returns></returns>
        public string Js()
        {
            StringBuilder sb = new StringBuilder();

            foreach (FileElement f in _files)
            {
                if (CheckBrowser(f.Browser))
                    if (f.Src.EndsWith(".js"))
                        sb.AppendFormat("<script type=\"text/javascript\" src=\"{0}{1}\"></script>", CheckCdn(f.Src), _version);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Insert the cdn location if a placeholder has been specified in the file src
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private string CheckCdn(string src)
        {
            if (!string.IsNullOrEmpty(_cdn1) || !string.IsNullOrEmpty(_cdn1))
                return src.Replace("[cdn1]", _cdn1).Replace("[cdn2]", _cdn2);

            return src;
        }

        /// <summary>
        /// Do a cheap ol check on browser type.. This is pretty awful & needs some work :)
        /// </summary>
        /// <param name="browser"></param>
        /// <returns></returns>
        private bool CheckBrowser(string browser)
        {
            if (string.IsNullOrEmpty(browser)) return true;

            if (HttpContext.Current != null)
            {
                var rq = HttpContext.Current.Request;

                switch (browser)
                {
                    case "ie6":
                        return (rq.Browser.Browser == "IE" && rq.Browser.MajorVersion == 6);

                    default:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Test the useragent for the existince of a string.. 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userAgentToTest"></param>
        /// <returns></returns>
        private bool IsUserAgent(HttpRequest request, string userAgentToTest)
        {
            if (request != null)
                if (!string.IsNullOrEmpty(request.UserAgent))
                    return (request.UserAgent.IndexOf(userAgentToTest, StringComparison.OrdinalIgnoreCase) > 0);

            return false;
        }
    }

    /// <summary>
    /// A source file
    /// </summary>
    public class FileElement : ConfigurationElement
    {
        [ConfigurationProperty("src", IsRequired = true, IsKey = true)]
        public string Src
        {
            get { return (string)this["src"]; }
            set { this["src"] = value; }
        }

        [ConfigurationProperty("browser", IsRequired = false, IsKey = true)]
        public string Browser
        {
            get { return (string)this["browser"]; }
            set { this["browser"] = value; }
        }
    }

    /// <summary>
    /// A collection of source files
    /// </summary>
    public class FilesCollection : ConfigurationElementCollection
    {
        public FileElement this[int index]
        {
            get { return base.BaseGet(index) as FileElement; }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                    this.BaseAdd(index, value);
                }
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new FileElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FileElement)element).Src;
        }
    }

    public class BundleElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        /// <summary>
        /// Files to use in production
        /// </summary>
        [ConfigurationProperty("prod", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(FileElement), AddItemName = "file")]
        public FilesCollection Prod
        {
            get { return (FilesCollection)base["prod"]; }
        }

        /// <summary>
        /// Files to use in debug/dev
        /// </summary>
        [ConfigurationProperty("debug", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(FileElement), AddItemName = "file")]
        public FilesCollection Debug
        {
            get { return (FilesCollection)base["debug"]; }
        }
    }

    /// <summary>
    /// A collection of Bundles that we use in our application
    /// </summary>
    public class BundlesCollection : ConfigurationElementCollection
    {
        public BundleElement this[int index]
        {
            get { return base.BaseGet(index) as BundleElement; }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                    this.BaseAdd(index, value);
                }
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new BundleElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((BundleElement)element).Name;
        }
    }


    public class SettingsConfig : ConfigurationSection
    {
        public static SettingsConfig GetConfiguration()
        {
            SettingsConfig configuration = ConfigurationManager.GetSection("scripta/settings") as SettingsConfig;

            if (configuration != null)
                return configuration;

            return new SettingsConfig();
        }

        [ConfigurationProperty("version", IsRequired = false, IsKey = true)]
        public string Version
        {
            get { return (string)this["version"]; }
            set { this["version"] = value; }
        }

        [ConfigurationProperty("cdn1", IsRequired = false, IsKey = true)]
        public string Cdn1
        {
            get { return (string)this["cdn1"]; }
            set { this["cdn1"] = value; }
        }

        [ConfigurationProperty("cdn2", IsRequired = false, IsKey = true)]
        public string Cdn2
        {
            get { return (string)this["cdn2"]; }
            set { this["cdn2"] = value; }
        }
    }

    public class ScriptsConfig : ConfigurationSection
    {
        public static ScriptsConfig GetConfiguration()
        {
            ScriptsConfig configuration = ConfigurationManager.GetSection("scripta/scripts") as ScriptsConfig;

            if (configuration != null)
                return configuration;

            return new ScriptsConfig();
        }

        [ConfigurationProperty("bundles", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(BundleElement), AddItemName = "bundle")]
        public BundlesCollection Bundles
        {
            get { return (BundlesCollection)base["bundles"]; }
        }
    }
}
