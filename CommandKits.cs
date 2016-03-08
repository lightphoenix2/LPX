using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using System;
using Rocket.API;
using System.Linq;
using SDG.Unturned;
using System.Data;
using System.Collections.Generic;
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
            if (!LIGHT.Instance.Configuration.Instance.KitsEnabled) return;
            string[] permission = {};
            string[] Kits;
            Kits = LIGHT.Instance.Database.GetPlayerKitName(LIGHT.Instance.Database.CheckUserGroup(caller.Id));
            bool hasPerm = false;
            bool console = (caller is ConsolePlayer);
            if (!console)
            {
                if (caller.HasPermission("kits") || caller.HasPermission("kits.*") || caller.HasPermission("*"))
                {
                    hasPerm = true;
                }
                if (!hasPerm && !(caller.IsAdmin))
                {
                    UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_no_perm"));
                    return;
                }
            }          
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
                UnturnedChat.Say(caller, LIGHT.Instance.Translate("kit_list", ListKit));
                return;
            }
            if (command.Length >= 1)
            {
                hasPerm = false;
                switch (command[0].ToLower())
                {
                    case "all":
                        if (caller.HasPermission("kits.all") || caller.HasPermission("kits.*") || caller.HasPermission("*"))
                        {
                            hasPerm = true;
                        }                      
                        if (!hasPerm && !console && !(caller.IsAdmin))
                        {
                            UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_no_perm"));
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
                            UnturnedChat.Say(caller, LIGHT.Instance.Translate("kit_list_all",ALL));
                        }
                        break;
                    case "cooldown":
                        if (caller.HasPermission("kits.cd") || caller.HasPermission("kits.*") || caller.HasPermission("*"))
                        {
                            hasPerm = true;
                        }
                        if (!hasPerm && !console && !(caller.IsAdmin))
                        {
                            UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_no_perm"));
                            return;
                        }
                        else
                        {
                            if(command.Length == 1)
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("kits_chngcd_help"));
                                return;
                            }
                            if(command.Length == 2)
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("kits_chngcd_help2"));
                                return;
                            }
                            if(command.Length > 2)
                            {
                                string KitName = "";
                                double NewCD = 60.00;
                                for(int x = 1; x < command.Length-1; x++)
                                {
                                    KitName += command[x] + " ";
                                }
                                KitName = KitName.Trim();
                                if (double.TryParse(command[command.Length - 1], out NewCD))
                                {
                                    if (LIGHT.Instance.Database.SetKitsCooldown(KitName, NewCD))
                                    {
                                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("kits_success_chngcd", KitName, NewCD));
                                    }
                                    else
                                    {
                                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("kits_failed_chngcd", KitName));
                                        return;
                                    }
                                }
                                else
                                {
                                    UnturnedChat.Say(caller, LIGHT.Instance.Translate("kits_error_cooldown", KitName));
                                    return;
                                }
                            }                                
                        }
                        break;
                    case "add":
                        if (caller.HasPermission("kits.add") || caller.HasPermission("kits.*") || caller.HasPermission("*"))
                        {
                            hasPerm = true;
                        }
                        if (!hasPerm && !console && !(caller.IsAdmin))
                        {
                            UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_no_perm"));
                            return;
                        }
                        else
                        {
                            if (command.Length == 1)
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("kits_add_help"));
                                return;
                            }
                            if (command.Length >= 2)
                            {
                                string KitName = "";
                                string itemID = "";
                                double NewCD = 60.00;                              
                                for (int x = 1; x < command.Length; x++)
                                {
                                    if(!command[x].Contains("/"))
                                        KitName += command[x] + " ";
                                    else
                                        itemID += command[x] + " ";
                                } 
                                KitName = KitName.Trim();
                                itemID = itemID.Trim();
                                if(LIGHT.Instance.Database.AddKit(KitName,itemID,NewCD))
                                {
                                    UnturnedChat.Say(caller, LIGHT.Instance.Translate("kits_add_success",KitName));
                                }
                                else
                                {
                                    UnturnedChat.Say(caller, LIGHT.Instance.Translate("kits_add_failed",KitName));
                                    return;
                                }
                            }
                        }
                        break;
                    case "remove":
                        if (caller.HasPermission("kits.remove") || caller.HasPermission("kits.*") || caller.HasPermission("*"))
                        {
                            hasPerm = true;
                        }
                        if (!hasPerm && !console && !(caller.IsAdmin))
                        {
                            UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_no_perm"));
                            return;
                        }
                        else
                        {
                            if (command.Length == 1)
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("kits_remove_help"));
                                return;
                            }
                            if (command.Length >= 2)
                            {
                                string KitName = "";
                                for (int x = 1; x < command.Length; x++)
                                {
                                    KitName += command[x] + " ";
                                }
                                KitName = KitName.Trim();
                                if(LIGHT.Instance.Database.CheckKit(KitName))
                                {
                                    if(LIGHT.Instance.Database.RemoveKit(KitName))
                                    {
                                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("kits_remove_success", KitName));
                                    }
                                    else
                                    {
                                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("kits_remove_failed", KitName));
                                        return;
                                    }
                                }
                                else
                                {
                                    UnturnedChat.Say(caller, LIGHT.Instance.Translate("kit_notexist"));
                                    return;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
