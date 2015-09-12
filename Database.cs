using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace LIGHT
{
    public class DatabaseManager
    {
        
        internal DatabaseManager()
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
                connection = new MySqlConnection(String.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", LIGHT.Instance.Configuration.Instance.DatabaseAddress, LIGHT.Instance.Configuration.Instance.DatabaseName, LIGHT.Instance.Configuration.Instance.DatabaseUsername, LIGHT.Instance.Configuration.Instance.DatabasePassword, LIGHT.Instance.Configuration.Instance.DatabasePort));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return connection;
        }
        public List<string> getMembers(string group)
        {
            string members = "";
            List<string> Members = new List<string>();
            DataTable dt = new DataTable();
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `steamID` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` where `group` = '" + group + "'"; 
                connection.Open();              
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                adapter.Fill(dt);
                connection.Close();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    members += dt.Rows[i].ItemArray[0].ToString() + " ";
                }
                members.Trim();
                Members = members.Split(' ').ToList();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return Members;
        }
        public List<string> getParentGroup(string group)
        {
            string PGroup = "";
            List<string> Pgroups = new List<string>();
            DataTable dt = new DataTable();
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `parentgroup` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup+ "` where `name` = '" + group + "'";
                connection.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                adapter.Fill(dt);
                connection.Close();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    PGroup += dt.Rows[i].ItemArray[0].ToString() + " ";
                }
                PGroup.Trim();
                Pgroups = PGroup.Split(' ').ToList();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return Pgroups;
        }
        public string[] getPermission(string id)
        {
            string name ="";
            string[] permission = {};
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `group` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` where `steamId` = '" + id.ToString() + "'";
                connection.Open();
                object group = command.ExecuteScalar();
                if (group != null)
                    name = group.ToString();
                else
                    name = "default";
                command.CommandText = "select `permission` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` where `name` = '" + name + "';";
                group = command.ExecuteScalar();
                if (group != null)
                {
                    permission = (group.ToString()).Split(' ');
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return permission;
        }
        public string[] getGroupPermission(string group)
        {
            string[] permission = { };
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `permission` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` where `name` = '" + group + "'";
                connection.Open();
                object obj = command.ExecuteScalar();
                if (obj != null)
                {
                    permission = (obj.ToString()).Split(' ');
                }
                connection.Close();
                string parentgroup = string.Join(" ", LIGHT.Instance.Database.getParentGroup(group).ToArray()).Trim();           
                if(parentgroup != "")
                {
                    string[] pgroups = parentgroup.Split(' ');
                    string parentgroup2 = "";
                    for (int i = 0; i < pgroups.Length; i++)
                    {
                        if (string.Join(" ",LIGHT.Instance.Database.getParentGroup(pgroups[i]).ToArray()).Trim() != "")
                            parentgroup2 += " " + string.Join(" ", LIGHT.Instance.Database.getParentGroup(pgroups[i]).ToArray());
                    }
                    if(parentgroup2!= "")
                        parentgroup += parentgroup2;
                    parentgroup = parentgroup.Trim();
                    pgroups = parentgroup.Split(' ');
                    string[] pgroupPerm = new string[pgroups.Length];
                    string newPermission = "";
                    for (int i = 0; i < pgroups.Length; i++)
                    {
                        pgroupPerm[i] = getGroupPermissionString(pgroups[i]);
                        pgroupPerm[i].Trim();
                    }
                    for(int i = 0; i < pgroupPerm.Length; i ++)
                    {
                        newPermission += " "+ pgroupPerm[i];                
                    }
                    newPermission = newPermission.Trim();
                    string oldpermission = obj.ToString();
                    oldpermission = oldpermission.Trim();
                    string combine = oldpermission + " " + newPermission;
                    permission = (oldpermission +" " +newPermission).Split(' ');
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return permission;
        }
        public string getGroupPermissionString(string group)
        {
            string permission = "";
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `permission` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` where `name` = '" + group + "'";
                connection.Open();
                object obj = command.ExecuteScalar();
                if (obj != null)
                {
                    permission = obj.ToString();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return permission;
        }
        public void AddUserIntoGroup(string id, string group)
        {
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `steamId` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` WHERE `steamId` = '" + id + "'";
                connection.Open();
                object result = command.ExecuteScalar();              
                connection.Close();
                if (result == null)
                {
                    command.CommandText = "insert into `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` (`steamId`,`group`) values('" + id + "', '" + group + "')";
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                else
                {
                    command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` set `group` = '" + group + "' where `steamId` = '" + id + "'";
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

        }
        public bool AddGroup(string group, string income)
        {
            decimal pay = decimal.Parse(income);
            bool added = false;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "insert into `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` (`name`,`income`) values('" + group + "', '" + income + "')";
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                added = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                added = false;
            }
            return added;
        }
        public bool RemoveGroup(string group)
        {
            bool removed = false;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "Delete from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` (`name`) where `name` = '" + group + "'";
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                removed = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                removed = false;
            }
            return removed;
        }
        public bool RemoveUser(string Id)
        {
            bool added = false;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "Delete from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` where `steamId` = '" + Id + "'";
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                added = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                added = false;
            }
            return added;
        }
        public bool AddGroupFreeItem(string group, string id)
        {
            bool added = false;
            string[] ItemID = GetGroupFreeItem(group);
            string Items = "";
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                if (ItemID.Length < 1)
                    command.CommandText = "insert into `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` (`freeitem`) values('" + id + "')";
                else
                {
                    for (int i = 0; i < ItemID.Length; i++ )
                    {
                        Items += (" " + ItemID[i]); 
                    }
                    Items.Trim();
                    command.CommandText = "insert into `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` (`freeitem`) values('" + Items + "')";
                }
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                added = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                added = false;
            }
            return added;
        }
        public bool AddPermissionIntoGroup(string permi, string group)
        {
            bool results = false;
            try
            {
                string permissions = "";
                string[] permission = { };
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `permission` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` WHERE `name` = '" + group + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                connection.Close();
                if (result != null)
                {
                    permissions = result.ToString();
                    permission = permissions.Split(' ');
                }
                else
                    return false;
                if(permi.Contains("color."))
                {
                    string newpermissions = "";
                    for (int i = 0; i < (permission.Length); i++)
                    {
                        if (newpermissions.Length > 0 && !(permission[i].Contains("color.")))
                            newpermissions += " " + permission[i];
                        else if (!(permission[i].Contains("color.")))
                            newpermissions += permission[i];
                        Logger.Log(permission[i]);
                        if (permission[i].Contains("color."))
                        {                           
                            if (i == 0)
                                newpermissions += permi;
                            else
                                newpermissions += " " + permi;
                        }
                        else if (newpermissions.Length < 1)
                            newpermissions = permi;
                        else if(!(permissions.Contains("color.")))
                            newpermissions += " " + permi;                        
                    }
                    command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` set `permission` = '" + newpermissions + "' where `name` = '" + group + "'";
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    results = true;
                    return results;
                }
                if (permissions.Length < 1)
                {
                    command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` set `permission` = '" + permi + "' where `name` = '" + group + "'";
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                else
                {
                    permissions += " " + permi;
                    command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` set `permission` = '" + permissions + "' where `name` = '" + group + "'";
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                results = true;
            }
            catch (Exception ex)
            {
                results = false;
                Logger.LogException(ex);
            }
            return results;
        }
        public bool RemovePermissionFromGroup(string group, string permi, string id)
        {
            bool results = false;
            try
            {
                string permissions = "";
                string[] permission = getPermission(id);
                for (int i = 0; i < permission.Length; i++)
                {
                    if(permission[i].Contains(permi))
                        permission[i].Remove(i);
                    if (i != (permission.Length - 1))
                        permissions += permission[i] + " ";
                    else
                        permissions += permission[i];
                }
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` set `permission` = '" + permissions + "' where `name` = '" + group + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                connection.Close();
                results = true;
            }
            catch (Exception ex)
            {
                results = false;
                Logger.LogException(ex);
            }
            return results;
        }
        public bool CheckGroup(string group)
        {
            bool contain = false;
            string exist= "";
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `name` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` where `name` = '" + group + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                if (result != null) exist = result.ToString();
                connection.Close();
                if (exist == "")
                    contain = false;
                else
                    contain = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return contain;
        }
        public bool AddParentGroup(string group, string parentgroup)
        {
            bool added = false;
            string exist = "";
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `parentgroup` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` where `name` = '" + group + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                connection.Close();
                if (result == null)
                {
                    connection.Open();
                    command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` set `parentgroup` = '" + parentgroup + "' where `name` = '" + group + "'";
                    command.ExecuteScalar();
                    connection.Close();                   
                }
                else
                {
                    exist = result.ToString().Trim();
                    string combine = exist + " " + parentgroup.Trim();
                    connection.Open();
                    command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` set `parentgroup` = '" + combine + "' where `name` = '" + group + "'";
                    command.ExecuteScalar();
                    connection.Close();
                }
                added = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return added = false;
            }
            return added;
        }
        public string CheckUserGroup(string id)
        {
            string exist = "";
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `group` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` where `steamId` = '" + id + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                connection.Close();
                if (result != null) exist = result.ToString();
                else return "default";              
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return exist;
        }
        public DataTable GetGroup()
        {
            DataTable dt = new DataTable();
            try
            {               
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `name` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "`";
                connection.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);             
                adapter.Fill(dt);
                connection.Close();               
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return dt;
        }
        public decimal[] GetAllGroupIncome()
        {
            DataTable dt = new DataTable();
            decimal[] stringArr = new decimal[50];
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `income` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "`";
                connection.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                adapter.Fill(dt);
                connection.Close();             
                for (int i = 0; i < (dt.Rows.Count); i++)
                {
                    stringArr[i] += decimal.Parse(dt.Rows[i].ItemArray[0].ToString());
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return stringArr;
        }
        public string[] GetFreeItem(string id)
        {
            string group = CheckUserGroup(id);
            if (group == "")
                group = "default";
            string exist="";
            string[] stringArr = new string[50];
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `freeitem` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` where name = '" + group + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                if (result != null) exist = result.ToString();
                stringArr = exist.Split(' ');
                connection.Close();              
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return stringArr;
        }
        public string[] GetGroupFreeItem(string group)
        {
            string exist = "";
            string[] stringArr = new string[50];
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `freeitem` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` where name = '" + group + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                if (result != null) exist = result.ToString();
                stringArr = exist.Split(' ');
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return stringArr;
        }
        public bool SetGroupIncome(string group, string income)
        {
            decimal pay = decimal.Parse(income);
            bool added = false;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` (`income`) values('" + income + "')";
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                added = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                added = false;
            }
            return added;
        }
        internal void CheckSchema()
        {
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "show tables like '" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "'";
                connection.Open();
                object test = command.ExecuteScalar();

                if (test == null)
                {
                    command.CommandText = "CREATE TABLE `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` (`steamId` varchar(32) NOT NULL,`group` varchar(32),PRIMARY KEY (`steamId`)) ";
                    command.ExecuteNonQuery();
                }
                command.CommandText = "show tables like '" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "'";
                test = command.ExecuteScalar();

                if (test == null)
                {
                    command.CommandText = "CREATE TABLE `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` (`name` varchar(32) NOT NULL,`permission` varchar(568),`income` decimal(10) NOT NULL DEFAULT 0.00,`freeitem` int(8),`parentgroup` varchar(32),PRIMARY KEY (`name`)) ";
                    command.ExecuteNonQuery();
                    command.CommandText = "insert into `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` (`name`,`income`) values('default', 10)";
                    command.ExecuteNonQuery();
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

