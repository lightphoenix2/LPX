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
            if (group == null || group == "")
                group = "default";
            RocketPermissionsGroup RPG = new RocketPermissionsGroup(group, group, LIGHT.Instance.Database.getParentGroup(group), LIGHT.Instance.Database.getMembers(group), LIGHT.Instance.Database.getGroupPermission(group).ToList());
            Group.Add(RPG);
            return Group;
        }
        public bool HasPermission(IRocketPlayer player, string Permission, bool defaultreturnvalue)
        {
            bool hasPerm = false;
            hasPerm = LIGHT.Instance.checkPermission(Permission, player.Id);
            //if (hasPerm == false) UnturnedChat.Say(player, LIGHT.Instance.DefaultTranslations.Translate("lpx_no_perm"));
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
        public List<string> GetPermissions(IRocketPlayer player)
        {
            return LIGHT.Instance.Database.getGroupPermission(LIGHT.Instance.Database.CheckUserGroup(player.Id)).ToList();
        }
        public void Reload()
        {

        }
    }                            
}
