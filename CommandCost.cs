using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using fr34kyn01535.Uconomy;

namespace LIGHT
{
    public class CommandCost : IRocketCommand
    {
        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Player;
            }
        }
        public string Name
        {
            get
            {
                return "cost";
            }
        }
        public string Help
        {
            get
            {
                return "Tells you the cost of a selected item.";
            }
        }
        public string Syntax
        {
            get
            {
                return "[v.]<name or id>";
            }
        }
        public List<string> Aliases
        {
            get { return new List<string>(); }
        }
        public List<string> Permissions
        {
            get 
            {
                return new List<string>() { "cost" }; 
            }
        }
        public void Execute(IRocketPlayer caller, params string[] command)
        {
            if (!LIGHT.Instance.Configuration.Instance.EnableShop)
            {
                UnturnedChat.Say(caller, LIGHT.Instance.Translate("shop_disable"));
                return;
            }
            UnturnedPlayer player = (UnturnedPlayer)caller;
            string message;
            if (command.Length == 0)
            {
                UnturnedChat.Say(player, LIGHT.Instance.Translate("cost_command_usage"));
                return;
            }
            if (command.Length == 2 && command[0] != "v")
            {
                UnturnedChat.Say(player, LIGHT.Instance.Translate("cost_command_usage"));
                return;
            }
            ushort id;
            switch (command[0])
            {
                case "v":
                    string name = null;
                    if (!ushort.TryParse(command[1], out id))
                    {
                        Asset[] array = Assets.find(EAssetType.VEHICLE);
                        Asset[] array2 = array;
                        for (int i = 0; i < array2.Length; i++)
                        {
                            VehicleAsset vAsset = (VehicleAsset)array2[i];
                            if (vAsset != null && vAsset.name != null && vAsset.name.ToLower().Contains(command[1].ToLower()))
                            {
                                id = vAsset.id;
                                name = vAsset.name;
                                break;
                            }
                        }
                    }
                    if (name == null && id == 0)
                    {
                        UnturnedChat.Say(player, LIGHT.Instance.Translate("could_not_find",command[1]));
                        return;
                    }
                    else if (name == null && id != 0)
                    {
                        try
                        {
                            name = ((VehicleAsset)Assets.find(EAssetType.VEHICLE, id)).name;
                        }
                        catch
                        {
                            UnturnedChat.Say(player, LIGHT.Instance.Translate("could_not_find", command[1]));
                            return;
                        }
                    }
                    decimal cost = LIGHT.Instance.ShopDB.GetVehicleCost(id);
                    message = LIGHT.Instance.Translate("vehicle_cost_msg", new object[] { name, cost.ToString(), Uconomy.Instance.Configuration.Instance.MoneyName });
                    if (cost <= 0m)
                    {
                        message = LIGHT.Instance.Translate("error_getting_cost", new object[] { name });
                    }
                    UnturnedChat.Say(player, message);
                    break;
                default:
                    name = null;
                    if (!ushort.TryParse(command[0], out id))
                    {
                        Asset[] array = Assets.find(EAssetType.ITEM);
                        Asset[] array2 = array;
                        for (int i = 0; i < array2.Length; i++)
                        {
                            ItemAsset iAsset = (ItemAsset)array2[i];
                            if (iAsset != null && iAsset.name != null && iAsset.name.ToLower().Contains(command[0].ToLower()))
                            {
                                id = iAsset.id;
                                name = iAsset.name;
                                break;
                            }
                        }
                    }
                    if (name == null && id == 0)
                    {
                        UnturnedChat.Say(player, LIGHT.Instance.Translate("could_not_find",command[0]));
                        return;
                    }
                    else if (name == null && id != 0)
                    {
                        try
                        {
                            name = ((ItemAsset)Assets.find(EAssetType.ITEM, id)).name;
                        }
                        catch
                        {
                            UnturnedChat.Say(player, LIGHT.Instance.Translate("could_not_find", command[0]));
                            return;
                        }
                    }
                    cost = LIGHT.Instance.ShopDB.GetItemCost(id);
                    decimal bbp = LIGHT.Instance.ShopDB.GetItemBuyPrice(id);
                    if (LIGHT.Instance.Configuration.Instance.SaleEnable)
                    {
                        decimal saleprice = Convert.ToDecimal(Convert.ToDouble(LIGHT.Instance.Configuration.Instance.SalePercentage) / 100.00);
                        if (LIGHT.Instance.sale.salesStart == true)
                            cost = (cost * (Convert.ToDecimal(1.00) - saleprice));
                    }
                    message = LIGHT.Instance.Translate("item_cost_msg", new object[] { name, cost.ToString(), Uconomy.Instance.Configuration.Instance.MoneyName, bbp.ToString(), Uconomy.Instance.Configuration.Instance.MoneyName });
                    if (cost <= 0m)
                        message = LIGHT.Instance.Translate("error_getting_cost", new object[] { name });
                    UnturnedChat.Say(player, message);
                    break;
            }
        }
    }
}