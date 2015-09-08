using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using System.Linq;
using UnityEngine;
using Rocket.Core;
using Rocket.API;

namespace LIGHT
{
    public class LIGHT : RocketPlugin<Configuration>
    {
        public DatabaseManager Database;
        public static LIGHT Instance;
        static IRocketPermissionsProvider OriginalPermissions;

        protected override void Load()
        {
            Instance = this;
            Database = new DatabaseManager();
            U.Events.OnPlayerConnected += RocketServerEvents_OnPlayerConnected;
            SQLPermission permission = new SQLPermission();
            OriginalPermissions = R.Permissions;
            R.Permissions = permission;
        }
        protected override void Unload()
        {
            R.Permissions = OriginalPermissions;
        }
        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList(){
                {"lpx_no_perm","You have no permission to that command!"},
                {"lpx_invaild","Invalid command!"},
                {"lpx_help","/lpx adduser, /lpx removeuser, /lpx addpermission, /lpx removepermission, /lpx addgroup, /lpx removegroup, /lpx listgroup, /lpx setgroupincome}"},
                {"lpx_help_adduser","/lpx adduser <playername> <group>"},
                {"lpx_help_adduser2","Missing <group> parameter!"},
                {"lpx_fail_nouser","User not found!"},
                {"lpx_fail_nogroup","Group does not exist!"},
                {"lpx_added_user","{0} has being added to Group: {1}"},
                {"lpx_help_addpermission","/lpx addpermission <group> <permission>"},
                {"lpx_help_addpermission2","Missing <permission> parameter!"},
                {"lpx_added_permission","Successfully added {0} to Group: {1}!"},
                {"lpx_failed_permission","Fail to add {0} to Group: {1}!"},
                {"lpx_help_addgroup","/lpx addgroup <name> <income>"},
                {"lpx_group_exist","Group: {0} already exist!"},
                {"lpx_added_group","Group: {0} successfully added!"},
                {"lpx_failed_group","Fail to add Group: {0}!"},
                {"lpx_help_removeuser","/lpx removeuser <name>"},
                {"lpx_removed_user","{0} has being removed from Group: {1}"},
                {"lpx_fail_remove","Fail to remove {0} from Group: {1}"},
                {"lpx_help_removegroup","/lpx removegroup <name>"},
                {"lpx_fail_removegroup","Fail to remove Group: {0}!"},
                {"lpx_removed_group","Group: {0} succesfully removed!"},
                {"lpx_help_removepermission","/lpx removepermission <group> <permission>"},
                {"lpx_help_removepermission2","Missing <permission> parameter!"},
                {"unable_to_pay_group_msg","Unable to pay {0} as no {1} group salary set."},
                {"pay_time_msg", "You have received {0} {1} in salary for being a {2}."},
                {"new_balance_msg", "Your new balance is {0} {1}."},
                {"lpx_help_addgroupfreeitem","/lpx addgroupfreeitem <group> <ID>"},
                {"lpx_help_addgroupfreeitem2","Missing parameter Item <ID>"},
                {"lpx_added_addgroupfreeitem","Sucessfully added Item ID:{1} to group {0}"},
                {"lpx_failed_addgroupfreeitem","Fail to add Item ID:{1} to group {0}"},
                {"lpx_help_listpermission","/lpx listpermission <group>"},
                {"lpx_listp_group","Group:{0}"},
                {"lpx_listp_permission","Permission:{0}"},
                {"lpx_help_setgroupincome","/lpx setgroupincome <group> <amount>"},
                {"lpx_added_income","Group: {0}, income have been set to {1}"},
                {"lpx_failed_setincome","Fail to set income to Group: {0}"}
                };
            }
        }
        public bool checkPermission(string PermissionCmd, string ID)
        {          
            string[] permission = { };
            bool hasPerm = false;
            permission = LIGHT.Instance.Database.getPermission(ID);
            for (int i = permission.Length - 1; i >= 0; i--)
            {
                if (permission[i] == PermissionCmd)
                    hasPerm = true;
            }
            return hasPerm;
        }

        public void RocketServerEvents_OnPlayerConnected(UnturnedPlayer player)
        {
            string[] permission = { };
            string[] cmd = {};
            permission = LIGHT.Instance.Database.getPermission(player.Id);
            for (int i = permission.Length - 1; i >= 0; i--)
            {
                if (permission[i].Contains("color."))
                {
                    
                    cmd = permission[i].Split('.');
                    Color? color = UnturnedChat.GetColorFromName(cmd[1], Color.blue);
                    player.Color = color.Value;
                }
            }
        }

    }
}