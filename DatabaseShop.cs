using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Rocket.API.Serialisation;

namespace LIGHT
{
    public class DatabaseManagerShop
    {
        // The base code for this class comes from Uconomy itself.
        internal DatabaseManagerShop()
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
        public bool AddItem(int id, string name, decimal cost, bool change)
        {
            bool added = false;
            try
            {
                if(CheckItemExist(id) && !change)
                {
                    return added;
                }
                MySqlConnection Connection = this.createConnection();
                MySqlCommand Command = Connection.CreateCommand();
                if (!change)
                    Command.CommandText = "Insert into `" + LIGHT.Instance.Configuration.Instance.DatabaseItemShop + "` (`id`,`itemname`,`cost`) Values('" + id.ToString() + "', '" + name + "', '" + cost.ToString() + "')";
                else
                    Command.CommandText = "Update `" + LIGHT.Instance.Configuration.Instance.DatabaseItemShop +"` set itemname='" + name + "', cost='" + cost.ToString() + "' where id='" + id.ToString() + "'";
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
        public bool AutoAddItem(int id, string name, bool exist)
        {
            bool added = false;
            try
            {
                MySqlConnection Connection = this.createConnection();
                MySqlCommand Command = Connection.CreateCommand();
                if (!exist)
                    Command.CommandText = "Insert into `" + LIGHT.Instance.Configuration.Instance.DatabaseItemShop + "` (`id`,`itemname`,`cost`) Values('" + id.ToString() + "', '" + name + "', '" + 0 + "')";
                else
                    Command.CommandText = "Update `" + LIGHT.Instance.Configuration.Instance.DatabaseItemShop + "` set itemname='" + name + "' where id='" + id.ToString() + "'";
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
        public bool CheckItemExist(int id)
        {
            bool exist = true;
            try
            {
                MySqlConnection Connection = this.createConnection();
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "select `id` from `" + LIGHT.Instance.Configuration.Instance.DatabaseItemShop + "` where `id` = '" + id.ToString() + "';";
                Connection.Open();
                object obj = Command.ExecuteScalar();
                Connection.Close();
                if (obj == null)
                    exist = false;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return exist;
        }
        public void DeleteEmptyItemRow(int id)
        {
            try
            {
                MySqlConnection Connection = this.createConnection();
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "select `ItemName` from `" + LIGHT.Instance.Configuration.Instance.DatabaseItemShop + "` where `id` = '" + id.ToString() + "';";
                Connection.Open();
                object obj = Command.ExecuteScalar();
                Connection.Close();
                if (obj.ToString() == "" || obj.ToString() == " ")
                {
                    Command.CommandText = "delete from `" + LIGHT.Instance.Configuration.Instance.DatabaseItemShop + "` where id='" + id.ToString() + "'";
                    Connection.Open();
                    Command.ExecuteNonQuery();
                    Connection.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
        public bool AddVehicle(int id, string name, decimal cost, bool change)
        {
            bool added = false;
            try
            {
                if(CheckVehicleExist(id) && !change)
                {
                    return added;
                }
                MySqlConnection Connection = this.createConnection();
                MySqlCommand Command = Connection.CreateCommand();
                if (!change)
                {
                    Command.CommandText = "Insert into `" + LIGHT.Instance.Configuration.Instance.DatabaseVehicleShop + "` (`id`, `vehiclename`, `cost`) VALUES ('" + id.ToString() + "', '" + name + "', '" + cost.ToString() + "')";
                }
                else
                {
                    Command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseVehicleShop + "` set vehiclename='" + name + "', cost='" + cost.ToString() + "' where id='" + id.ToString() + "'";
                }
                Connection.Open();
                int affected = Command.ExecuteNonQuery();
                Connection.Close();
                added = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return added;
        }
        public bool AutoAddVehicle(int id, string name, bool exist)
        {
            bool added = false;
            try
            {
                MySqlConnection Connection = this.createConnection();
                MySqlCommand Command = Connection.CreateCommand();
                if (!exist)
                    Command.CommandText = "Insert into `" + LIGHT.Instance.Configuration.Instance.DatabaseVehicleShop + "` (`id`,`vehiclename`,`cost`) Values('" + id.ToString() + "', '" + name + "', '" + 0 + "')";
                else
                    Command.CommandText = "Update `" + LIGHT.Instance.Configuration.Instance.DatabaseVehicleShop + "` set vehiclename='" + name + "' where id='" + id.ToString() + "'";
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
        public bool CheckVehicleExist(int id)
        {
            bool exist = true;
            try
            {
                MySqlConnection Connection = this.createConnection();
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "select `id` from `" + LIGHT.Instance.Configuration.Instance.DatabaseVehicleShop + "` where `id` = '" + id.ToString() + "';";
                Connection.Open();
                object obj = Command.ExecuteScalar();
                Connection.Close();
                if (obj == null)
                    exist = false;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return exist;
        }
        public void DeleteEmptyVehicleRow(int id)
        {
            try
            {
                MySqlConnection Connection = this.createConnection();
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "select `vehiclename` from `" + LIGHT.Instance.Configuration.Instance.DatabaseVehicleShop + "` where `id` = '" + id.ToString() + "';";
                Connection.Open();
                object obj = Command.ExecuteScalar();
                Connection.Close();
                if (obj.ToString() == "" || obj.ToString() == " ")
                {
                    Command.CommandText = "delete from `" + LIGHT.Instance.Configuration.Instance.DatabaseVehicleShop + "` where id='" + id.ToString() + "'";
                    Connection.Open();
                    Command.ExecuteNonQuery();
                    Connection.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
        public decimal GetItemCost(int id)
        {
            decimal cost = new decimal(0);
            try
            {
                MySqlConnection Connection = this.createConnection();
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "select `cost` from `" + LIGHT.Instance.Configuration.Instance.DatabaseItemShop + "` where `id` = '" + id.ToString() + "'";
                Connection.Open();
                object obj = Command.ExecuteScalar();
                if (obj != null)
                {
                    decimal.TryParse(obj.ToString(), out cost);
                }
                Connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return cost;
        }

        public decimal GetVehicleCost(int id)
        {
            decimal num = new decimal(0);
            try
            {
                MySqlConnection Connection = this.createConnection();
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "select `cost` from `" +  LIGHT.Instance.Configuration.Instance.DatabaseVehicleShop +  "` where `id` = '" + id.ToString() + "';";
                Connection.Open();
                object obj = Command.ExecuteScalar();
                Connection.Close();
                if (obj != null)
                {
                    decimal.TryParse(obj.ToString(), out num);
                }
                
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return num;
        }

        public bool DeleteItem(int id)
        {
            bool added = false;
            try
            {
                MySqlConnection Connection = this.createConnection();
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "delete from `" + LIGHT.Instance.Configuration.Instance.DatabaseItemShop + "` where id='" + id.ToString() + "'";
                Connection.Open();
                object obj = Command.ExecuteNonQuery();
                Connection.Close();
                added = true;
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
            return added;
        }

        public bool DeleteVehicle(int id)
        {
            bool added = false;
            try
            {
                MySqlConnection Connection = this.createConnection();
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "delete from `" + LIGHT.Instance.Configuration.Instance.DatabaseVehicleShop + "` where id='" + id.ToString() + "'";
                Connection.Open();
                object obj = Command.ExecuteNonQuery();
                Connection.Close();
                added = true;
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
            return added;
        }

        public bool SetBuyPrice(int id, decimal cost)
        {
            bool added = false;
            try
            {
                MySqlConnection Connection = this.createConnection();
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseItemShop + "` set `buyback`='" + cost.ToString() + "' where id='" + id.ToString() + "'";
                Connection.Open();
                int affected = Command.ExecuteNonQuery();
                Connection.Close();
                added = true;
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
            return added;
        }

        public decimal GetItemBuyPrice(int id)
        {
            decimal price = new decimal(0);
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = "select `buyback` from `" + LIGHT.Instance.Configuration.Instance.DatabaseItemShop + "` where `id` = '" + id.ToString() + "'" ;
                mySqlConnection.Open();
                object obj = mySqlCommand.ExecuteScalar();
                mySqlConnection.Close();
                if (obj != null)
                {
                    decimal.TryParse(obj.ToString(), out price);
                }              
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
            return price;
        }
        internal void CheckSchema()
        {
            try
            {
                MySqlConnection Connection = this.createConnection();
                MySqlCommand Command = Connection.CreateCommand();
                Connection.Open();
                object test;
                if (LIGHT.Instance.Configuration.Instance.EnableShop)
                {
                    Command.CommandText = "show tables like '" + LIGHT.Instance.Configuration.Instance.DatabaseItemShop + "'";
                    test = Command.ExecuteScalar();
                    if (test == null)
                    {
                        Command.CommandText = "CREATE TABLE `" + LIGHT.Instance.Configuration.Instance.DatabaseItemShop + "` (`id` int(6) NOT NULL,`ItemName` varchar(50) NOT NULL,`Cost` decimal(15,2) NOT NULL DEFAULT '20.00',`buyback` decimal(15,2) NOT NULL DEFAULT '0.00',PRIMARY KEY (`id`))";
                        Command.ExecuteNonQuery();
                    }
                    Command.CommandText = "show tables like '" + LIGHT.Instance.Configuration.Instance.DatabaseVehicleShop + "'";
                    test = Command.ExecuteScalar();
                    if (test == null)
                    {
                        Command.CommandText = "CREATE TABLE `" + LIGHT.Instance.Configuration.Instance.DatabaseVehicleShop + "` (`id` int(6) NOT NULL,`vehiclename` varchar(40) NOT NULL,`cost` decimal(15,2) NOT NULL DEFAULT '100.00',PRIMARY KEY (`id`))";
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