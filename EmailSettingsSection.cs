using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService1
{
    public class EmailSettingsSection : ConfigurationSection
    {
        [ConfigurationProperty("smtpSettings")]
        public SmtpSettingsElement SmtpSettings
        {
            get { return (SmtpSettingsElement)this["smtpSettings"]; }
            set { this["smtpSettings"] = value; }
        }
    }

    public class SmtpSettingsElement : ConfigurationElement
    {
        [ConfigurationProperty("email", IsRequired = true)]
        public string Email
        {
            get { return (string)this["email"]; }
            set { this["email"] = value; }
        }

        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get { return (string)this["password"]; }
            set { this["password"] = value; }
        }

        [ConfigurationProperty("server", IsRequired = true)]
        public string Server
        {
            get { return (string)this["server"]; }
            set { this["server"] = value; }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get { return (int)this["port"]; }
            set { this["port"] = value; }
        }
    }
}
