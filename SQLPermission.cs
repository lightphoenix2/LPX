using System;
using Rocket.API;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using System.Collections.Generic;
using Rocket.API.Serialisation;
using System.Linq;
using Rocket.Core.Logging;

namespace LIGHT
{
    public class SQLPermission: IRocketPermissionsProvider
    {

        public RocketPermissionsProviderResult AddGroup(RocketPermissionsGroup group)
        {
            if(LIGHT.Instance.Database.AddGroup(group.DisplayName,"10",group.ParentGroup,"",99,false))
                return RocketPermissionsProviderResult.Success;
            else
                return RocketPermissionsProviderResult.UnspecifiedError;
        }
        public RocketPermissionsProviderResult AddPlayerToGroup(string groupId, IRocketPlayer player)
        {
            string group = "";
            try
            {
                group = LIGHT.Instance.Database.CheckUserGroupByID(groupId);
            }
            catch
            {
                return RocketPermissionsProviderResult.GroupNotFound;
            }
            if (LIGHT.Instance.Database.AddUserIntoGroup(player.Id, group))
                return RocketPermissionsProviderResult.Success;
            else
                return RocketPermissionsProviderResult.UnspecifiedError;
        }
        public RocketPermissionsProviderResult DeleteGroup(string groupId)
        {
            string group = "";
            try
            {
                group = LIGHT.Instance.Database.CheckUserGroupByID(groupId);
            }
            catch
            {
                return RocketPermissionsProviderResult.GroupNotFound;
            }
            if (group == "default")
                return RocketPermissionsProviderResult.UnspecifiedError;
            if (LIGHT.Instance.Database.RemoveGroup(group))
                return RocketPermissionsProviderResult.Success;
            else
                return RocketPermissionsProviderResult.UnspecifiedError;
        }
        public RocketPermissionsGroup GetGroup(string groupId)
        {
            string group = "";
            group = LIGHT.Instance.Database.CheckUserGroupByID(groupId);
            RocketPermissionsGroup RPG = new RocketPermissionsGroup(group, group, LIGHT.Instance.Database.getParentGroup(group), LIGHT.Instance.Database.getMembers(group), GetGroupPermission(groupId), LIGHT.Instance.Database.GetColor(group));
            return RPG;
        }

        public List<RocketPermissionsGroup> GetGroups(IRocketPlayer player , bool IncludeParentGroup)
        {
            List<RocketPermissionsGroup> Group = new List<RocketPermissionsGroup>();
            string group = "";
            group = LIGHT.Instance.Database.CheckUserGroup(player.Id);
            if (player.IsAdmin && player.Id != null)
                group = "admin";
            else if (group == null || group == "")
                group = "default";         
            RocketPermissionsGroup RPG = new RocketPermissionsGroup(group, group, LIGHT.Instance.Database.getParentGroup(group), LIGHT.Instance.Database.getMembers(group), GetGroupPermission(player), LIGHT.Instance.Database.GetColor(group));
            Group.Add(RPG);
            return Group;
        }

        public List<Permission> GetPermissions(IRocketPlayer player)
        {
            List<Permission> UserGroup = new List<Permission>();
            Permission Usergroup = new Permission(string.Join(" ", LIGHT.Instance.Database.getGroupPermission(LIGHT.Instance.Database.CheckUserGroup(player.Id))), LIGHT.Instance.Database.Cooldown(LIGHT.Instance.Database.CheckUserGroup(player.Id)));
            UserGroup.Add(Usergroup);
            return UserGroup;
        }

        public List<Permission> GetPermissions(IRocketPlayer player, List<string> requestedPermissions)
        {
            List<Permission> UserGroup = new List<Permission>();
            Permission Usergroup = new Permission(string.Join(" ", LIGHT.Instance.Database.getGroupPermission(LIGHT.Instance.Database.CheckUserGroup(player.Id))), LIGHT.Instance.Database.Cooldown(LIGHT.Instance.Database.CheckUserGroup(player.Id)));
            UserGroup.Add(Usergroup);
            return UserGroup;
        }

        /*public bool HasPermission(IRocketPlayer player, string Permission, bool defaultreturnvalue)
        {
            bool hasPerm = false;
            hasPerm = LIGHT.Instance.checkPermission(Permission, player.Id);
            if (player.IsAdmin || LIGHT.Instance.Database.checkhaveAllPermission(LIGHT.Instance.Database.CheckUserGroup(player.Id)))
                hasPerm = true;
            return defaultreturnvalue == false ? hasPerm : defaultreturnvalue;
        }
        public bool HasPermission(IRocketPlayer player, string Permission,out uint? cooldown, bool defaultreturnvalue)
        {
            bool hasPerm = false;
            hasPerm = LIGHT.Instance.checkPermission(Permission, player.Id);
            if (player.IsAdmin || LIGHT.Instance.Database.checkhaveAllPermission(LIGHT.Instance.Database.CheckUserGroup(player.Id)))
                hasPerm = true;
            cooldown = null;
            if (LIGHT.Instance.Database.Cooldown(LIGHT.Instance.Database.CheckUserGroup(player.Id)) != 0 && hasPerm)
            {
                cooldown = LIGHT.Instance.Database.Cooldown(LIGHT.Instance.Database.CheckUserGroup(player.Id));
            }              
            return defaultreturnvalue == false ? hasPerm : defaultreturnvalue;            
        }
        public bool HasPermission(IRocketPlayer player, List<string> Permission, out uint? cooldown, bool defaultreturnvalue)
        {
            bool hasPerm = false;
            hasPerm = LIGHT.Instance.checkPermission(Permission, player.Id);
            if (player.IsAdmin || LIGHT.Instance.Database.checkhaveAllPermission(LIGHT.Instance.Database.CheckUserGroup(player.Id)))
                hasPerm = true;
            cooldown = null;
            if (LIGHT.Instance.Database.Cooldown(LIGHT.Instance.Database.CheckUserGroup(player.Id)) != 0 && hasPerm)
            {
                cooldown = LIGHT.Instance.Database.Cooldown(LIGHT.Instance.Database.CheckUserGroup(player.Id));
            }
            return defaultreturnvalue == false ? hasPerm : defaultreturnvalue;
        }
        public bool HasPermission(IRocketPlayer player, IRocketCommand command, out uint? cooldown, bool defaultreturnvalue)
        {
            bool hasPerm = false;
            hasPerm = LIGHT.Instance.checkPermission(command, player.Id);
            if (player.IsAdmin || LIGHT.Instance.Database.checkhaveAllPermission(LIGHT.Instance.Database.CheckUserGroup(player.Id)))
                hasPerm = true;
            cooldown = null;
            if (LIGHT.Instance.Database.Cooldown(LIGHT.Instance.Database.CheckUserGroup(player.Id)) != 0 && hasPerm)
            {
                cooldown = LIGHT.Instance.Database.Cooldown(LIGHT.Instance.Database.CheckUserGroup(player.Id));
            }
            return defaultreturnvalue == false ? hasPerm : defaultreturnvalue;
        }*/
        
        public bool HasPermission(IRocketPlayer player, List<string> requestedPermissions)
        {
            bool hasPerm = false;
            hasPerm = LIGHT.Instance.checkPermission(requestedPermissions, player.Id);
            if (player.IsAdmin || LIGHT.Instance.Database.checkhaveAllPermission(LIGHT.Instance.Database.CheckUserGroup(player.Id)))
                hasPerm = true;
            return hasPerm;
        }

        public bool SetGroup(IRocketPlayer caller , string group)
        {
            bool Result = false;
            if (LIGHT.Instance.Database.CheckGroup(group))
            {
                UnturnedPlayer target = (UnturnedPlayer)caller;
                if (target != null)
                {
                    LIGHT.Instance.Database.AddUserIntoGroup(caller.Id, group);
                    UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("lpx_added_user", target.SteamName, group));
                    Result = true;
                }
                else
                {
                    UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("lpx_fail_nouser"));
                    Result = false;
                }
            }
            else
            {
                UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("lpx_fail_nogroup"));
                Result = false;
            } 
            return Result;                                          
        }
        
        public void Reload()
        {

        }

        public RocketPermissionsProviderResult RemovePlayerFromGroup(string groupId, IRocketPlayer player)
        {
            string group = "";
            try
            {
                group = LIGHT.Instance.Database.CheckUserGroupByID(groupId);
            }
            catch
            {
                return RocketPermissionsProviderResult.GroupNotFound;
            }
            if (group == "default")
                return RocketPermissionsProviderResult.UnspecifiedError;
            if (LIGHT.Instance.Database.RemoveUser(player.Id))
                return RocketPermissionsProviderResult.Success;
            else
                return RocketPermissionsProviderResult.UnspecifiedError;
        }
        public RocketPermissionsProviderResult SaveGroup(RocketPermissionsGroup group)
        {
            return RocketPermissionsProviderResult.Success;
        }

        private List<Permission> GetGroupPermission(IRocketPlayer player)
        {
            string group = "";
            group = LIGHT.Instance.Database.CheckUserGroup(player.Id);
            if (player.IsAdmin && player.Id != null)
                group = "admin";
            else if (group == null || group == "")
                group = "default";
            List<Permission> GroupPermission = new List<Permission>();
            Permission GpPer = new Permission(group, LIGHT.Instance.Database.Cooldown(group));
            GroupPermission.Add(GpPer);
            return GroupPermission;
        }
        private List<Permission> GetGroupPermission(string GroupID)
        {
            string group = "";
            group = LIGHT.Instance.Database.CheckUserGroupByID(GroupID);
            if (group == null || group == "")
                group = "default";
            List<Permission> GroupPermission = new List<Permission>();
            Permission GpPer = new Permission(group, LIGHT.Instance.Database.Cooldown(group));
            GroupPermission.Add(GpPer);
            return GroupPermission;
        }
    }                            
}
