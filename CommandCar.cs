using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using System;
using Rocket.API;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;
using Rocket.Core.Logging;
using fr34kyn01535.Uconomy;
using Logger = Rocket.Core.Logging.Logger;

namespace LIGHT
{
    public class CommandCar : IRocketCommand
    {
        
        public string Help
        {
            get { return "Vehicle commands"; }
        }

        public string Name
        {
            get { return "car"; }
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
            get { return "<Car No> <player>"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "car.*", "car", "car.refuel", "car.repair"};
            }
        }
       
        public void Execute(IRocketPlayer caller,params string[] command)
        {
            bool hasPerm = false;
            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_help_1"));
                return;
            }
            if (command.Length > 0)
            {

                if (player.HasPermission("car") || player.HasPermission("car.*") || player.HasPermission("*"))
                {
                    hasPerm = true;
                }
                if (!hasPerm && !(caller.IsAdmin))
                {
                    UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_no_perm"));
                    return;
                }
                else
                {
                    InteractableVehicle veh = player.Player.movement.getVehicle();                    
                    switch (command[0].ToLower())
                    {
                        case "refuel":
                            if (player.HasPermission("car.refuel") || player.HasPermission("car.*") || player.HasPermission("*"))
                                hasPerm = true;
                            if (!hasPerm && !(caller.IsAdmin))
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_no_perm"));
                                return;
                            }
                            if(command.Length < 2)
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_refuel_help_1"));
                                return;
                            }
                            else
                            {
                                
                                if (veh != null)
                                {
                                    VehicleAsset vehi = veh.asset;
                                    ushort fuel = vehi.fuel;
                                    ushort maxfuel = vehi.fuel;
                                    double percent = 0;
                                    double truefuel = 0;
                                    if (command[1] == "full" || command[1] == "all" || command[1] == "100")
                                    {
                                        truefuel = (double)fuel - (double)veh.fuel;
                                        percent = double.Parse(fuel.ToString()) / (double)maxfuel * 100.00;
                                        percent = Math.Round(percent, 2);
                                    }
                                    else
                                    {
                                        fuel = 0;                                        
                                        if (ushort.TryParse(command[1], out fuel))
                                        {

                                            if ((((double)veh.fuel / (double)maxfuel * 100.00) + (double)fuel) > 100.00)
                                            {
                                                truefuel = (double)maxfuel - (double)veh.fuel;
                                            }
                                            else
                                            {
                                                truefuel = (double)fuel / 100.00 * (double)maxfuel;
                                            }
                                            truefuel = Math.Round(truefuel, 0);
                                            percent = Math.Round(((double)truefuel / (double)maxfuel) * 100.00, 2);

                                        }
                                        else
                                        {
                                            UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_refuel_error2"));
                                            return;
                                        }

                                    }
                                    decimal price = decimal.Parse("1.3");
                                    decimal.TryParse(LIGHT.Instance.Configuration.Instance.FuelPrice.ToString(), out price);
                                    if (price != 0.00m)
                                    {
                                        decimal balance = Uconomy.Instance.Database.GetBalance(player.Id);
                                        decimal totalprice = Math.Round(price * (decimal)truefuel / (decimal)10, 2);
                                        if (balance < totalprice)
                                        {
                                            UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_not_enough_currency", Uconomy.Instance.Configuration.Instance.MoneyName));
                                            return;
                                        }
                                        decimal bal = Uconomy.Instance.Database.IncreaseBalance(player.CSteamID.ToString(), (totalprice * -1));
                                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_refuel_paid", totalprice, Uconomy.Instance.Configuration.Instance.MoneyName, veh.instanceID.ToString(), percent.ToString()));
                                        if (bal >= 0.0m) UnturnedChat.Say(player.CSteamID, LIGHT.Instance.Translate("new_balance_msg", new object[] { bal, Uconomy.Instance.Configuration.Instance.MoneyName }));
                                    }
                                    else
                                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_refuelled_1", percent.ToString()));
                                    veh.askFillFuel((ushort)truefuel);                                                                           
                                }
                                else
                                {
                                    UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_error_1"));
                                    return;
                                }
                            }
                            break;
                        case "lock":
                            if (veh != null)
                            {
                                if (command.Length == 1)
                                {
                                    if (LIGHT.Instance.DatabaseCar.CheckOwner(veh.instanceID.ToString()) == player.Id)
                                    {
                                        LIGHT.Instance.DatabaseCar.AddLockedStatus(veh.instanceID.ToString(), true);
                                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_Locked", veh.instanceID.ToString()));
                                    }
                                    else
                                    {
                                        string[] PlayersWithKey = LIGHT.Instance.DatabaseCar.GetGivenKeys(veh.instanceID.ToString());
                                        for(int x = 0; x < PlayersWithKey.Length; x++)
                                        {
                                            if(PlayersWithKey[x].Trim() == player.Id)
                                            {
                                                LIGHT.Instance.DatabaseCar.AddLockedStatus(veh.instanceID.ToString(), true);
                                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_Locked", veh.instanceID.ToString()));
                                                break;
                                            }
                                            
                                        }
                                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_commmand_notowner"));
                                        return;
                                    }
                                    
                                }
                            }
                            else
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_error_1"));
                                return;
                            }
                            break;
                        case "unlock":
                            if (veh != null)
                            {
                                if (command.Length == 1)
                                {
                                    if (LIGHT.Instance.DatabaseCar.CheckOwner(veh.instanceID.ToString()) == player.Id)
                                    {
                                        LIGHT.Instance.DatabaseCar.AddLockedStatus(veh.instanceID.ToString(), false);
                                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_Unlocked", veh.instanceID.ToString()));
                                    }
                                    else
                                    {
                                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_commmand_notowner"));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_error_1"));
                                return;
                            }
                            break;
                        case "key":
                            if(command.Length == 1)
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_commmand_keyhelp"));
                                return;
                            }
                            if(command.Length == 2)
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_commmand_keyhelp2"));
                                return;
                            }
                            if(command.Length > 2)
                            {
                                if (LIGHT.Instance.DatabaseCar.CheckCarExistInDB(command[1]))
                                {
                                    if (LIGHT.Instance.DatabaseCar.CheckOwner(command[1]) == player.Id)
                                    {
                                        UnturnedPlayer PlayerKey = UnturnedPlayer.FromName(command[2]);
                                        if (PlayerKey != null)
                                        {
                                            LIGHT.Instance.DatabaseCar.AddGivenKeys(command[1], PlayerKey.Id);
                                            UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_key_given",command[1], PlayerKey.CharacterName));
                                        }
                                        else
                                        {
                                            UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_playernotfound"));
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_commmand_notowner"));
                                        return;
                                    }
                                }
                                else
                                {
                                    UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_not_found"));
                                    return;
                                }
                            }
                            break;
                        case "repair":
                            if (player.HasPermission("car.repair") || player.HasPermission("car.*") || player.HasPermission("*"))
                                hasPerm = true;
                            if (!hasPerm && !(caller.IsAdmin))
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_no_perm"));
                                return;
                            }
                            if (command.Length == 1)
                            {
                                if (veh != null)
                                {
                                    VehicleAsset vehi = veh.asset;
                                    int repair = 0;
                                    double percent = 0.00;
                                    repair = vehi.health - veh.health;
                                    if(repair > 0)
                                    {
                                        
                                        percent = Math.Round(((double)repair / (double)vehi.health) * 100.00, 2);
                                        if(LIGHT.Instance.Configuration.Instance.RepairPrice == 0)
                                            UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_repaired", veh.instanceID.ToString(), percent));
                                        else
                                        {
                                            decimal price = decimal.Parse("2");
                                            decimal.TryParse(LIGHT.Instance.Configuration.Instance.RepairPrice.ToString(), out price);
                                            decimal balance = Uconomy.Instance.Database.GetBalance(player.Id);
                                            decimal totalprice = Math.Round(price * (decimal)repair / (decimal)6, 2);
                                            if (balance < totalprice)
                                            {
                                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_not_enough_currency", Uconomy.Instance.Configuration.Instance.MoneyName));
                                                return;
                                            }
                                            decimal bal = Uconomy.Instance.Database.IncreaseBalance(player.CSteamID.ToString(), (totalprice * -1));
                                            if (bal >= 0.0m) UnturnedChat.Say(player.CSteamID, LIGHT.Instance.Translate("new_balance_msg", new object[] { bal, Uconomy.Instance.Configuration.Instance.MoneyName }));
                                            UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_repaired_price", veh.instanceID.ToString(), percent, totalprice, Uconomy.Instance.Configuration.Instance.MoneyName));
                                        }
                                        veh.askRepair((ushort)repair);
                                        
                                    }
                                    else
                                    {
                                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_repair_notneeded", veh.instanceID.ToString()));
                                        return;
                                    }
                                }
                                else
                                {
                                    UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_error_1"));
                                    return;
                                }
                            }
                            break;
                        case "locate":
                            if (command.Length == 1)
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_locate_help"));
                                return;
                            }
                            if(command.Length == 2)
                            {
                                if (LIGHT.Instance.DatabaseCar.CheckCarExistInDB(command[1]))
                                {
                                    if (LIGHT.Instance.DatabaseCar.CheckOwner(command[1]) == player.Id)
                                    {
                                        ushort index;
                                        string Status = "";
                                        if (ushort.TryParse(command[1], out index))
                                        {
                                            InteractableVehicle vehicle = VehicleManager.getVehicle(index);
                                            Vector3 pos = vehicle.transform.position;
											if (vehicle.isEmpty)
                                                Status += "It is Empty. ";
                                            if (vehicle.isDrowned)
                                                Status += "It is Drowned. ";
                                            if (vehicle.isDriven)
                                                Status += "It is Being Drove. ";
                                            UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_located", index, pos, Status));
                                        }
                                        else
                                        {
                                            UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_invalid_input"));
                                            return;
                                        }

                                    }
                                    else
                                    {
                                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_commmand_notowner"));
                                        return;
                                    }
                                }
                                else
                                {
                                    UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_not_found"));
                                    return;
                                }
                            }
                            break;
                        case "rkey":
                            if (command.Length == 1)
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_rkey_help"));
                                return;
                            }
                            if (command.Length == 2)
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_rkey_help2"));
                                return;                                                             
                            }
                            if(command.Length > 2)
                            {
                                if (LIGHT.Instance.DatabaseCar.CheckCarExistInDB(command[1]))
                                {
                                    UnturnedPlayer target;
                                    if (LIGHT.Instance.DatabaseCar.CheckOwner(command[1]) == player.Id)
                                    {
                                        target = UnturnedPlayer.FromName(command[2]);
                                        if (target == null)
                                        {
                                            if(LIGHT.Instance.DatabaseCar.RemoveGiveKeysCar(command[1], command[2]))
                                            {
                                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_rkey_success", command[2], command[1]));
                                            }
                                            else
                                            {
                                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_rkey_keynotfound", command[1]));
                                                return;     
                                            }
                                        }
                                        else
                                        {
                                            if (LIGHT.Instance.DatabaseCar.RemoveGiveKeysCar(command[1], target.Id))
                                            {
                                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_rkey_success", target.CharacterName, command[1]));
                                            }
                                            else
                                            {
                                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_rkey_keynotfound", command[1]));
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_commmand_notowner"));
                                        return;
                                    }
                                }
                                else
                                {
                                    UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_not_found"));
                                    return;
                                }
                            }
                            break;
                        case "list":
                            string cars = LIGHT.Instance.DatabaseCar.GetAllOwnedCars(player.Id);
                            if(cars == "" || cars == " ")
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_zero_owned"));
                            }
                            else
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_list_all"));
                            }
                            break;
                        case "steal":
                            if (command.Length == 1)
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_steal_help"));
                                return;
                            }
                            else
                            {
                                if (LIGHT.Instance.DatabaseCar.CheckCarExistInDB(command[1]))
                                { /** La voiture existe **/
                                    decimal[] stealVehPrice = { 100, 100,/**/100, 250, 500,/**/1000, 1500, 2000,/**/2500, 3000, 4000, 10000,/**/50000 };
                                    decimal[] lLicencePrice = { 500, 750,/**/1000, 1500, 3500,/**/1250, 1750, 3750,/**/1500, 2000, 4000, 10000,/**/90000 };
                                    string strVehID = LIGHT.Instance.DatabaseCar.GetCarID(command[1]);
                                    int vehID = LIGHT.Instance.DatabaseCar.convertLicenceToInt(strVehID);
                                    if (vehID == -1)
                                    {
                                        break;
                                    }
                                    /** Prix vol vehicule **/
                                    decimal vehprice = stealVehPrice[vehID];
                                    if (player.HasPermission("Car_Steal_"+strVehID) || player.HasPermission("Car_Steal_*"))
                                    {
                                        vehprice = 100; /** Prix pour forcer le vehicule avec autorisation **/
                                    }
                                    /** Prix licence **/
                                    decimal licPrice = 0;
                                    if (!player.HasPermission("Licence_" + strVehID) && LIGHT.Instance.DatabaseCar.CheckLicence(player.Id, strVehID))
                                    {
                                        licPrice = lLicencePrice[vehID];
                                    }
                                    decimal balance = Uconomy.Instance.Database.GetBalance(player.Id);
                                    decimal price = vehprice + licPrice;
                                    if (balance < price)
                                    {
                                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_not_enough_currency", Uconomy.Instance.Configuration.Instance.MoneyName));
                                        return;
                                    }
                                    /** Paiement du vol du vehicule **/
                                    decimal bal = Uconomy.Instance.Database.IncreaseBalance(player.Id, (price * -1));
                                    if (bal >= 0.0m)
                                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("new_balance_msg", new object[] { bal, Uconomy.Instance.Configuration.Instance.MoneyName }));
                                    /** Unlocking du vehicule **/
                                    string oldowner = LIGHT.Instance.DatabaseCar.GetOwnerName(command[1]);
                                    LIGHT.Instance.DatabaseCar.RemoveOwnership(command[1]);
                                    LIGHT.Instance.DatabaseCar.AddOwnership(command[1], player.Id, player.DisplayName);
                                    Logger.Log(player.CharacterName + " a vole la voiture numero " + command[1] + " de categorie " + vehID + " appartenant a " + oldowner);
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
}
