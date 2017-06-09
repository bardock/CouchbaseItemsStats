using System.Configuration;

namespace CouchbaseItemsStats
{
    public class ConfigSection : ConfigurationSection
    {
        public static ConfigSection Default
        {
            get { return (ConfigSection)ConfigurationManager.GetSection("couchbaseItemsStats"); }
        }

        [ConfigurationProperty("viewDesignName", IsRequired = true)]
        public string ViewDesignName
        {
            get { return (string)this["viewDesignName"]; }
            set { this["viewDesignName"] = value; }
        }

        [ConfigurationProperty("viewName", IsRequired = true)]
        public string ViewName
        {
            get { return (string)this["viewName"]; }
            set { this["viewName"] = value; }
        }

        [ConfigurationProperty("keyPrefixRegex", IsRequired = true)]
        public string KeyPrefixRegex
        {
            get { return (string)this["keyPrefixRegex"]; }
            set { this["keyPrefixRegex"] = value; }
        }

        [ConfigurationProperty("dev", DefaultValue = false)]
        public bool Dev
        {
            get { return (bool)this["dev"]; }
            set { this["dev"] = value; }
        }
    }
}