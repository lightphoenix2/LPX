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
using Logger = Rocket.Core.Logging.Logger;

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
                    if(LIGHT.Instance.DatabaseCar.CheckOwner(command[0]) != "")
                    {
                        UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_car_alreadyown"));
                        return;
                    }
                    else
                    {
                        if (LIGHT.Instance.DatabaseCar.CheckCarExistInDB(command[0]))
                        {
                            // Declarer les prix dans Light.cs L502 et L444
                            decimal[] lLicencePrice = { 500, 750,/**/1000, 1500, 3500,/**/1250, 1750, 3750,/**/1500, 2000, 4000, 10000,/**/90000 }; /** Transport Ouvrier, Quad, Civil Terrestre, Civil Aerien, Civil Maritime, Police Terrestre, Police Aerien, Police Maritime, Mili Terrestre, Mili Aerien, Mili Maritime, Tanks, VIP (rainbow) **/
                            decimal[] lVehiclePrice = { 250, 300,/**/500, 500, 1500,/**/500, 500, 1500,/**/500, 500, 1500, 2000,/**/10000 };
                            /* -- CALCUL DU PRIX A PAYER -- */
                            decimal licenceprice = 0;
                            decimal vehicleprice = 0;
                            string vehicleID = LIGHT.Instance.DatabaseCar.GetCarID(command[0]); /* On recupe l'ID de la voiture demandee */
							Logger.Log("CarID: "+vehicleID);
                            string strLicence = LIGHT.Instance.DatabaseCar.GetLicence(vehicleID);
                            Logger.Log("Licence Requise: " +strLicence);
                            int noLicence = LIGHT.Instance.DatabaseCar.convertLicenceToInt(strLicence);  /* On regarde a quelle licence correspond le vehicule */
                            Logger.Log("Numero de la licence: " +noLicence.ToString());
                            if (noLicence == -1)
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_car_Forbidden"));
								return;
                            }
                            Logger.Log("Le Vehicule est autorise");
                            bool HasLicence = LIGHT.Instance.DatabaseCar.CheckLicence(player.Id, strLicence);
							if (! HasLicence && ! player.HasPermission("Licence_"+strLicence)) { //Sans Permission
                                Logger.Log("L'Utilisateur n'a pas la licence");
                                licenceprice = lLicencePrice.ElementAt(noLicence); /* Ajout du prix de la licence */
                            }
                            vehicleprice = lVehiclePrice.ElementAt(noLicence); /* Ajout du prix du vehicule */
                            decimal totalprice = vehicleprice + licenceprice; /* Calcul du prix total */
                            Logger.Log(player.SteamName+" doit regler "+totalprice.ToString());
                            /* -- COMPARAISON DU TARIF ET DE L'ECONOMIE -- */
                            decimal balance = Uconomy.Instance.Database.GetBalance(player.Id);
                            if (balance < totalprice)
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("car_not_enough_currency", Uconomy.Instance.Configuration.Instance.MoneyName));
                                return;
                            }
                            Logger.Log("L'utilisateur possede assez d'argent");
                            /* -- PAIEMENT -- */
                            decimal bal = Uconomy.Instance.Database.IncreaseBalance(player.Id, (totalprice * -1));
                            if (bal >= 0.0m)
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("new_balance_msg", new object[] { bal, Uconomy.Instance.Configuration.Instance.MoneyName }));
                            /* -- AJOUT DE LICENCE -- */
                            if (! HasLicence)
                            {
                                LIGHT.Instance.DatabaseCar.AddLicenceToPlayer(player.Id, strLicence);
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_car_licenceOK")+noLicence.ToString());
                            }
                            /* -- AJOUT DU VEHICULE -- */
                            if (!LIGHT.Instance.DatabaseCar.AddOwnership(command[0], player.Id, player.SteamName))
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_car_purchaseFailed"));
                                bal = Uconomy.Instance.Database.IncreaseBalance(player.Id, vehicleprice);
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("new_balance_msg", new object[] { bal, Uconomy.Instance.Configuration.Instance.MoneyName }));
                                return;
                            }
                            else
                            {
                                UnturnedChat.Say(caller, LIGHT.Instance.Translate("lpx_car_purchased", command[0], vehicleprice, Uconomy.Instance.Configuration.Instance.MoneyName));
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
