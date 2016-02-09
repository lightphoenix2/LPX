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
    public class CommandKit : IRocketCommand
    {
        
        public string Help
        {
            get { return "Give Kits to player"; }
        }

        public string Name
        {
            get { return "kit"; }
        }

        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Player;
            }
        }

        public string Syntax
        {
            get { return "<Kit Name>"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "kit"};
            }
        }

        public void Execute(IRocketPlayer caller,params string[] command)
        {
            string[] permission = { };
            bool hasPerm = false;
            UnturnedPlayer player;
            permission = LIGHT.Instance.Database.getGroupPermission(LIGHT.Instance.Database.CheckUserGroup(caller.Id));
            player = (UnturnedPlayer)caller;
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("kit_help"));
                return;
            }
            if (command.Length > 0)
            {
                for (int i = permission.Length - 1; i >= 0; i--)
                {
                    if (permission[i] == "kit.*" || (permission[i] == ("kit." + command[0]) && command.Length == 1))
                    {
                        hasPerm = true;
                    }
                    if (command.Length > 1)
                    {
                        string kitnamePermission = "";
                        for (int x = 0; x < command.Length; x++)
                        {
                            kitnamePermission += command[x];
                        }
                        if (permission[i] == ("kit." + kitnamePermission))
                            hasPerm = true;
                    }
                }
                if (!hasPerm && !(caller.IsAdmin))
                {
                    UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("lpx_no_perm"));
                    return;
                }
                string comd = String.Join(" ", command);
                if (!LIGHT.Instance.Configuration.Instance.KitsEnabled) return;
                else
                {
                    hasPerm = false;
                    KeyValuePair<string, DateTime> playerCooldown = LIGHT.PlayerCooldown.Where(z => z.Key == caller.ToString()).FirstOrDefault();
                    if (command.Length == 1)
                    {
                        if (LIGHT.Instance.Database.CheckKit(command[0]))
                        {
                            double cooldown = LIGHT.Instance.Database.GetKitCooldown(command[0]);
                            if (!playerCooldown.Equals(default(KeyValuePair<string, DateTime>)))
                            {
                                double CooldownLeft = (DateTime.Now - playerCooldown.Value).TotalSeconds;
                                if (CooldownLeft < cooldown)
                                {
                                    UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("kit_cooldown", (int)(cooldown - CooldownLeft)));
                                    return;
                                }
                            }
                            string[] itemIDAmt = LIGHT.Instance.Database.GetKitItems(command[0]);
                            string[] itemID = new string[itemIDAmt.Length];
                            string[] Amount = new string[itemIDAmt.Length];
                            for (int x = 0; x < itemID.Length; x++)
                            {
                                itemID[x] = itemIDAmt[x].Split('/')[0];
                                Amount[x] = itemIDAmt[x].Split('/')[1];
                                try
                                {
                                    player.GiveItem(ushort.Parse(itemID[x]), byte.Parse(Amount[x]));
                                }
                                catch
                                {
                                    Logger.Log("Error in Kit Name or Amount. Please check your database and correct them as soon as possible.");
                                }
                            }
                            UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("kit_given", command[0]));
                            if (LIGHT.PlayerCooldown.ContainsKey(caller.ToString()))
                            {
                                LIGHT.PlayerCooldown[caller.ToString()] = DateTime.Now;
                            }
                            else
                            {
                                LIGHT.PlayerCooldown.Add(caller.ToString(), DateTime.Now);
                            }
                        }
                        else
                        {
                            UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("kit_notexist", command[0]));
                            return;
                        }
                    }
                    else if (command.Length > 1)
                    {
                        string name = "";
                        for (int x = 0; x < command.Length; x++)
                        {
                            name += command[x] + " ";
                        }
                        name.Trim();
                        if (LIGHT.Instance.Database.CheckKit(name))
                        {
                            double cooldown = LIGHT.Instance.Database.GetKitCooldown(name);
                            if (!playerCooldown.Equals(default(KeyValuePair<string, DateTime>)))
                            {
                                double CooldownLeft = (DateTime.Now - playerCooldown.Value).TotalSeconds;
                                if (CooldownLeft < cooldown)
                                {
                                    UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("kit_cooldown", (int)(cooldown - CooldownLeft)));
                                    return;
                                }
                            } 
                            string[] itemIDAmt = LIGHT.Instance.Database.GetKitItems(name);
                            string[] itemID = new string[itemIDAmt.Length];
                            string[] Amount = new string[itemIDAmt.Length];
                            for (int x = 0; x < itemID.Length; x++)
                            {
                                itemID[x] = itemIDAmt[x].Split('/')[0];
                                Amount[x] = itemIDAmt[x].Split('/')[1];
                                try
                                {
                                    player.GiveItem(ushort.Parse(itemID[x]), byte.Parse(Amount[x]));
                                }
                                catch
                                {
                                    Logger.Log("Error in Kit Name or Amount. Please check your database and correct them as soon as possible.");
                                }
                            }
                            UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("kit_given", name));
                            if (LIGHT.PlayerCooldown.ContainsKey(caller.ToString()))
                            {
                                LIGHT.PlayerCooldown[caller.ToString()] = DateTime.Now;
                            }
                            else
                            {
                                LIGHT.PlayerCooldown.Add(caller.ToString(), DateTime.Now);
                            }
                        }
                        else
                        {
                            UnturnedChat.Say(caller, LIGHT.Instance.DefaultTranslations.Translate("kit_notexist", name));
                            return;
                        }
                    }
                    else
                        return;
                }
            }
        }
    }
}
