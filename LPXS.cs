﻿using System;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using Rocket.Core.Logging;
using fr34kyn01535.Uconomy;
using System.Data;
using System.Collections.Generic;
using UnityEngine;
using SDG.Unturned;
using Logger = Rocket.Core.Logging.Logger;

namespace LIGHT
{
    public class LPXS : UnturnedPlayerComponent
    {
        private DateTime lastpaid;
        public delegate void PlayerPaidEvent(UnturnedPlayer player, decimal amount);
        public event PlayerPaidEvent OnPlayerPaid;
        public Dictionary<string, decimal> PayGroups = new Dictionary<string, decimal>();
        protected void Start()
        {
            this.lastpaid = DateTime.Now;
        }
        private void FixedUpdate()
        {
            if (LIGHT.Instance.Configuration.Instance.IncomeEnabled && (DateTime.Now - this.lastpaid).TotalSeconds >= LIGHT.Instance.Configuration.Instance.IncomeInterval)
            {
                PayGroups.Clear();
                this.lastpaid = DateTime.Now;
                decimal pay = 0.0m;
                string paygroup = "Player";
                DataTable dt = new DataTable();
                dt = LIGHT.Instance.Database.GetGroup();
                string[] stringArr = new string[dt.Rows.Count];
                List<string> plgroups = new List<string>();
                for (int i = 0; i < (dt.Rows.Count); i++)
                {
                    stringArr[i] += dt.Rows[i].ItemArray[0].ToString();
                    PayGroups.Add(stringArr[i], (LIGHT.Instance.Database.GetAllGroupIncome())[i]);
                    plgroups.Add(stringArr[i]);
                }
                decimal pay2 = 0.0m;
                foreach (string s in plgroups)
                {
                    PayGroups.TryGetValue(s, out pay2);
                    if (LIGHT.Instance.Database.CheckUserGroup(this.Player.Id).ToLower() == s.ToLower())
                    {
                        pay = pay2;
                        paygroup = s;
                        if (s == "defualt") paygroup = "Player";
                    }
                }
                if (pay == 0.0m)
                {
                    // We assume they are default group.
                    PayGroups.TryGetValue("default", out pay);
                    if (pay == 0.0m)
                    {
                        // There was an error.  End it.
                        Logger.Log(LIGHT.Instance.Translate("unable_to_pay_group_msg", new object[] { this.Player.CharacterName, "" }));
                        return;
                    }
                }
                if (paygroup == "default") paygroup = "Player";
                decimal bal = Uconomy.Instance.Database.IncreaseBalance(this.Player.CSteamID.ToString(), pay);
                if (OnPlayerPaid != null)
                    OnPlayerPaid(this.Player, pay);
                this.Player.Player.gameObject.SendMessage("UEOnPlayerPaid", new object[] { this.Player.Player, pay });
                UnturnedChat.Say(this.Player.CSteamID, LIGHT.Instance.Translate("pay_time_msg", new object[] { pay, Uconomy.Instance.Configuration.Instance.MoneyName, paygroup }));
                if (bal >= 0.0m) UnturnedChat.Say(this.Player.CSteamID, LIGHT.Instance.Translate("new_balance_msg", new object[] { bal, Uconomy.Instance.Configuration.Instance.MoneyName }));
                PayGroups.Clear();
                string[] permission = { };
                string[] cmd = { };
                permission = LIGHT.Instance.Database.getPermission(this.Player.Id);
                for (int i = permission.Length - 1; i >= 0; i--)
                {
                    if (permission[i].Contains("color.") && !(this.Player.IsAdmin))
                    {
                        cmd = permission[i].Split('.');
                        Color? color = UnturnedChat.GetColorFromName(cmd[1], Color.white);
                        this.Player.Color = color.Value;
                    }
                }
                
            }
        }
    }
}
