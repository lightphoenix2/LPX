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
using fr34kyn01535.Uconomy;

namespace LIGHT
{
    public class CommandBuyV : IRocketCommand
    {
        
        public string Help
        {
            get { return "Buy a vehicle"; }
        }

        public string Name
        {
            get { return "buyv"; }
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
            get { return "<Car No>"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "buyv" };
            }
        }

        public void Execute(IRocketPlayer caller,params string[] command)
        {
            if (!LIGHT.Instance.Configuration.Instance.AllowCarOwnerShip) return;
            string[] permission = { };
            bool hasPerm = false;
            UnturnedPlayer player= (UnturnedPlayer)caller;
            if(LIGHT.Instance.Configuration.Instance.LPXEnabled)
                permission = LIGHT.Instance.Database.getGroupPermission(LIGHT.Instance.Database.CheckUserGroup(caller.Id));
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_car_buyhelp"));
                return;
            }
            if (command.Length > 0)
            {
                if (LIGHT.Instance.Configuration.Instance.LPXEnabled)
                {
                    for (int i = permission.Length - 1; i >= 0; i--)
                    {
                        if (permission[i] == "buyv" || permission[i] == "lpx.buyv" || permission[i] == "*")
                        {
                            hasPerm = true;
                        }
                    }
                }
                else
                {
                    if (player.HasPermission("buyv") || player.HasPermission("*"))
                        hasPerm = true;
                }
                if (!hasPerm && !(caller.IsAdmin))
                {
                    UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_no_perm"));
                    return;
                }                                
                else
                {
                    if(LIGHT.Instance.Database.CheckOwner(command[0]) != "")
                    {
                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_car_alreadyown"));
                        return;
                    }
                    else
                    {
                        if (LIGHT.Instance.Database.CheckCarExistInDB(command[0]))
                        {
                            decimal price = decimal.Parse("100");
                            decimal.TryParse(LIGHT.Instance.Configuration.Instance.LicencePrice.ToString(), out price);
                            decimal balance = Uconomy.Instance.Database.GetBalance(player.Id);
                            if (balance < price)
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_not_enough_currency", Uconomy.Instance.Configuration.Instance.MoneyName));
                                return;
                            }
                            decimal bal = Uconomy.Instance.Database.IncreaseBalance(player.CSteamID.ToString(), (price * -1));
                            if (bal >= 0.0m) UnturnedChat.Say(player.CSteamID, LIGHT.Instance.Translate("new_balance_msg", new object[] { bal, Uconomy.Instance.Configuration.Instance.MoneyName }));
                            if (!LIGHT.Instance.Database.AddOwnership(command[0], player.Id, player.SteamName))
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_car_purchaseFailed"));
                                bal = Uconomy.Instance.Database.IncreaseBalance(player.CSteamID.ToString(), price);
                                UnturnedChat.Say(player.CSteamID, LIGHT.Instance.Translate("new_balance_msg", new object[] { bal, Uconomy.Instance.Configuration.Instance.MoneyName }));
                                return;
                            }
                            else
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_car_purchased", command[0], price.ToString(), Uconomy.Instance.Configuration.Instance.MoneyName));
                            }
                        }
                        else
                            UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_car_purchaseFailed"));
                    }
                }
            }
        }
    }
}
