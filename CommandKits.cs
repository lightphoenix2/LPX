using Rocket.Unturned;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using System;
using Rocket.API;
using System.Linq;
using SDG.Unturned;
using System.Data;
using System.Collections.Generic;
using UnityEngine;
using Rocket.Core.Logging;

namespace LIGHT
{
    public class CommandKits : IRocketCommand
    {
        
        public string Help
        {
            get { return "Display all Kits. Add/Edit/Remove Kits"; }
        }

        public string Name
        {
            get { return "kits"; }
        }

        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Both;
            }
        }

        public string Syntax
        {
            get { return "<add/remove/edit> <Kit Name>"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "kits" };
            }
        }

        public void Execute(IRocketPlayer caller,params string[] command)
        {
            string[] permission = {};
            string[] Kits;
            Kits = LIGHT.Instance.Database.GetPlayerKitName(LIGHT.Instance.Database.CheckUserGroup(caller.Id));
            bool hasPerm = false;
            UnturnedPlayer player;
            bool console = (caller is ConsolePlayer);
            if (!console)
            {
                permission = LIGHT.Instance.Database.getGroupPermission(LIGHT.Instance.Database.CheckUserGroup(caller.Id));
                player = (UnturnedPlayer)caller;
                for (int i = permission.Length - 1; i >= 0; i--)
                {
                    if (permission[i] == "kits" || permission[i] == "kits.*")
                    {
                        hasPerm = true;
                    }
                    if (permission[i] == "kit.*")
                    {
                        Kits = LIGHT.Instance.Database.GetAllKitName();
                    }

                }
                if (!hasPerm && !(caller.IsAdmin))
                {
                    UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("lpx_no_perm"));
                    return;
                }
            }
            if (!LIGHT.Instance.Configuration.Instance.KitsEnabled) return;
            if (command.Length == 0)
            {
                string ListKit = "";
                for (int x = 0; x < Kits.Length; x++)
                {
                    if (x < (Kits.Length - 1))
                        ListKit += Kits[x] + ", ";
                    else
                        ListKit += Kits[x] + ".";
                }
                UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("kit_list", ListKit));
                return;
            }
            if (command.Length >= 1)
            {
                hasPerm = false;
                switch (command[0])
                {
                    case "all":
                        for (int i = permission.Length - 1; i >= 0; i--)
                        {
                            if (permission[i] == "kits.all" || permission[i] == "kits.*")
                                hasPerm = true;
                        }
                        if (!hasPerm && !console && !(caller.IsAdmin))
                        {
                            UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("lpx_no_perm"));
                            return;
                        }
                        else
                        {
                            Kits = LIGHT.Instance.Database.GetAllKitName();
                            string[] cooldown = new string[Kits.Length];
                            string ALL = "";
                            for(int x = 0; x < Kits.Length; x ++)
                            {
                                cooldown[x] = LIGHT.Instance.Database.GetKitCooldown(Kits[x]).ToString();
                                if(x < (Kits.Length-1))
                                    ALL += Kits[x] + " Cooldown: " + cooldown[x] + ", ";
                                else
                                    ALL += Kits[x] + " Cooldown: " + cooldown[x];
                            }         
                            UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("kit_list_all",ALL));
                        }
                        break;
                    case "cooldown":
                        for (int i = permission.Length - 1; i >= 0; i--)
                        {
                            if (permission[i] == "kits.cd" || permission[i] == "kits.*")
                                hasPerm = true;
                        }
                        if (!hasPerm && !console && !(caller.IsAdmin))
                        {
                            UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("lpx_no_perm"));
                            return;
                        }
                        else
                        {
                            if(command.Length == 1)
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("kits_chngcd_help"));
                                return;
                            }
                            if(command.Length == 2)
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("kits_chngcd_help2"));
                                return;
                            }
                            if(command.Length > 2)
                            {
                                string KitName = "";
                                double NewCD = 10.00;
                                for(int x = 1; x < command.Length-1; x++)
                                {
                                    KitName += command[x] + " ";
                                }
                                KitName.Trim();
                                if(double.TryParse(command[command.Length-1], out NewCD))
                                if(LIGHT.Instance.Database.SetKitsCooldown(KitName, NewCD))
                                {
                                    UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("kits_success_chngcd",KitName,NewCD));
                                }
                                else
                                {
                                    UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("kits_failed_chngcd",KitName));
                                    return;
                                }
                            }                                
                        }
                        break;
                }
            }
        }
    }
}
