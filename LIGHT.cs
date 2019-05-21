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
using SDG.Unturned;
using fr34kyn01535.Uconomy;
using System;
using System.Collections.Generic;
using Logger = Rocket.Core.Logging.Logger;

namespace LIGHT
{
    public class LIGHT : RocketPlugin<Configuration>
    {
        public DatabaseManager Database;
        public DatabaseManagerShop ShopDB;
        public DatabaseManagerAuction DatabaseAuction;
        public DatabaseCar DatabaseCar;
        public static LIGHT Instance;
        static IRocketPermissionsProvider OriginalPermissions;
        DateTime logouttime;
        public static Dictionary<string, DateTime> PlayerCooldown = new Dictionary<string, DateTime>();
        public static Dictionary<string, DateTime> PlayerCooldownPerKit = new Dictionary<string, DateTime>();
        private readonly Dictionary<string, ushort> _lastVehicleStates = new Dictionary<string, ushort>();
        public Sales sale;
        private DateTime LastVehicleCheck;

        protected override void Load()
        {
            LastVehicleCheck = DateTime.Now;
            Instance = this;
            if(LIGHT.Instance.Configuration.Instance.EnableShop)
                ShopDB = new DatabaseManagerShop();                  
            OriginalPermissions = R.Permissions;
            if (LIGHT.Instance.Configuration.Instance.LPXEnabled)
            {
                SQLPermission permission = new SQLPermission();               
                R.Permissions = permission;
                U.Events.OnPlayerConnected += RocketServerEvents_OnPlayerConnected;
                U.Events.OnPlayerDisconnected += RocketServerEvents_OnPlayerDisconnected;                
            }
            Database = new DatabaseManager();
            DatabaseCar = new DatabaseCar();
            if (LIGHT.Instance.Configuration.Instance.KitsEnabled && LIGHT.Instance.Configuration.Instance.KitCooldownResetOnDeath)
                UnturnedPlayerEvents.OnPlayerDead += RocketServerEvents_OnPlayerDead;
            if (LIGHT.Instance.Configuration.Instance.AllowAuction)
                DatabaseAuction = new DatabaseManagerAuction();
            if(LIGHT.Instance.Configuration.Instance.AutoRemoveEnabled)
                LIGHT.Instance.Database.AutoRemove();
            if(LIGHT.Instance.Configuration.Instance.AllowCarOwnerShip)
                UnturnedPlayerEvents.OnPlayerUpdateStance += OnPlayerUpdateStance;
            if (LIGHT.Instance.Configuration.Instance.SaleEnable && LIGHT.Instance.Configuration.Instance.EnableShop)
            {
                sale = new Sales(); 
                sale.resetSale();
            }
            if(LIGHT.Instance.Configuration.Instance.EnableShop && LIGHT.Instance.Configuration.Instance.EnableAutoDBUpdate)
            {
                ItemAsset ia;
                Logger.Log("Loading Item Database...", ConsoleColor.Yellow);
                for(ushort x = 0; x < 60000; x++)
                {
                    try
                    {                     
                        ia = (ItemAsset)Assets.find(EAssetType.ITEM, x);                       
                        if(ia.itemName != "" && ia.itemName != " ")
                            LIGHT.Instance.ShopDB.AutoAddItem((int)ia.id, ia.name, LIGHT.Instance.ShopDB.CheckItemExist(x));
                        LIGHT.Instance.ShopDB.DeleteEmptyItemRow(x);
                        if(x == 500)
                            Logger.Log("We are halfway there...", ConsoleColor.Yellow);
                    }
                    catch
                    {
                        LIGHT.Instance.ShopDB.DeleteItem(x);
                    }
                }
                Logger.Log("Item Database Updated!", ConsoleColor.Yellow);
                VehicleAsset va;
                Logger.Log("Loading Vehicle Database...", ConsoleColor.Yellow);
                for (ushort x = 0; x < 20000; x++)
                {
                    try
                    {
                        va = (VehicleAsset)Assets.find(EAssetType.VEHICLE, x);
                        if (va.vehicleName != "" && va.vehicleName != " ")
                            LIGHT.Instance.ShopDB.AutoAddVehicle((int)va.id, va.vehicleName, LIGHT.Instance.ShopDB.CheckVehicleExist(x));
                        LIGHT.Instance.ShopDB.DeleteEmptyVehicleRow(x);
                        if (x == 50)
                            Logger.Log("Should be quick...", ConsoleColor.Yellow);
                    }
                    catch
                    {
                        LIGHT.Instance.ShopDB.DeleteVehicle(x);
                    }
                }
                Logger.Log("Vehicle Database Updated!", ConsoleColor.Yellow);
            }
            /** Ajout du reset des licences **/
            UnturnedPlayerEvents.OnPlayerDeath += Instance.DatabaseCar.resetLicence;

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
                {"lpx_invaild_para","Invalid Parameter!"},
                {"lpx_group_notexist","Invalid Parameter!"},
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
                {"lpx_failed_copypermission","Permission: {0} already exist in Group: {1}!"},
                {"lpx_help_addgroup","/lpx addgroup <name> <income> <parentgroup> <updategroup> <updatetime> <update enable>"},
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
                {"lpx_remove_removepermission","Sucessfully removed {0} from group {1}!"},
                {"lpx_failed_removepermission","Failed to remove permission from group {0}!"},
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
                {"lpx_failed_setincome","Fail to set income to Group: {0}"},
                {"lpx_help_addparentgroup","/lpx addparentgroup <group> <parentgroup>"},
                {"lpx_added_parentgroup", "Successfully added Parent Group:{1} to group {0}"},
                {"lpx_failed_parentgroup", "Fail to added Parent Group:{1} to group {0}"},
                {"lpx_add_sameparentgroup","Unable to add same group to be Parent Group of that group!"},
                {"lpx_promotion","You have been added to Group: {0} for playing for more than {1} hours"},
                {"lpx_help_setpromotegroup","/lpx setpromotegroup <group> <updategroup>"},
                {"lpx_set_promotegroup", "Successfully set Promote Group:{1} to group {0}"},
                {"lpx_failed_promotegroup", "Fail to set Promote Group:{1} to group {0}"},
                {"lpx_help_setpromotetime","/lpx setpromotetime <group> <hour>"},
                {"lpx_set_promotetime","Successfully set Promote Time to {1} for group {0}"},
                {"lpx_failed_promotegroup", "Fail to set Promote Time to group {0}"},
                {"lpx_help_enablepromote","/lpx enablepromote <group> <true or false>"},
                {"lpx_set_enablepromote","Successfully set Enable Promote to {1} for group {0}"},
                {"lpx_failed_enablepromote", "Fail to set Enable Promote to group {0}"},
                {"lpx_playernotfound","Unable to find player with that name"},
                {"lpx_invalid_input","Invalid Input"},
                {"kit_help", "/Kit <kit Name>"},
                {"kit_given", "You have received the kit {0}!"},
                {"kit_notexist", "Kit {0} does not exist"},
                {"kit_cooldown", "Please wait {0} seconds before using {1} kit again"},
                {"kit_list", "Kit that are available to you are : {0}"},
                {"kit_list_all","{0}"},
                {"kits_chngcd_help","/kits cooldown <Kit Name> <New Cooldown>"},
                {"kits_chngcd_help2","Missing <New Cooldown> Parameter"},
                {"kits_failed_chngcd","Fail to change cooldown of {0}!"},
                {"kits_success_chngcd","Successful change {0}'s cooldown to {1}"},
                {"kits_add_help","/kits add <Kit Name> <ItemId/Amount ....>"},
                {"kits_error_cooldown","Error in changing cooldown. Make sure it is numeric!"},
                {"kits_add_success","Successfully Added Kit: {0}"},
                {"kits_add_failed","Failed to Add Kit: {0}"},
                {"kits_remove_help","/kits remove <Kit Name>"},
                {"kits_remove_failed","Fail to remove kit {0}!"},
                {"kits_remove_success","Kit {0} removed successfully"},
                {"lpx_car_ownerfalse","You do not own that vehicle"},
                {"lpx_car_promotebuy","This car, licence: {0}, is available to be purchased"},
                {"lpx_car_ownertrue","Welcome back, please have a safe journey!"},
                {"lpx_car_buyhelp","/buyv <Car No>"},
                {"lpx_car_alreadyown","This car belongs to someone else"},
                {"lpx_car_purchaseFailed","Fail to purchase car, Payment is cancelled."},
                {"lpx_car_purchased","You bought car licence: {0} for {1}{2}!"},
                {"car_error_1","You need to be inside the vehicle"},
                {"car_refuelled_1","Car refuel by {0}%"},
                {"car_refuel_error2","Invalid Fuel Percent, only 1-100%"},
                {"car_not_enough_currency", "You do not have enough {0} to made this payment!"},
                {"car_refuel_paid","You have paid {0}{1} for Car No: {2}. Car refuel by {3}%"},
                {"car_help_1","/car refuel <amount>, /car lock, /car unlock, /car repair, /car locate <Car No>, /car key <Car No> <player name>, /car rkey <Car No> <player name>, /car list"},
                {"car_refuel_help_1","Missing Amount Parameter, Please enter 1-100% or Full or All"},
                {"car_commmand_notowner","This car does not belong to you"},
                {"car_Locked", "You have locked Car No: {0}"},
                {"car_Unlocked", "You have unlocked Car No: {0}. Other players can drive it!"},
                {"car_commmand_keyhelp","/car key <Car No> <player name>"},
                {"car_commmand_keyhelp2","Missing <player name> parameter"},
                {"car_not_found","Unable to locate the vehicle"},
                {"car_key_given","You have given your Car No:{0} key to {1}"},
                {"car_key_used","You have used Car No:{0} Key"},
                {"car_rkey_help","/car rkey <carNo> <player Name or SteamID>"},
                {"car_rkey_help2","Parameter <player Name or SteamID> is needed. Enter <all> to remove all given keys."},
                {"car_rkey_success","You removed key from Steam ID:{0} for Car No: {1}"},
                {"car_rkey_keynotfound","Unable to find key for Car No: {0}"},
                {"car_zero_owned","You do not own any vehicle"},
                {"car_list_all","Car No: {0}"},
                {"car_not_locked","You have entered {0} car"},
                {"car_repair_notneeded","This vehicle No:{0} is already fully repaired"},
                {"car_repaired","Vehicle No:{0} have being repaired by {1}%"},
                {"car_repaired_price","Vehicle No:{0} have being repaired by {1}% for {2}{3}"},
                {"car_locate_help","/car locate <Car No>"},
                {"car_located","The Car No: {0} is at X:{1} Y:{2} Z:{3}, {4}"},
                {"shop_disable","Shop is disabled!"},
                {"buy_command_usage", "Usage: /buy [v.]<name or id> [amount] [quality: 25/50/75/100] (Default amount: 1, Quality: 100)"},
                {"cost_command_usage", "Usage: /cost [v.]<name or id>."},
                {"sell_command_usage", "Usage: /sell <name or id> [amount] (optional)."},
                {"shop_command_usage", "Usage: /shop <add/rem/chng/buy> [v.]<itemid> <cost>  <cost> is not required for rem, buy is only for items."},
                {"error_giving_item", "There was an error giving you {0}.  You have not been charged."},
                {"error_getting_cost", "There was an error getting the cost of {0}!"},
                {"item_cost_msg", "The item {0} costs {1} {2} to buy and gives {3} {4} when you sell it."},
                {"vehicle_cost_msg", "The vehicle {0} costs {1} {2} to buy."},
                {"item_buy_msg", "You have bought {5} {0} for {1} {2}.  You now have {3} {4}."},
                {"item_buy_freemsg","You got {0} {1} for free, because you are in group {2}"},
                {"item_invalid","Invalid Item Name or ID!"},
                {"vehicle_buy_msg", "You have bought 1 {0} for {1} {2}.  You now have {3} {4}."},
                {"vehicle_invalid","Invalid Vehicle ID or Name!"},
                {"not_enough_currency_msg", "You do not have enough {0} to buy {1} {2}."},
                {"buy_items_off", "I'm sorry, but the ability to buy items is turned off."},
                {"buy_vehicles_off", "I'm sorry, but the ability to buy vehicles is turned off."},
                {"item_not_available", "I'm sorry, but {0} is not available in the shop."},
                {"vehicle_not_available", "I'm sorry, but {0} is not available in the shop."},
                {"could_not_find", "I'm sorry, I couldn't find an id for {0}."},
                {"sell_items_off", "I'm sorry, but the ability to sell items is turned off."},
                {"not_have_item_sell", "I'm sorry, but you don't have any {0} to sell."},
                {"not_enough_items_sell", "I'm sorry, but you don't have {0} {1} to sell."},
                {"not_enough_ammo_sell", "I'm sorry, but you don't have enough ammo in {0} to sell."},
                {"sold_items", "You have sold {0} {1} to the shop and receive {2} {3} in return.  Your balance is now {4} {5}."},
                {"no_sell_price_set", "The shop is not buying {0} right now"},
                {"no_itemid_given", "An itemid is required."},
                {"no_cost_given", "A cost is required."},
                {"v_not_provided", "You must specify v for vehicle or just an item id.  Ex. /shop rem/101"},
                {"invalid_id_given", "You need to provide an item or vehicle id."},
                {"no_permission_shop_chng", "You don't have permission to use the shop chng msg."},
                {"no_permission_shop_add", "You don't have permission to use the shop add msg."},
                {"no_permission_shop_rem", "You don't have permission to use the shop rem msg."},
                {"no_permission_shop_buy", "You don't have permission to use the shop buy msg."},
                {"changed", "changed"},
                {"added", "added"},
                {"changed_or_added_to_shop", "You have {0} the {1} with cost {2} to the shop."},
                {"error_adding_or_changing", "There was an error adding/changing {0}!"},
                {"removed_from_shop", "You have removed the {0} from the shop."},
                {"not_in_shop_to_remove", "{0} wasn't in the shop, so couldn't be removed."},
                {"not_in_shop_to_set_buyback", "{0} isn't in the shop so can't set a buyback price."},
                {"set_buyback_price", "You set the buyback price for {0} to {1} in the shop."},
                {"invalid_shop_command", "You entered an invalid shop command."},
                {"sale_start", "The sale is starting in {0} seconds!"},
				{"sale_started", "The sale has started and will end in {0} minutes! Everything in the shop is now on sale!"},
                {"sale_command","The next sale will start in {0} Minutes"},
                {"sale_end","Sales have ended, all price are back to normal"},
                {"sale_ending", "Sale is ending in {0} {1}"},
                {"auction_command_usage","/auction add, /auction list, /auction find, /auction cancel"},
                {"auction_addcommand_usage","/auction <Item Name or ID> <Price>"},
                {"auction_addcommand_usage2","Missing <Price> Parameter"},
                {"auction_disabled","Auction is disabled."},
                {"not_have_item_auction", "You do not have any {0} for auctioning."},
                {"auction_item_notinshop","The item you are auctioning is not available from the shop."},
                {"auction_item_mag_ammo","Unable to auction magazines or ammo!"},
                {"auction_item_succes","You have placed {0} on auction for {1} {2}"},
                {"auction_item_failed","Fail to place item on auction"},
                {"auction_unequip_item","Please de-equip {0} first before auctioning"},
                {"auction_buycommand_usage","/auction buy <ID 0 - 9 ...>"},
                {"auction_addcommand_idnotexist","Auction ID does not exist!"},
                {"auction_buy_msg","You got item {0} for {1} {2}.  You now have {3} {4}."},
                {"auction_notexist","Auction ID does not exist"},
                {"auction_notown","You do not own that auction!"},
                {"auction_cancelled","You have remove Auction {0}"},
                {"auction_cancelcommand_usage","/auction cancel [Auction ID]"},
                {"auction_findcommand_usage","/auction find [Item Name or ID]"},
                {"auction_find_invalid","Invalid Item ID or Item Name"},
                {"auction_find_failed","No item found with that ID/Name"},
                {"lpx_car_licenceOK","Vous avez recu la licence n°"},
                {"lpx_car_Forbidden","Vehicule Interdit"},
                {"car_steal_help","/car steal [ID]"}
                };
            }
        }
        public bool checkPermission(string PermissionCmd, string ID)
        {          
            string[] permission = { };
            bool hasPerm = false;
            permission = LIGHT.Instance.Database.getGroupPermission(LIGHT.Instance.Database.CheckUserGroup(ID));
            for (int i = permission.Length - 1; i >= 0; i--)
            {
                if (permission[i] == PermissionCmd)
                    hasPerm = true;
            }
            return hasPerm;
        }
        public bool checkPermission(List <string> PermissionCmd, string ID)
        {
            string[] permission = { };
            bool hasPerm = false;
            permission = LIGHT.Instance.Database.getGroupPermission(LIGHT.Instance.Database.CheckUserGroup(ID));
            for (int i = permission.Length - 1; i >= 0; i--)
            {
                if (PermissionCmd.Contains(permission[i]))
                    hasPerm = true;
            }
            return hasPerm;
        }
        public bool checkPermission(IRocketCommand Command, string ID)
        {
            string[] permission = { }, cmdPermission = { }, Permi = { };
            bool hasPerm = false;
            permission = LIGHT.Instance.Database.getGroupPermission(LIGHT.Instance.Database.CheckUserGroup(ID));
            cmdPermission = Command.Permissions.ToArray();
            if (cmdPermission.Length < 1)
                return true;
            else
            {
                for (int x = cmdPermission.Length - 1; x >= 0; x--)
                {
                    for (int i = permission.Length - 1; i >= 0; i--)
                    {
                        if (cmdPermission[x].Contains('.'))
                        {
                            Permi = cmdPermission[x].Split('.');
                            if (permission[i].Trim() == Permi[1].Trim())
                                return true;
                        }
                        if (cmdPermission[x] == (permission[i]))
                                return true;
                    }
                }
                
            }
            return hasPerm;
        }
        public void RocketServerEvents_OnPlayerConnected(UnturnedPlayer player)
        {
            string[] permission = { };
            string[] cmd = {};
            if (player.IsAdmin && LIGHT.Instance.Database.CheckUserGroup(player.Id) != "admin")
            {
                LIGHT.Instance.Database.AddUserIntoGroup(player.Id, "admin");
            }
            if (LIGHT.Instance.Database.CheckIfUserInAnyGroup(player.Id) == false && LIGHT.Instance.Configuration.Instance.AutoAddDefault)
            {
                LIGHT.Instance.Database.AddUserIntoGroup(player.Id, "default");
            }
            permission = LIGHT.Instance.Database.getPermission(player.Id);
            for (int i = permission.Length - 1; i >= 0; i--)
            {
                if (permission[i].Contains("color.") && !(player.IsAdmin))
                {                   
                    cmd = permission[i].Split('.');
                    Color? color = UnturnedChat.GetColorFromName(cmd[1].Trim(),  Color.white);
                    if (color.Value == Color.white && player.IsPro)
                        color = Color.yellow;
                    player.Color = color.Value;
                }
            }       
            LIGHT.Instance.Database.LastLogin(player.Id);
            LIGHT.Instance.Database.SetSteamName(player.Id, player.SteamName);
            if(LIGHT.Instance.Database.CheckEnablePromotion(player.Id) && !(player.IsAdmin))
            {
                decimal TotalHour = LIGHT.Instance.Database.GetTotalOnlineHours(player.Id);
                string PromotedGroup = LIGHT.Instance.Database.GetUpdateGroup(player.Id);
                if(TotalHour >= LIGHT.Instance.Database.GetUpdateTime(player.Id))
                {
                    LIGHT.Instance.Database.AddUserIntoGroup(player.Id, PromotedGroup);
                    UnturnedChat.Say(player.CSteamID, LIGHT.Instance.Translate("lpx_promotion", PromotedGroup, TotalHour));
                }
            }         
        }
        public void RocketServerEvents_OnPlayerDisconnected(UnturnedPlayer player)
        {
            logouttime = DateTime.Now;
            LIGHT.Instance.Database.HoursOnline(player.Id, logouttime);
        }
        private void OnPlayerUpdateStance(UnturnedPlayer player, byte stance)
        {
            
            if (stance == (byte)6 && LIGHT.Instance.Configuration.Instance.AllowCarOwnerShip)
            {
                InteractableVehicle veh = player.Player.movement.getVehicle();            
                if (LIGHT.Instance.DatabaseCar.CheckCarExistInDB(veh.instanceID.ToString()))
                {
                    if (LIGHT.Instance.DatabaseCar.CheckCarDestoryed(veh.instanceID.ToString()) != veh.id.ToString())
                    {
                        uint CheckIDOld = veh.instanceID + 1;
                        string PlayerID = LIGHT.Instance.DatabaseCar.GetOwnerID((CheckIDOld).ToString());
                        string CarID = LIGHT.Instance.DatabaseCar.GetCarID((CheckIDOld).ToString());
                        if(PlayerID == player.Id && CarID == veh.id.ToString())
                        {
                            LIGHT.Instance.DatabaseCar.RemovedDestoryedCar(veh.instanceID.ToString());
                            LIGHT.Instance.DatabaseCar.RemovedDestoryedCar(CheckIDOld.ToString());
                            LIGHT.Instance.DatabaseCar.AddOwnership(veh.instanceID.ToString(), player.Id, player.SteamName);
                            return;
                        }
                        else 
                        {
                            for (int x = 0; x < 10; x++)
                            {
                                CheckIDOld++;
                                PlayerID = LIGHT.Instance.DatabaseCar.GetOwnerID(CheckIDOld.ToString());
                                CarID = LIGHT.Instance.DatabaseCar.GetCarID(CheckIDOld.ToString());
                                if (PlayerID == player.Id && CarID == veh.id.ToString())
                                {
                                    LIGHT.Instance.DatabaseCar.RemovedDestoryedCar(veh.instanceID.ToString());
                                    LIGHT.Instance.DatabaseCar.RemovedDestoryedCar(CheckIDOld.ToString());
                                    LIGHT.Instance.DatabaseCar.AddOwnership(veh.instanceID.ToString(), player.Id, player.SteamName);
                                    return;
                                }
                            }
                        }
                        LIGHT.Instance.DatabaseCar.RemovedDestoryedCar(veh.instanceID.ToString());
                        if (!LIGHT.Instance.Configuration.Instance.DriveUnownedCar) // Vehicule avec proprio
                        {
                            byte seat = 0;
                            foreach (Passenger p in player.Player.movement.getVehicle().passengers)
                            {
                                if (p.player.playerID.steamID.ToString() == player.Id)
                                {
                                    break;
                                }
                                seat++;
                            }
                            VehicleManager.sendExitVehicle(veh, seat, pos, 0, false);
                        }
                        LIGHT.Instance.DatabaseCar.AddCar(veh.instanceID.ToString(), veh.id.ToString(), veh.name);
                        UnturnedChat.Say(player.CSteamID, LIGHT.Instance.Translate("lpx_car_promotebuy", veh.instanceID.ToString()));
                        return;
                    }
                    if (LIGHT.Instance.DatabaseCar.CheckOwner(veh.instanceID.ToString()) == "") // Possibilite d'achat
                    {
                        UnturnedChat.Say(player.CSteamID, LIGHT.Instance.Translate("lpx_car_promotebuy", veh.instanceID.ToString()));
                        if (!LIGHT.Instance.Configuration.Instance.DriveUnownedCar)
                        {
                            byte seat = 0;
                            foreach (Passenger p in player.Player.movement.getVehicle().passengers)
                            {
                                if (p.player.playerID.steamID.ToString() == player.Id)
                                {
                                    break;
                                }
                                seat++;
                            }
                            veh.forceRemoveAllPlayers();
                        }
                        return;
                    }
                    if (player.Id != LIGHT.Instance.DatabaseCar.CheckOwner(veh.instanceID.ToString()))
                    {
                        string[] PlayersWithKey = LIGHT.Instance.DatabaseCar.GetGivenKeys(veh.instanceID.ToString());
                        for (int x = 0; x < PlayersWithKey.Length; x++)
                        {
                            if (PlayersWithKey[x].Trim() == player.Id)
                            {
                                UnturnedChat.Say(player, LIGHT.Instance.Translate("car_key_used", veh.instanceID.ToString()));
                                return;
                            }
                        }
                        if (LIGHT.Instance.DatabaseCar.CheckLockedStatus(veh.instanceID.ToString()))
                        {
                            byte seat = 0;
                            foreach (Passenger p in player.Player.movement.getVehicle().passengers)
                            {
                                if (p.player.playerID.steamID.ToString() == player.Id)
                                {
                                    break;
                                }
                                seat++;
                            }
                            veh.forceRemoveAllPlayers();
                            UnturnedChat.Say(player.CSteamID, LIGHT.Instance.Translate("lpx_car_ownerfalse"));
                            UnturnedChat.Say(player.CSteamID, LIGHT.Instance.Translate("lpx_car_promotebuy", veh.instanceID.ToString()));
                        }
                        else
                        {
                            UnturnedChat.Say(player.CSteamID, LIGHT.Instance.Translate("car_not_locked", LIGHT.Instance.DatabaseCar.GetOwnerName(veh.instanceID.ToString())));
                        }
                    }
                    else
                    {
                        /** A la voiture mais pas forcement la licence **/
                        decimal[] lLicencePrice = { 500, 750,/**/1000, 1500, 3500,/**/1250, 1750, 3750,/**/1500, 2000, 4000, 10000,/**/90000 };
                        decimal licenceprice = 0;
                        
                        string strLicence = Instance.DatabaseCar.GetLicence(veh.id.ToString());
                        int numLicence = Instance.DatabaseCar.convertLicenceToInt(strLicence);
                        /** VERIFICATION LICENCE **/
                        bool HasLicence = LIGHT.Instance.DatabaseCar.CheckLicence(player.Id, strLicence);
                        if (!HasLicence && !player.HasPermission("Licence_" + strLicence))
                        {
                            licenceprice = lLicencePrice.ElementAt(numLicence);

                            /** COMPARAISON PRIX **/
                            decimal balance = Uconomy.Instance.Database.GetBalance(player.Id);
                            if (balance < licenceprice)
                            {
                                UnturnedChat.Say(player, "Il vous faut " + licenceprice + " pour conduire de nouveau");
                                veh.forceRemoveAllPlayers();
                                return;
                            }
                            /** PAIEMENT **/
                            decimal bal = Uconomy.Instance.Database.IncreaseBalance(player.Id, (licenceprice * -1));
                            if (bal >= 0.0m)
                                UnturnedChat.Say(player, LIGHT.Instance.Translate("new_balance_msg", new object[] { bal, Uconomy.Instance.Configuration.Instance.MoneyName }));
                        }
                        if (!HasLicence)
                        {
                            LIGHT.Instance.DatabaseCar.AddLicenceToPlayer(player.Id, strLicence);
                            UnturnedChat.Say(player, LIGHT.Instance.Translate("lpx_car_licenceOK") + numLicence.ToString());
                        }
                    }
                }
                else
                {
                    if (!LIGHT.Instance.Configuration.Instance.DriveUnownedCar)
                    {
                        byte seat = 0;
                        foreach (Passenger p in player.Player.movement.getVehicle().passengers)
                        {
                            if (p.player.playerID.steamID.ToString() == player.Id)
                            {
                                break;
                            }
                            seat++;
                        }
                        veh.forceRemoveAllPlayers();
                    }
                    LIGHT.Instance.DatabaseCar.AddCar(veh.instanceID.ToString(), veh.id.ToString(), veh.name);
                    UnturnedChat.Say(player.CSteamID, LIGHT.Instance.Translate("lpx_car_list_all", veh.instanceID.ToString()));
                }

            }           
        }
        public void RocketServerEvents_OnPlayerDead(UnturnedPlayer player, Vector3 Position)
        {
            if (LIGHT.Instance.Configuration.Instance.KitCooldownApplyToAll)
            {
                if (PlayerCooldown.ContainsKey(player.Id))
                {                            
                    PlayerCooldown.Remove(player.Id);
                }
                else
                    return;
            }
            else
            {
                if (LIGHT.PlayerCooldownPerKit.ContainsKey(player.Id))
                {
                    string[] kitname = Database.GetPlayerKitName(Database.CheckUserGroup(player.Id));
                    for (int x = 0; x < kitname.Length; x++)
                    {
                        try
                        {
                            PlayerCooldownPerKit.Remove(player.Id + kitname[x]);
                        }
                        catch
                        {

                        }
                    }
                }
                else
                    return;
            }
        }
        public void FixedUpdate()
        {
            Instance = this;
            if (LIGHT.Instance.Configuration.Instance.SaleEnable && LIGHT.Instance.Configuration.Instance.EnableShop) sale.checkSale();
        }
    }
}