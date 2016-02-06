using System;
using Rocket.API;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using System.Collections.Generic;
using Rocket.API.Serialisation;
using System.Linq;

namespace LIGHT
{
    public class SQLPermission: IRocketPermissionsProvider
    {
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
        private List<Permission> GetGroupPermission(IRocketPlayer player)
        {
            string group = "";
            group = LIGHT.Instance.Database.CheckUserGroup(player.Id);
            if (player.IsAdmin && player.Id != null)
                group = "admin";
            else if (group == null || group == "")
                group = "default";
            List<Permission> GroupPermission = new List<Permission>();
            Permission GpPer = new Permission(string.Join(" ", LIGHT.Instance.Database.getGroupPermission(group)), LIGHT.Instance.Database.Cooldown(LIGHT.Instance.Database.CheckUserGroup(player.Id)));
            GroupPermission.Add(GpPer);
            return GroupPermission;
        }
        public bool HasPermission(IRocketPlayer player, string Permission, bool defaultreturnvalue)
        {
            bool hasPerm = false;
            hasPerm = LIGHT.Instance.checkPermission(Permission, player.Id);
            if (player.IsAdmin)
                hasPerm = true;
            return defaultreturnvalue == false ? hasPerm : defaultreturnvalue;
        }
        public bool HasPermission(IRocketPlayer player, string Permission,out uint? cooldown, bool defaultreturnvalue)
        {
            bool hasPerm = false;
            hasPerm = LIGHT.Instance.checkPermission(Permission, player.Id);
            if (player.IsAdmin)
                hasPerm = true;
            cooldown = null; 
            if (LIGHT.Instance.Database.Cooldown(LIGHT.Instance.Database.CheckUserGroup(player.Id)) != 0 && hasPerm)
                cooldown = LIGHT.Instance.Database.Cooldown(LIGHT.Instance.Database.CheckUserGroup(player.Id));                                     
            return defaultreturnvalue == false ? hasPerm : defaultreturnvalue;            
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
        public List<Permission> GetPermissions(IRocketPlayer player)
        {
            List<Permission> UserGroup = new List<Permission>();
            Permission Usergroup = new Permission(string.Join(" ", LIGHT.Instance.Database.getGroupPermission(LIGHT.Instance.Database.CheckUserGroup(player.Id))), LIGHT.Instance.Database.Cooldown(LIGHT.Instance.Database.CheckUserGroup(player.Id)));
            UserGroup.Add(Usergroup);
            return UserGroup;
        }
        public void Reload()
        {

        }
    }                            
}
