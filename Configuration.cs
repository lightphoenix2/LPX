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
        public bool KitCooldownApplyToAll;
        public bool KitCooldownResetOnDeath;
        public ushort IncomeInterval;        
        public bool AutoRemoveEnabled;        
        public int AutoRemoveDays;
        public bool AutoAddDefault;
        public string DatabaseCarOwners;
        public bool AllowCarOwnerShip;
        public bool DriveUnownedCar;
        public double LicencePrice;
        public double FuelPrice;
        public double RepairPrice;
        public bool CanBuyItems;
        public bool CanBuyVehicles;
        public bool CanSellItems;
        public bool QualityCounts;
        public bool EnableShop;
        public bool EnableAutoDBUpdate;
        public string DatabaseItemShop;
        public string DatabaseVehicleShop; 
        public bool SaleEnable;
        public int SalePercentage;
        public int minNextSaleTime;
        public int maxNextSaleTime;
        public int SaleTime;
        public bool AllowAuction;
        public string DatabaseAuction;
        

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
            KitCooldownApplyToAll = true;
            KitCooldownResetOnDeath = false;
            IncomeInterval = 900;
            AutoRemoveEnabled = false;
            AutoRemoveDays = 30;
            AutoAddDefault = false;
            DatabaseCarOwners = "Car";
            AllowCarOwnerShip = true;
            DriveUnownedCar = false;
            LicencePrice = 100.00;
            FuelPrice = 1.3;
            RepairPrice = 2;
            CanBuyItems = true;
            CanBuyVehicles = false;
            CanSellItems = true;
            QualityCounts = true;
            EnableShop = true;
            EnableAutoDBUpdate = false;
            DatabaseItemShop = "ItemShop";
            DatabaseVehicleShop = "VehicleShop";
            SaleEnable = false;
            SalePercentage = 15;
            minNextSaleTime = 600;
            maxNextSaleTime = 1200;
            SaleTime = 3;
            AllowAuction = false;
            DatabaseAuction = "AuctionShop";
        }
    }
}
