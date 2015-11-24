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
        public int DatabasePort;
        public bool LPXEnabled;
        public bool IncomeEnabled;
        public ushort IncomeInterval;
        public string defaultUserGroup;

        public void LoadDefaults()
        {
            DatabaseAddress = "localhost";
            DatabaseUsername = "root";
            DatabasePassword = "password";
            DatabaseName = "unturned";
            DatabaseTableName = "LPX";
            DatabaseTableGroup = "LPXGroups";
            defaultUserGroup = "default";
            DatabasePort = 3306;
            LPXEnabled = true;
            IncomeEnabled = true;
            IncomeInterval = 900;
        }
    }
}
