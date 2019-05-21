using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Rocket.API.Serialisation;

namespace LIGHT
{
    public class DatabaseCar
    {
        internal DatabaseCar()
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
        public bool CheckCarExistInDB(string carNo)
        {
            bool exist = false;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `carNo` from `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` where `carNo` = '" + carNo + "'";
                connection.Open();
                object obj = command.ExecuteScalar();
                if (obj == null)
                    exist = false;
                else
                    exist = true;
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return exist;
        }
        public bool AddCar(string carNo, string carID, string carName)
        {
            bool added = false;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "insert into `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` (`carNo`,`carID`,`carName`) values('" + carNo + "', '" + carID + "', " + carName + ")";
                connection.Open();
                command.ExecuteScalar();
                connection.Close();
                added = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return added;
        }

        public int convertLicenceToInt(string noL)
        {
            if (noL.Equals(""))
                return -1;
            if (noL.Equals("A"))
                return 10;
            if (noL.Equals("B"))
                return 11;
            if (noL.Equals("C"))
                return 12;
            decimal i;
            decimal.TryParse(noL, out i);
            return (int)i;
        }

        public void resetLicence(Rocket.Unturned.Player.UnturnedPlayer player, SDG.Unturned.EDeathCause cause, SDG.Unturned.ELimb limb, Steamworks.CSteamID murderer)
        {
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` set `permission` = '' where `steamId` = '" + player.Id + "'";
                connection.Open();
                command.ExecuteScalar();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public string GetLicence(string carID)
        {
            /** Retourne le numero de la licence requise selon le vehicule, "" si le vehicule est interdit **/
            decimal i = Decimal.Parse(carID);
            if (i <= 32) // OffRoader HatchBack Truck Sedan
                return "2";
            if (i == 33) // PoliceCar
                return "5";
            if (i <= 50) // Firetruck Van Roadster
                return "2";
            if (i <= 53) // Ural Apc Humvee
                return "8";
            if (i == 54) // Ambulance
                return "2";
            if (i <= 57) // Ural Apc Humvee
                return "8";
            if (i == 58) // Explorer
                return "0";
            if (i <= 75) // SnowMobile Quad GolfCart
                return "1";
            if (i <= 84) // RaceCar Taxi
                return "2";
            if (i == 85) // Tractor
                return "0";
            if (i == 86) // Bus
                return "2";
            if (i <= 88) // Jeep
                return "8";
            if (i <= 91) // MakeShift
                return "";
            if (i == 92) // SandPiper
                return "4";
            if (i <= 94) // Huey
                return "A";
            if (i <= 96) // Skycrane Otter
                return "4";
            if (i <= 105) // RoundAbout JetSki
                return "3";
            if (i == 106) // Police Helicopter
                return "7";
            if (i == 107) // HummingBird
                return "4";
            if (i == 108) // Police Launch
                return "6";
            if (i == 109) // RainbowCar
                return "C";
            if (i <= 117) // Auto
                return "2";
            if (i <= 119) // Toiler Apc
                return "8";
            if (i <= 121) // Tank
                return "B";
            if (i == 122) // Luggage
                return "0";
            if (i == 123) // Ghost
                return "2";
            if (i == 124) // Dinghy
                return "9";
            if (i <= 132) // Rover
                return "2";
            if (i <= 135) // Annushka Orca Hind
                return "A";
            if (i == 136) // Ural
                return "8";
            if (i == 137) // Tank
                return "B";
            return "";
        }

        public bool AddLicenceToPlayer(string id, string L)
        {
            string perm = "";
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `permission` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` where `steamId` = '" + id + "'";
                connection.Open();
                object obj = command.ExecuteScalar();
                if (obj != null)
                    perm = obj.ToString();
                perm = perm + L;
                command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` set `permission` = '" + perm + "' where `steamId` = '" + id + "'";
                command.ExecuteScalar();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return true;
        }

        public bool CheckLicence(string id, string sLicence)
        {
            string perm = "";
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `permission` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` where `steamId` = '" + id + "'";
                connection.Open();
                object obj = command.ExecuteScalar();
                if (obj == null)
                    return false;
                perm = obj.ToString();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return perm.Contains(sLicence);
        }


        public bool AddOwnership(string carNo, string playerID, string playerName)
        {
            bool added = false;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` set `name` = '" + playerName + "' where `carNo` = '" + carNo + "'";
                connection.Open();
                command.ExecuteScalar();
                command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` set `id` = '" + playerID + "' where `carNo` = '" + carNo + "'";
                command.ExecuteScalar();
                command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` set `Locked` = 'True' where `carNo` = '" + carNo + "'";
                command.ExecuteScalar();
                connection.Close();
                added = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return added;
        }

        public bool RemoveOwnership(string carNo)
        {
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` set `name` = '' where `carNo` = '" + carNo + "'";
                connection.Open();
                command.ExecuteScalar();
                command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` set `id` = '' where `carNo` = '" + carNo + "'";
                command.ExecuteScalar();
                command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` set `Locked` = 'False' where `carNo` = '" + carNo + "'";
                command.ExecuteScalar();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return true;
        }
        public string CheckOwner(string CarNo)
        {
            string ID = "";
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `id` from `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` where `carNo` = '" + CarNo + "'";
                connection.Open();
                object obj = command.ExecuteScalar();
                if (obj != null)
                    ID = obj.ToString();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return ID;
        }
        public string GetOwnerName(string CarNo)
        {
            string Name = "";
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `name` from `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` where `carNo` = '" + CarNo + "'";
                connection.Open();
                object obj = command.ExecuteScalar();
                if (obj != null)
                    Name = obj.ToString();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return Name;
        }
        public string GetOwnerID(string CarNo)
        {
            string ID = "";
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `id` from `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` where `carNo` = '" + CarNo + "'";
                connection.Open();
                object obj = command.ExecuteScalar();
                if (obj != null)
                    ID = obj.ToString();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return ID;
        }
        public string GetCarID(string CarNo)
        {
            string ID = "";
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `CarID` from `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` where `carNo` = '" + CarNo + "'";
                connection.Open();
                object obj = command.ExecuteScalar();
                if (obj != null)
                    ID = obj.ToString();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return ID;
        }
        public string CheckCarDestoryed(string CarNo)
        {
            string CarID = "";
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `carID` from `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` where `carNo` = '" + CarNo + "'";
                connection.Open();
                object obj = command.ExecuteScalar();
                if (obj != null)
                    CarID = obj.ToString();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return CarID;
        }
        public void RemovedDestoryedCar(string CarNo)
        {
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "Delete from `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` where `carNo` = '" + CarNo + "'";
                connection.Open();
                command.ExecuteScalar();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
        public bool AddLockedStatus(string carNo, bool Locked)
        {
            bool added = false;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` set `Locked` = '" + Locked.ToString() + "' where `carNo` = '" + carNo + "'";
                connection.Open();
                command.ExecuteScalar();
                connection.Close();
                added = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return added;
        }
        public bool CheckLockedStatus(string CarNo)
        {
            string Locked = "";
            bool locked = false;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `Locked` from `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` where `carNo` = '" + CarNo + "'";
                connection.Open();
                object obj = command.ExecuteScalar();
                if (obj != null)
                    Locked = obj.ToString();
                connection.Close();
                bool.TryParse(Locked, out locked);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return locked;
        }
        public string[] GetGivenKeys(string CarNo)
        {
            string[] KeyOwners = { };
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `givenKeys` from `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` where `carNo` = '" + CarNo + "'";
                connection.Open();
                object obj = command.ExecuteScalar();
                if (obj != null)
                    KeyOwners = obj.ToString().Split(' ');
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return KeyOwners;
        }
        public bool AddGivenKeys(string CarNo, string TargetPlayerID)
        {
            bool added = false;
            string exist = "";
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `givenKeys` from `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` where `carNo` = '" + CarNo + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                connection.Close();
                if (result == null || result.ToString() == "")
                {
                    connection.Open();
                    command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` set `givenKeys` = '" + TargetPlayerID + "' where `carNo` = '" + CarNo + "'";
                    command.ExecuteScalar();
                    connection.Close();
                }
                else
                {
                    exist = result.ToString().Trim();
                    string combine = exist + " " + TargetPlayerID.Trim();
                    connection.Open();
                    command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` set `givenKeys` = '" + combine + "' where `carNo` = '" + CarNo + "'";
                    command.ExecuteScalar();
                    connection.Close();
                }
                added = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return added;
        }
        public bool RemoveGiveKeysCar(string CarNo, string ID)
        {
            string[] keys = { };
            string newkey = "";
            bool keyexist = false;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `givenKeys` from `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` where `carNo` = '" + CarNo + "'";
                connection.Open();
                object obj = command.ExecuteScalar();
                connection.Close();
                if (obj != null)
                {
                    keys = obj.ToString().Split(' ');
                    for (int x = 0; x < keys.Length; x++)
                    {
                        if (keys[x].Trim() == ID)
                        {
                            keyexist = true;
                        }
                        else
                        {
                            newkey += keys[x] + " ";
                        }
                    }
                    newkey = newkey.Trim();
                }
                else
                {
                    return keyexist;
                }
                if (keyexist)
                {
                    command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` set `givenKeys` = '" + newkey + "' where `carNo` = '" + CarNo + "'";
                    connection.Open();
                    command.ExecuteScalar();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return keyexist;
        }
        public string GetAllOwnedCars(string id)
        {
            DataTable dt = new DataTable();
            string carNo = "";
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `carNo` from `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` where `id` = '" + id + "'";
                connection.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                adapter.Fill(dt);
                connection.Close();
                for (int i = 0; i < (dt.Rows.Count); i++)
                {
                    if (i < (dt.Rows.Count - 1))
                        carNo += dt.Rows[i].ItemArray[0].ToString() + ", ";
                    else
                        carNo += dt.Rows[i].ItemArray[0].ToString();
                }
                carNo = carNo.Trim();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return carNo;
        }
        public string[] GetAllCars()
        {
            DataTable dt = new DataTable();
            string[] carNo = { };
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `carNo` from `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "`";
                connection.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                adapter.Fill(dt);
                carNo = new string[dt.Rows.Count];
                connection.Close();
                for (int i = 0; i < (dt.Rows.Count); i++)
                {
                    if (dt.Rows[i].ItemArray[0].ToString() != "" && dt.Rows[i].ItemArray[0].ToString() != " ")
                    {
                        carNo[i] = dt.Rows[i].ItemArray[0].ToString();
                        carNo[i] = carNo[i].Trim();
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return carNo;
        }
        public bool UpdateCarNo(ushort carNo)
        {
            bool added = false;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` set `carNo` = '" + (carNo - 1).ToString() + "' where `carNo` = '" + carNo.ToString() + "'";
                connection.Open();
                command.ExecuteScalar();
                connection.Close();
                added = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return added;
        }
        internal void CheckSchema()
        {
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                connection.Open();
                object test;
                 command.CommandText = "show tables like '" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "'";
                test = command.ExecuteScalar();

                if (test == null)
                {
                    command.CommandText = "CREATE TABLE `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` (`name` varchar(32),`id` varchar(18),`carNo` varchar(5),`carID` varchar(6),`carName` varchar(30),`givenKeys` varchar(255),`Locked` varchar(5),PRIMARY KEY (`carNo`)) ";
                    command.ExecuteNonQuery();
                }
                else
                {
                    command.CommandText = "SHOW COLUMNS FROM `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` LIKE 'Locked'";
                    test = command.ExecuteScalar();
                    if (test == null)
                    {
                        command.CommandText = "ALTER TABLE `" + LIGHT.Instance.Configuration.Instance.DatabaseCarOwners + "` ADD `Locked` varchar(5) AFTER `givenKeys`";
                        command.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }           
        }
    }
}
