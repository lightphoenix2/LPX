using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Rocket.API.Serialisation;

namespace LIGHT
{
    public class DatabaseManagerAuction
    {
        // The base code for this class comes from Uconomy itself.
        internal DatabaseManagerAuction()
        {
            new I18N.West.CP1250(); //Workaround for database encoding issues with mono
            CheckSchema();
        }

        private MySqlConnection createConnection()
        {
            MySqlConnection connection = null;
            try
            {
                if (LIGHT.Instance.Configuration.Instance.DatabasePort == 0) LIGHT.Instance.Configuration.Instance.DatabasePort = 3306;
                connection = new MySqlConnection(String.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4}; Convert Zero Datetime=True;", LIGHT.Instance.Configuration.Instance.DatabaseAddress, LIGHT.Instance.Configuration.Instance.DatabaseName, LIGHT.Instance.Configuration.Instance.DatabaseUsername, LIGHT.Instance.Configuration.Instance.DatabasePassword, LIGHT.Instance.Configuration.Instance.DatabasePort));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return connection;
        }
        public bool AddAuctionItem(int id, string itemid, string itemname, decimal price, decimal shopprice, int quality, string sellerID)
        {
            bool added = false;
            try
            {
                MySqlConnection Connection = this.createConnection();
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "Insert into `" + LIGHT.Instance.Configuration.Instance.DatabaseAuction + "` (`id`,`itemid`,`ItemName`,`Price`,`ShopPrice`,`Quality`,`SellerID`) Values('" + id.ToString() + "', '" + itemid + "', '" + itemname + "', '" + price + "', '" + shopprice + "', '" + quality + "', '"+ sellerID + "')";
                Connection.Open();
                Command.ExecuteNonQuery();
                Connection.Close();
                added = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return added;
        }
        public bool checkAuctionExist(int id)
        {
            bool exist = false;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `id` from `" + LIGHT.Instance.Configuration.Instance.DatabaseAuction + "` where `id` = "+ id +"";
                connection.Open();
                object obj = command.ExecuteScalar();
                connection.Close();
                if (obj != null)
                    exist = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return exist;
        }
        public int GetLastAuctionNo()
        {
            DataTable dt = new DataTable();
            int AuctionNo = 0;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `id` from `" + LIGHT.Instance.Configuration.Instance.DatabaseAuction + "`";
                connection.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                adapter.Fill(dt);
                connection.Close();
                for (int i = 0; i < (dt.Rows.Count); i++)
                {
                    if(dt.Rows[i].ItemArray[0].ToString() != i.ToString()) 
                    {
                        AuctionNo = i;
                        return AuctionNo;
                    }
                }
                AuctionNo = dt.Rows.Count;

            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return AuctionNo;
        }
        public string[] GetAllItemNameWithQuality()
        {
            DataTable dt = new DataTable();
            DataTable quality = new DataTable();
            string[] ItemName = {};
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `ItemName` from `" + LIGHT.Instance.Configuration.Instance.DatabaseAuction + "`";
                connection.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                adapter.Fill(dt);
                command.CommandText = "select `Quality` from `" + LIGHT.Instance.Configuration.Instance.DatabaseAuction + "`";
                adapter = new MySqlDataAdapter(command);
                adapter.Fill(quality);
                connection.Close();
                ItemName = new string[dt.Rows.Count];
                for (int i = 0; i < (dt.Rows.Count); i++)
                {
                    ItemName[i] = dt.Rows[i].ItemArray[0].ToString() + "(" + quality.Rows[i].ItemArray[0].ToString() + ")";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return ItemName;
        }
        public string[] GetAllAuctionID()
        {
            DataTable dt = new DataTable();
            string[] AuctionID = { };
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `id` from `" + LIGHT.Instance.Configuration.Instance.DatabaseAuction + "`";
                connection.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                adapter.Fill(dt);
                connection.Close();
                AuctionID = new string[dt.Rows.Count];
                for (int i = 0; i < (dt.Rows.Count); i++)
                {
                    AuctionID[i] = dt.Rows[i].ItemArray[0].ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return AuctionID;
        }
        public string[] GetAllItemPrice()
        {
            DataTable dt = new DataTable();
            string[] ItemPrice= {};
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `Price` from `" + LIGHT.Instance.Configuration.Instance.DatabaseAuction + "`";
                connection.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                adapter.Fill(dt);
                connection.Close();
                ItemPrice = new string[dt.Rows.Count];
                for (int i = 0; i < (dt.Rows.Count); i++)
                {
                    ItemPrice[i] = dt.Rows[i].ItemArray[0].ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return ItemPrice;
        }
        internal void CheckSchema()
        {
            try
            {
                MySqlConnection Connection = this.createConnection();
                MySqlCommand Command = Connection.CreateCommand();
                Connection.Open();
                object test;
                if (LIGHT.Instance.Configuration.Instance.AllowAuction)
                {
                    Command.CommandText = "show tables like '" + LIGHT.Instance.Configuration.Instance.DatabaseAuction + "'";
                    test = Command.ExecuteScalar();
                    if (test == null)
                    {
                        Command.CommandText = "CREATE TABLE `" + LIGHT.Instance.Configuration.Instance.DatabaseAuction + "` (`id` int(6) NOT NULL,`itemid` int(7) NOT NULL,`ItemName` varchar(56) NOT NULL,`Price` decimal(15,2) NOT NULL DEFAULT '20.00',`ShopPrice` decimal(15,2) NOT NULL DEFAULT '0.00', `Quality` int(3) NOT NULL DEFAULT '50', `SellerID` varchar(20) NOT NULL, PRIMARY KEY (`id`))";
                        Command.ExecuteNonQuery();
                    }
                }
                Connection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
        }
    }
}