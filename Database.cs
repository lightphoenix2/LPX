using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Rocket.API.Serialisation;

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
                connection = new MySqlConnection(String.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4}; Convert Zero Datetime=True;", LIGHT.Instance.Configuration.Instance.DatabaseAddress, LIGHT.Instance.Configuration.Instance.DatabaseName, LIGHT.Instance.Configuration.Instance.DatabaseUsername, LIGHT.Instance.Configuration.Instance.DatabasePassword, LIGHT.Instance.Configuration.Instance.DatabasePort));
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
        public string getParentGroup(string group)
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
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return PGroup;
        }
        public string[] getParentGroupString(string group)
        {
            string PGroup = "";
            string[] Pgroups = {};
            DataTable dt = new DataTable();
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `parentgroup` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` where `name` = '" + group + "'";
                connection.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                adapter.Fill(dt);
                connection.Close();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    PGroup += dt.Rows[i].ItemArray[0].ToString() + " ";
                }
                PGroup.Trim();
                Pgroups = PGroup.Split(' ');
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
        public string[] getPermission2(string group)
        {
            string[] permission = { };
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();               
                if (group == null)
                    group = "default";
                command.CommandText = "select `permission` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` where `name` = '" + group + "';";
                connection.Open();
                object  obj = command.ExecuteScalar();
                connection.Close();
                if (group != null)
                {
                    permission = (obj.ToString()).Split(' ');
                }              
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return permission;
        }
        public bool checkPermissionCopy(string group, string permission)
        {
            string[] permissions = { };
            bool CopyDetected = false;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `permission` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` where `name` = '" + group + "';";
                connection.Open();
                object perm = command.ExecuteScalar();
                if (perm != null)
                {
                    permissions = (perm.ToString()).Split(' ');
                }
                connection.Close();
                for(int x = 0; x < permissions.Length; x ++)
                {
                    if (permissions[x] == permission)
                    {
                        CopyDetected = true;
                    }
                    else
                        CopyDetected = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return CopyDetected;
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
                string parentgroup = string.Join(" ", LIGHT.Instance.Database.getParentGroupString(group)).Trim();
                string AllParentGroups = "";
                string[] pgroups;
                if(parentgroup != "")
                {                   
                    while (parentgroup.Trim() != " " && parentgroup.Trim() != "")
                    {
                        pgroups = parentgroup.Split(' ');
                        if (parentgroup.StartsWith(" "))
                            AllParentGroups += parentgroup;
                        else
                            AllParentGroups += " " + parentgroup;
                        parentgroup = "";
                        for (int i = 0; i < pgroups.Length; i++)
                        {
                            string[] ParentGroup = LIGHT.Instance.Database.getParentGroupString(pgroups[i]);
                            for (int x = 0; x < ParentGroup.Length; x++)
                            {
                                if(parentgroup == "")
                                    parentgroup += ParentGroup[x].Trim();
                                else
                                    parentgroup += " " + ParentGroup[x].Trim();
                            }
                        }
                    }
                    AllParentGroups = AllParentGroups.Trim();
                    pgroups = AllParentGroups.Split(' ');
                    string[] pgroupPerm = new string[pgroups.Length];
                    string newPermission = "";
                    string[] FindColor;                  
                    for (int i = 0; i < pgroups.Length; i++)
                    {
                        string NoColor = "";
                        FindColor = getParentGroupPermissionString(pgroups[i]).Split(' ');
                        for(int y = 0; y < FindColor.Length; y++)
                        {
                            if (!(FindColor[y].Contains("color.")))
                            {
                                if(FindColor[y] != "")
                                    NoColor += FindColor[y] + " ";
                            }
                            pgroupPerm[i] = NoColor;
                        }
                        pgroupPerm[i].Trim();
                    }
                    for (int i = 0; i < pgroupPerm.Length; i++)
                    {
                        if(pgroupPerm[i] != "")
                            newPermission += " " + pgroupPerm[i].Trim();
                    }
                    newPermission = newPermission.Trim();
                    string oldpermission = obj.ToString();
                    oldpermission = oldpermission.Trim();
                    permission = (oldpermission +" " +newPermission).Split(' ');
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return permission;
        }
        public string getParentGroupPermissionString(string group)
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
        public bool AddGroup(string group, string income,string parentgroup, string updategroup, int Updatetime, bool autoUpdate)
        {
            decimal pay = decimal.Parse(income);
            int AutoUpdate = 0;
            if (autoUpdate)
                AutoUpdate = 1;
            bool added = false;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "insert into `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` (`name`,`income`,`parentgroup`,`updategroup`,`updatetime`, `updateenable`) values('" + group + "', '" + income + "', '" + parentgroup + "', '" + updategroup + "', " + Updatetime + ", " + AutoUpdate + ")";
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
        public bool RemovePermissionFromGroup(string group, string permi)
        {
            bool results = false;
            try
            {
                string permissions = "";
                string[] permission = getPermission2(group);
                for (int i = 0; i < permission.Length; i++)
                {
                    if (permission[i] != permi)
                        permissions += permission[i] + " ";
                }
                permissions = permissions.Trim();
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
        public void HoursOnline(string id, DateTime logouttime)
        {
            DateTime loginTime = DateTime.Now;
            decimal HoursOnline = 0M, newHoursOnline = 0M;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `lastlogin` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` where `steamId` = '" + id + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                if (result != null) loginTime = (DateTime)result;
                connection.Close();
                newHoursOnline = (decimal)(logouttime - loginTime).TotalHours;
                command.CommandText = "select `hours` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` where `steamId` = '" + id + "'";
                connection.Open();
                result = command.ExecuteScalar();
                if (result != null) HoursOnline = int.Parse(result.ToString());
                connection.Close();
                newHoursOnline += HoursOnline;
                command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` set `hours` = " + newHoursOnline + " where `steamId` = '" + id + "'";
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
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
            string group = "";
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `group` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` where `steamId` = '" + id + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                connection.Close();
                if (result != null) group = result.ToString();
                else return "default";              
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return group;
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
                command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` set  `income` = '" + income + "' where `name` = '" + group +"'";
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
        public void LastLogin(string id)
        {
            MySqlConnection connection = createConnection();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` set `lastlogin` = now() where `steamId` = '" + id + "'";
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }
        public void AutoRemove()
        {
            DataTable dt = new DataTable();
            string Date = "";
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `lastlogin`,`steamId` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "`";
                connection.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                adapter.Fill(dt);
                connection.Close();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Date = dt.Rows[i].ItemArray[0].ToString();
                    Date.Trim();
                    Date.Replace("-", "/");
                    if ((DateTime.Now - (Convert.ToDateTime(Date))).Days > LIGHT.Instance.Configuration.Instance.AutoRemoveDays)
                    {     
                        string[] Permission = getPermission(dt.Rows[i].ItemArray[1].ToString());
                        bool immunity = false;
                        for (int x = 0; x < Permission.Length; x ++ )
                        {
                            if (Permission[x].ToLower() == "lpx.autoremoveimmunity")
                                immunity = true;
                        }
                        if(!immunity)
                            RemoveUser(dt.Rows[i].ItemArray[1].ToString());
                    }              
                }            
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
        public bool CheckEnablePromotion(string id)
        {
            bool enable = false;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `updateenable` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` where `name` = '" + CheckUserGroup(id) + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                connection.Close();
                if (result != null) bool.TryParse(result.ToString(), out enable);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return enable;
        }
        public decimal GetTotalOnlineHours(string id)
        {
            decimal hours = 0m;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `hours` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` where `steamId` = '" + id + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                connection.Close();
                if (result != null)
                {
                    decimal.TryParse(result.ToString(), out hours);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return hours;
        }
        public decimal GetUpdateTime(string id)
        {
            decimal hours = 0m;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `updatetime` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` where `name` = '" + CheckUserGroup(id) + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                connection.Close();
                if (result != null)
                {
                    decimal.TryParse(result.ToString(), out hours);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return hours;
        }
        public string GetUpdateGroup(string id)
        {
            string group = "";
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `updategroup` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` where `name` = '" + CheckUserGroup(id) + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                connection.Close();
                if (result != null)
                {
                    group = result.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return group;
        }
        public bool SetUpdateGroup(string group, string updategroup)
        {
            bool added = false;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` set `updategroup` = '" + updategroup + "' where `name` = '" + group + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                connection.Close();
                added = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return added;
        }
        public bool SetUpdateTime(string group, int Time)
        {
            bool added = false;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` set `updatetime` = " + Time + " where `name` = '" + group + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                connection.Close();
                added = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return added;
        }
        public bool SetEnableUpdate(string group, bool enable)
        {
            bool added = false;
            try
            {
                int enabled = 0;
                if (enable)
                    enabled = 1;
                else
                    enabled = 0;
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` set `updateenable` = " + enabled + " where `name` = '" + group + "'";
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
        public bool CheckIfUserInAnyGroup(string id)
        {
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `steamId` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` WHERE `steamId` = '" + id + "'";
                connection.Open();
                object result = command.ExecuteScalar();
                connection.Close();
                if (result == null) return false;
                else return true;
            }
            catch (Exception ex)
            {              
                Logger.LogException(ex);
                return false;
            }

        }
        public string GetColor(string group)
        {
            string[] permission = { };
            string Color = "white";
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
                for(int x = 0; x < permission.Length; x++)
                {
                    if (permission[x].StartsWith("color."))
                    {
                        Color = "";
                        char[] buff = permission[x].ToCharArray();
                        for(int y = permission[x].IndexOf(".") + 1; y < permission[x].Length; y ++)
                        {
                            Color += buff[y];
                        }
                        Color.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return Color;
        }
        public uint? Cooldown(string group)
        {
            uint cooldown = 0;
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "select `cooldown` from `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` where `name` = '" + group + "'";
                connection.Open();
                object obj = command.ExecuteScalar();
                if (uint.TryParse(obj.ToString(), out cooldown) == false)
                    cooldown = 0;
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return cooldown;
        }
        public void SetSteamName(string id, string steamName)
        {
            try
            {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "update `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` set `SteamName` = '" + steamName + "' where `steamId` = '" + id + "'";
                connection.Open();
                command.ExecuteScalar();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
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
                    command.CommandText = "CREATE TABLE `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` (`steamId` varchar(32) NOT NULL,`SteamName` varchar(46),`group` varchar(32),`lastlogin` timestamp NOT NULL DEFAULT '0000-00-00 00:00:00' ON UPDATE CURRENT_TIMESTAMP,`hours` decimal(10) NOT NULL DEFAULT 0, PRIMARY KEY (`steamId`)) ";
                    command.ExecuteNonQuery();
                }
                else
                {                    
                    command.CommandText = "SHOW COLUMNS FROM `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` LIKE 'SteamName'";
                    test = command.ExecuteScalar();
                    if (test == null)
                    {
                        command.CommandText = "ALTER TABLE `" + LIGHT.Instance.Configuration.Instance.DatabaseTableName + "` ADD `SteamName` VARCHAR(46) AFTER `steamId`";
                        command.ExecuteNonQuery();
                    }
                }
                command.CommandText = "show tables like '" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "'";
                test = command.ExecuteScalar();

                if (test == null)
                {
                    command.CommandText = "CREATE TABLE `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` (`name` varchar(32) NOT NULL,`permission` varchar(668),`income` decimal(10) NOT NULL DEFAULT 0.00,`freeitem` int(8),`parentgroup` varchar(32),`updategroup` varchar(32),`updatetime` decimal(10) NOT NULL DEFAULT 15.00,`updateenable` bool,`cooldown` varchar(255),PRIMARY KEY (`name`)) ";
                    command.ExecuteNonQuery();
                    command.CommandText = "insert into `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` (`name`,`income`,`updatetime`,`updateenable`) values('default', 10,7,0)";
                    command.ExecuteNonQuery();
                    command.CommandText = "insert into `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` (`name`,`income`,`updatetime`,`updateenable`) values('admin', 60,7,0)";
                    command.ExecuteNonQuery();
                }
                else
                {
                    command.CommandText = "SHOW COLUMNS FROM `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` LIKE 'cooldown'";
                    test = command.ExecuteScalar();
                    if (test == null)
                    {
                        command.CommandText = "ALTER TABLE `" + LIGHT.Instance.Configuration.Instance.DatabaseTableGroup + "` ADD `cooldown` VARCHAR(255) AFTER `permission`";
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

