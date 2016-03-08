using System;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace LIGHT
{
    public class CommandShop : IRocketCommand
    {        
        public string Name
        {
            get
            {
                return "shop";
            }
        }
        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Both;
            }
        }
        public string Help
        {
            get
            {
                return "Allows admins to change, add, or remove items/vehicles from the shop.";
            }
        }
        public string Syntax
        {
            get
            {
                return "<add | rem | chng | buy> [v.]<itemid> <cost>";
            }
        }
        public List<string> Aliases
        {
            get { return new List<string>(); }
        }
        public List<string> Permissions
        {
            get { return new List<string>() { "shop.*", "shop.add", "shop.rem", "shop.chng", "shop.buy" }; }
        }
        public void Execute(IRocketPlayer caller, params string[] command)
        {
            bool console = (caller is ConsolePlayer);
            bool hasperm = false;
            string message;
            if (!console)
            {
                UnturnedPlayer player = (UnturnedPlayer)caller;
                if (player.HasPermission("shop.*") || player.HasPermission("shop.add") || player.HasPermission("shop.rem") || player.HasPermission("shop.chng") || player.HasPermission("shop.buy") || player.HasPermission("*"))
                    hasperm = true;
            }
            if (!hasperm && !console)
            {
                UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_no_perm"));
                return;
            }
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, LIGHT.Instance.Translate("shop_command_usage"));
                return;
            }
            if (command.Length < 2)
            {
                UnturnedChat.Say(caller, LIGHT.Instance.Translate("no_itemid_given"));
                return;
            }
            if (command.Length == 2 && command[0] != "rem")
            {
                UnturnedChat.Say(caller, LIGHT.Instance.Translate("no_cost_given"));
                return;
            }
            else if (command.Length >= 2)
            {
                string[] type = Parser.getComponentsFromSerial(command[1], '.');
                ushort id;
                if (type.Length > 1 && type[0] != "v")
                {
                    UnturnedChat.Say(caller, LIGHT.Instance.Translate("v_not_provided"));
                    return;
                }
                
                if (type.Length > 1)
                {
                    if (!ushort.TryParse(type[1], out id))
                    {
                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("invalid_id_given"));
                        return;
                    }
                }
                else
                {
                    if (!ushort.TryParse(type[0], out id))
                    {
                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("invalid_id_given"));
                        return;
                    }
                }
                bool success = false;
                bool change = false;
                bool pass = false;
                switch (command[0])
                {
                    case "chng":
                        if (caller.HasPermission("shop.*") || caller.HasPermission("shop.chng") || caller.HasPermission("*"))
                        {
                            change = true;
                            pass = true;
                            goto case "add";
                        }
                        else
                        {
                            UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_no_perm"));
                            return;
                        }
                    case "add":
                        if (!pass)
                        {
                            if (!caller.HasPermission("shop.*") && !caller.HasPermission("shop.add") && !caller.HasPermission("*"))
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("no_permission_shop_add"));
                                return;
                            }
                        }
                        string ac = (pass) ? LIGHT.Instance.Translate("changed") : LIGHT.Instance.Translate("added");
                        switch (type[0])
                        {
                            case "v":
                                try
                                {
                                    VehicleAsset va = (VehicleAsset)Assets.find(EAssetType.VEHICLE, id);
                                    message = LIGHT.Instance.Translate("changed_or_added_to_shop", new object[] { ac, va.Name, command[2] });
                                    success = LIGHT.Instance.ShopDB.AddVehicle((int)id, va.Name, decimal.Parse(command[2]), change);
                                    if (!success)
                                    {
                                        message = LIGHT.Instance.Translate("error_adding_or_changing", new object[] { va.Name });
                                    }
                                    this.sendMessage(caller, message, console);
                                }
                                catch
                                {
                                    UnturnedChat.Say(caller, LIGHT.Instance.Translate("vehicle_invalid"));
                                    return;
                                }
                                break;
                            default:
                                try
                                {
                                    ItemAsset ia = (ItemAsset)Assets.find(EAssetType.ITEM, id);
                                    message = LIGHT.Instance.Translate("changed_or_added_to_shop", new object[] { ac,ia.Name,command[2]});
                                    success = LIGHT.Instance.ShopDB.AddItem((int)id, ia.Name, decimal.Parse(command[2]), change);
                                    if (!success)
                                    {
                                        message = LIGHT.Instance.Translate("error_adding_or_changing", new object[] { ia.Name });
                                    }
                                    this.sendMessage(caller, message, console);
                                }
                                catch
                                {
                                     UnturnedChat.Say(caller, LIGHT.Instance.Translate("item_invalid"));
                                     return;
                                }                               
                                break;
                        }
                        break;
                    case "rem":
                        if (!caller.HasPermission("shop.*") && !caller.HasPermission("shop.rem") && !caller.HasPermission("*"))
                        {
                            message = LIGHT.Instance.DefaultTranslations.Translate("no_permission_shop_rem", new object[] { });
                            this.sendMessage(caller, message, console);
                            return;
                        }
                        switch (type[0])
                        {
                            case "v":
                                try
                                {
                                    VehicleAsset va = (VehicleAsset)Assets.find(EAssetType.VEHICLE, id);
                                    message = LIGHT.Instance.Translate("removed_from_shop", new object[] { va.Name });
                                    success = LIGHT.Instance.ShopDB.DeleteVehicle((int)id);
                                    if (!success)
                                    {
                                        message = LIGHT.Instance.Translate("not_in_shop_to_remove", new object[] { va.Name });
                                    }
                                    this.sendMessage(caller, message, console);
                                }
                                catch
                                {
                                    UnturnedChat.Say(caller, LIGHT.Instance.Translate("vehicle_invalid"));
                                    return;
                                }
                                break;
                            default:
                                try
                                {
                                    ItemAsset ia = (ItemAsset)Assets.find(EAssetType.ITEM, id);
                                    message = LIGHT.Instance.Translate("removed_from_shop", new object[] { ia.Name });
                                    success = LIGHT.Instance.ShopDB.DeleteItem((int)id);
                                    if (!success)
                                    {
                                        message = LIGHT.Instance.Translate("not_in_shop_to_remove", new object[] { ia.Name });
                                    }
                                    this.sendMessage(caller, message, console);
                                }
                                catch
                                {
                                    UnturnedChat.Say(caller, LIGHT.Instance.Translate("item_invalid"));
                                    return;
                                }
                                break;
                        }
                        break;
                    case "buy":
                        if (!caller.HasPermission("shop.*") && !caller.HasPermission("shop.buy") && !caller.HasPermission("*"))
                        {
                            UnturnedChat.Say(caller, LIGHT.Instance.Translate("no_permission_shop_buy"));
                            return;
                        }
                        try
                        {
                            ItemAsset iab = (ItemAsset)Assets.find(EAssetType.ITEM, id);
                            decimal buyb;
                            decimal.TryParse(command[2], out buyb);
                            message = LIGHT.Instance.Translate("set_buyback_price", new object[] {iab.Name, buyb.ToString()});
                            success = LIGHT.Instance.ShopDB.SetBuyPrice((int)id, buyb);
                            if (!success)
                            {
                                message = LIGHT.Instance.Translate("not_in_shop_to_buyback", new object[] { iab.Name });
                            }
                            this.sendMessage(caller, message, console);
                        }
                        catch
                        {
                            UnturnedChat.Say(caller, LIGHT.Instance.Translate("item_invalid"));
                            return;
                        }
                        break;
                    default:
                        // We shouldn't get this, but if we do send an error.
                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("not_in_shop_to_remove"));
                        return;
                }
            }
        }
        private void sendMessage(IRocketPlayer playerid, string message, bool console)
        {
            if (console)
            {
                Logger.Log(message);
            }
            else
            {
                UnturnedChat.Say(playerid, message);
            }
        }
    }
}