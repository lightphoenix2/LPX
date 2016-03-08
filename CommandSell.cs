using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using UnityEngine;
using fr34kyn01535.Uconomy;

namespace LIGHT
{
    public class CommandSell : IRocketCommand
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
                return "sell";
            }
        }
        public string Help
        {
            get
            {
                return "Allows you to sell items to the shop from your inventory.";
            }
        }
        public string Syntax
        {
            get
            {
                return "<name or id> [amount]";
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
                return new List<string>() {"sell"};
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
            if (command.Length == 0)
            {
                UnturnedChat.Say(player, LIGHT.Instance.Translate("sell_command_usage"));
                return;
            }
            byte amttosell = 1;
            byte amt = amttosell;
            ushort id;
            if (!LIGHT.Instance.Configuration.Instance.CanSellItems)
            {
                UnturnedChat.Say(player, LIGHT.Instance.Translate("sell_items_off"));
                return;
            }
            string name = null;
            ItemAsset vAsset = null;
            string ItemName = "";
            bool HaveAmt = false;
            if (command.Length > 1)
            {
                HaveAmt = byte.TryParse(command[command.Length - 1], out amt);
                if (!HaveAmt)
                {
                    amt = 1;
                    if (command.Length == 2)
                        ItemName = command[0] + " " + command[1];
                    else if (command.Length == 1)
                        ItemName = command[0];
                }
                else
                {
                    for (int i = 0; i < (command.Length - 1); i++)
                    {
                        if (i == (command.Length - 2))
                            ItemName += command[i];
                        else
                            ItemName += command[i] + " ";
                    }
                }
            }
            else
                ItemName = command[0].Trim();
            amttosell = amt;
            if (!ushort.TryParse(ItemName, out id))
            {
                Asset[] array = Assets.find(EAssetType.ITEM);
                Asset[] array2 = array;
                for (int i = 0; i < array2.Length; i++)
                {
                    vAsset = (ItemAsset)array2[i];
                    if (vAsset != null && vAsset.Name != null && vAsset.Name.ToLower().Contains(ItemName.ToLower()))
                    {
                        id = vAsset.Id;
                        name = vAsset.Name;
                        break;
                    }
                }
            }
            if (name == null && id == 0)
            {
                UnturnedChat.Say(player, LIGHT.Instance.Translate("could_not_find", ItemName));
                return;
            }
            else if (name == null && id != 0)
            {
                try
                {
                    vAsset = (ItemAsset)Assets.find(EAssetType.ITEM, id);
                    name = vAsset.Name;
                }
                catch
                {
                    UnturnedChat.Say(player, LIGHT.Instance.Translate("item_invalid"));
                    return;
                }
            }
            if (player.Inventory.has(id) == null)
            {
                UnturnedChat.Say(player, LIGHT.Instance.Translate("not_have_item_sell", name));
                return;
            }
            List<InventorySearch> list = player.Inventory.search(id, true, true);
            if (list.Count == 0 || (vAsset.Amount == 1 && list.Count < amttosell))
            {
                UnturnedChat.Say(player, LIGHT.Instance.Translate("not_enough_items_sell", amttosell.ToString(), name));
                return;
            }
            if (vAsset.Amount > 1)
            {
                int ammomagamt = 0;
                foreach (InventorySearch ins in list)
                {
                    ammomagamt += ins.ItemJar.Item.Amount;
                }
                if (ammomagamt < amttosell)
                {
                    UnturnedChat.Say(player, LIGHT.Instance.Translate("not_enough_ammo_sell", name));
                    return;
                }
            }
            // We got this far, so let's buy back the items and give them money.
            // Get cost per item.  This will be whatever is set for most items, but changes for ammo and magazines.
            decimal price = LIGHT.Instance.ShopDB.GetItemBuyPrice(id);
            if (price <= 0.00m)
            {
                UnturnedChat.Say(player, LIGHT.Instance.Translate("no_sell_price_set", name));
                return;
            }
            byte quality = 100;
            decimal peritemprice = 0;
            decimal addmoney = 0;
            switch (vAsset.Amount)
            {
                case 1:
                    // These are single items, not ammo or magazines
                    while (amttosell > 0)
                    {
                        if (player.Player.Equipment.checkSelection(list[0].InventoryGroup, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY))
                        {
                            player.Player.Equipment.dequip();
                        }
                        if (LIGHT.Instance.Configuration.Instance.QualityCounts)
                            quality = list[0].ItemJar.Item.Durability;
                        peritemprice = decimal.Round(price * (quality / 100.0m), 2);
                        addmoney += peritemprice;
                        player.Inventory.removeItem(list[0].InventoryGroup, player.Inventory.getIndex(list[0].InventoryGroup, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY));
                        list.RemoveAt(0);
                        amttosell--;
                    }
                    break;
                default:
                    // This is ammo or magazines
                    byte amttosell1 = amttosell;
                    while (amttosell > 0)
                    {
                        if (player.Player.Equipment.checkSelection(list[0].InventoryGroup, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY))
                        {
                            player.Player.Equipment.dequip();
                        }
                        if (list[0].ItemJar.Item.Amount >= amttosell)
                        {
                            byte left = (byte)(list[0].ItemJar.Item.Amount - amttosell);
                            list[0].ItemJar.Item.Amount = left;
                            player.Inventory.sendUpdateAmount(list[0].InventoryGroup, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY, left);
                            amttosell = 0;
                            if (left == 0)
                            {
                                player.Inventory.removeItem(list[0].InventoryGroup, player.Inventory.getIndex(list[0].InventoryGroup, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY));
                                list.RemoveAt(0);
                            }
                        }
                        else
                        {
                            amttosell -= list[0].ItemJar.Item.Amount;
                            player.Inventory.sendUpdateAmount(list[0].InventoryGroup, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY, 0);
                            player.Inventory.removeItem(list[0].InventoryGroup, player.Inventory.getIndex(list[0].InventoryGroup, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY));
                            list.RemoveAt(0);
                        }
                    }
                    peritemprice = decimal.Round(price * ((decimal)amttosell1 / (decimal)vAsset.Amount), 2);
                    addmoney += peritemprice;
                    break;
            }
            decimal balance = Uconomy.Instance.Database.IncreaseBalance(player.Id, addmoney);
            if (OnShopSell != null)
                OnShopSell(player, addmoney, amttosell, id);
            player.Player.gameObject.SendMessage("ZaupShopOnSell", new object[] { player, addmoney, amt, id }, SendMessageOptions.DontRequireReceiver);
            UnturnedChat.Say(player, LIGHT.Instance.Translate("sold_items",amt, name, addmoney, Uconomy.Instance.Configuration.Instance.MoneyName, balance, Uconomy.Instance.Configuration.Instance.MoneyName));
        }
        public delegate void PlayerShopSell(UnturnedPlayer player, decimal amt, byte items, ushort item);
        public event PlayerShopSell OnShopSell;
    }

}