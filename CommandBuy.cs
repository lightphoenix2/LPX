using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using fr34kyn01535.Uconomy;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LIGHT
{
    public class CommandBuy : IRocketCommand
    {
        public string Help
        {
            get { return "Allows you to buy items from the shop"; }
        }

        public string Name
        {
            get { return "buy"; }
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
            get { return "[v.]<name or id> [amount] [25 | 50 | 75 | 100]"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "buy"};
            }
        }

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            if(!LIGHT.Instance.Configuration.Instance.EnableShop)
            {
                UnturnedChat.Say(caller, LIGHT.Instance.Translate("shop_disable"));
                return;
            }
            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (command.Length == 0)
            {
                UnturnedChat.Say(player, LIGHT.Instance.Translate("buy_command_usage"));
                return;
            }
            
            byte amttobuy = 1;
            string Itemname = "";
            byte CheckItemID = 0;
            bool IsID = byte.TryParse(command[0], out CheckItemID);
            bool IsAmtKeyed = false;
            if (command.Length > 1)
                IsAmtKeyed = byte.TryParse(command[command.Length - 1], out amttobuy);
            for (int i = 0; i < (command.Length - 1); i++)
            {
                if (i == (command.Length - 2))
                    Itemname += command[i];
                else
                    Itemname += command[i] + " ";
            }
            if (!IsAmtKeyed)
            {
                amttobuy = 1;
                if (command.Length == 2)
                    Itemname = command[0] + " " + command[1];
                else if (command.Length == 1)
                    Itemname = command[0];
            }
            string[] components = Parser.getComponentsFromSerial(command[0], '.');
            if (components.Length == 2 && components[0] != "v")
            {
                UnturnedChat.Say(player, LIGHT.Instance.Translate("buy_command_usage"));
                return;
            }
            ushort id;
            switch (components[0])
            {
                case "v":
                    if (!LIGHT.Instance.Configuration.Instance.CanBuyVehicles)
                    {
                        UnturnedChat.Say(player, LIGHT.Instance.Translate("buy_vehicles_off"));
                        return;
                    }
                    string name = "";
                    if (!ushort.TryParse(components[1], out id))
                    {
                        Asset[] array = Assets.find(EAssetType.VEHICLE);
                        Asset[] array2 = array;
                        for (int i = 0; i < array2.Length; i++)
                        {
                            VehicleAsset vAsset = (VehicleAsset)array2[i];
                            if (vAsset != null && vAsset.vehicleName != null && vAsset.vehicleName.ToLower().Contains(components[1].ToLower()))
                            {
                                id = vAsset.id;
                                name = vAsset.vehicleName;
                                break;
                            }
                        }
                    }
                    if (name == null && id == 0)
                    {
                        UnturnedChat.Say(player, LIGHT.Instance.Translate("could_not_find",components[1]));
                        return;
                    }
                    else if (name == null && id != 0)
                    {
                        try
                        {
                            name = ((VehicleAsset)Assets.find(EAssetType.VEHICLE, id)).vehicleName;
                        }
                        catch
                        {
                            UnturnedChat.Say(caller, LIGHT.Instance.Translate("vehicle_invalid"));
                            return;
                        }
                    }
                    decimal cost = LIGHT.Instance.ShopDB.GetVehicleCost(id);
                    decimal balance = Uconomy.Instance.Database.GetBalance(player.Id);
                    if (cost <= 0m)
                    {
                        UnturnedChat.Say(player, LIGHT.Instance.Translate("vehicle_not_available", name));
                        return;
                    }
                    if (balance < cost)
                    {
                        UnturnedChat.Say(player, LIGHT.Instance.DefaultTranslations.Translate("not_enough_currency_msg",Uconomy.Instance.Configuration.Instance.MoneyName, name));
                        return;
                    }
                    if (!player.GiveVehicle(id))
                    {
                        UnturnedChat.Say(player, LIGHT.Instance.Translate("error_giving_item",name));
                        return;
                    }
                    decimal newbal = Uconomy.Instance.Database.IncreaseBalance(player.CSteamID.ToString(), (cost * -1));
                    if (OnShopBuy != null)
                        OnShopBuy(player, cost, 1, id, "vehicle");
                    player.Player.gameObject.SendMessage("ZaupShopOnBuy", new object[] { player, cost, amttobuy, id, "vehicle" }, SendMessageOptions.DontRequireReceiver);
                    UnturnedChat.Say(player, LIGHT.Instance.Translate("vehicle_buy_msg", name, cost, Uconomy.Instance.Configuration.Instance.MoneyName, newbal, Uconomy.Instance.Configuration.Instance.MoneyName));
                    break;
                default:
                    if (!LIGHT.Instance.Configuration.Instance.CanBuyItems)
                    {
                        UnturnedChat.Say(player, LIGHT.Instance.Translate("buy_items_off"));
                        return;
                    }
                    name = null;
                    if (!ushort.TryParse(Itemname, out id))
                    {
                        Asset[] array = Assets.find(EAssetType.ITEM);
                        Asset[] array2 = array;
                        for (int i = 0; i < array2.Length; i++)
                        {
                            ItemAsset vAsset = (ItemAsset)array2[i];
                            if (vAsset != null && vAsset.itemName != null && vAsset.itemName.ToLower().Contains(Itemname.ToLower()))
                            {
                                id = vAsset.id;
                                name = vAsset.itemName;
                                break;
                            }
                        }
                    }
                    if (name == null && id == 0)
                    {
                        UnturnedChat.Say(player, LIGHT.Instance.Translate("could_not_find", Itemname));
                        return;
                    }
                    else if (name == null && id != 0)
                    {
                        try
                        {
                            name = ((ItemAsset)Assets.find(EAssetType.ITEM, id)).itemName;
                        }
                        catch
                        {
                            UnturnedChat.Say(player, LIGHT.Instance.Translate("item_invalid"));
                            return;
                        }
                    }
                    cost = decimal.Round(LIGHT.Instance.ShopDB.GetItemCost(id) * amttobuy, 2);
                    if (LIGHT.Instance.Configuration.Instance.SaleEnable)
                    {
                        decimal saleprice = Convert.ToDecimal(Convert.ToDouble(LIGHT.Instance.Configuration.Instance.SalePercentage) / 100.00);
                        if (LIGHT.Instance.sale.salesStart == true)
                            cost = decimal.Round((cost * (Convert.ToDecimal(1.00) - saleprice)), 2);
                    }
                    balance = Uconomy.Instance.Database.GetBalance(player.Id);
                    if (cost <= 0m)
                    {
                        UnturnedChat.Say(player, LIGHT.Instance.Translate("item_not_available", name));
                        return;
                    }
                    if (balance < cost)
                    {
                        UnturnedChat.Say(player, LIGHT.Instance.Translate("not_enough_currency_msg", Uconomy.Instance.Configuration.Instance.MoneyName, amttobuy, name));
                        return;
                    }
                    player.GiveItem(id, amttobuy);
                    string[] freeItem = LIGHT.Instance.Database.GetFreeItem(caller.Id);
                    bool free = false;
                    for (int x = 0; x < freeItem.Length; x ++)
                    {
                        if(freeItem[x].Trim() == id.ToString().Trim())
                        {
                            free = true;
                            cost = 0;
                            break;
                        }
                    }
                    newbal = Uconomy.Instance.Database.IncreaseBalance(player.Id, (cost * -1));
                    if (OnShopBuy != null)
                        OnShopBuy(player, cost, amttobuy, id);
                    player.Player.gameObject.SendMessage("ZaupShopOnBuy", new object[] { player, cost, amttobuy, id, "item" }, SendMessageOptions.DontRequireReceiver);
                    if(free)
                        UnturnedChat.Say(player, LIGHT.Instance.Translate("item_buy_freemsg", amttobuy, name, LIGHT.Instance.Database.CheckUserGroup(caller.Id)));                   
                    else
                        UnturnedChat.Say(player, LIGHT.Instance.Translate("item_buy_msg", name, cost, Uconomy.Instance.Configuration.Instance.MoneyName, newbal, Uconomy.Instance.Configuration.Instance.MoneyName, amttobuy));
                    break;
            }
        }
        public event PlayerShopBuy OnShopBuy;
        public delegate void PlayerShopBuy(UnturnedPlayer player, decimal amt, byte items, ushort item, string type = "item");
    }
}