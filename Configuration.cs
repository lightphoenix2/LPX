using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LIGHT
{
    public class Configuration : IRocketPluginConfiguration
    {
        public string DatabaseAddress;
        public string DatabaseUsername;
        public string DatabasePassword;
        public string DatabaseName;
        public string DatabaseTableName;
        public string DatabaseTableGroup;
        public string DatabaseKit;
        public int DatabasePort;
        public bool LPXEnabled;
        public bool IncomeEnabled;
        public bool KitsEnabled;
        public ushort IncomeInterval;        
        public bool AutoRemoveEnabled;        
        public int AutoRemoveDays;
        public bool AutoAddDefault;

        public void LoadDefaults()
        {
            DatabaseAddress = "localhost";
            DatabaseUsername = "root";
            DatabasePassword = "password";
            DatabaseName = "unturned";
            DatabaseTableName = "LPX";
            DatabaseTableGroup = "LPXGroups";
            DatabaseKit = "Kits";
            DatabasePort = 3306;
            LPXEnabled = true;
            IncomeEnabled = true;
            KitsEnabled = false;
            IncomeInterval = 900;
            AutoRemoveEnabled = false;
            AutoRemoveDays = 30;
            AutoAddDefault = false;
        }
    }
}
