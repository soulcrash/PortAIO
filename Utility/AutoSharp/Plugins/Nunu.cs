﻿#region LICENSE

// Copyright 2014-2015 Support
// Nunu.cs is part of Support.
// 
// Support is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Support is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Support. If not, see <http://www.gnu.org/licenses/>.
// 
// Filename: Support/Support/Nunu.cs
// Created:  16/11/2014
// Date:     20/01/2015/11:20
// Author:   h3h3

#endregion

using System; using EloBuddy; using EloBuddy.SDK.Menu; using EloBuddy.SDK; using EloBuddy.SDK.Menu.Values;
using System.Linq;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Plugins
{
    public class Nunu : PluginBase
    {
        public Nunu()
        {
            Q = new LeagueSharp.Common.Spell(SpellSlot.Q, 125);
            W = new LeagueSharp.Common.Spell(SpellSlot.W, 700);
            E = new LeagueSharp.Common.Spell(SpellSlot.E, 550);
            R = new LeagueSharp.Common.Spell(SpellSlot.R, 650);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (true)
            {
                if (Q.IsReady() && ComboConfig["Combo.Q"].Cast<CheckBox>().CurrentValue &&
                    Player.HealthPercent < ComboConfig["Combo.Q.Health"].Cast<Slider>().CurrentValue)
                {
                    var minion = MinionManager.GetMinions(Player.Position, Q.Range).FirstOrDefault();
                    if (minion.LSIsValidTarget(Q.Range))
                    {
                        Q.Cast(minion);
                    }
                }
				var tarnunu = TargetSelector.GetTarget(900, DamageType.Magical);

                var allys = Helpers.AllyInRange(W.Range).OrderByDescending(h => h.FlatPhysicalDamageMod).ToList();
                if (W.IsReady() && allys.Count > 0 && ComboConfig["Combo.W"].Cast<CheckBox>().CurrentValue)
                {
                    W.Cast(allys.FirstOrDefault());
                }

                if (W.IsReady() && tarnunu.LSIsValidTarget(AttackRange) && ComboConfig["Combo.W"].Cast<CheckBox>().CurrentValue)
                {
                    W.Cast(Player);
                }

                if (E.IsReady() && Target.LSIsValidTarget(E.Range) && ComboConfig["Combo.E"].Cast<CheckBox>().CurrentValue)
                {
                    E.Cast(tarnunu);
                }
                if (R.IsReady() && Player.LSCountEnemiesInRange(R.Range) > 2)
                {
                    R.Cast();
                }
            }

            if (true)
            {
                if (Q.IsReady() && HarassConfig["Harass.Q"].Cast<CheckBox>().CurrentValue &&
                    Player.HealthPercent < HarassConfig["Harass.Q.Health"].Cast <Slider>().CurrentValue)
                {
                    var minion = MinionManager.GetMinions(Player.Position, Q.Range).FirstOrDefault();
                    if (minion.LSIsValidTarget(Q.Range))
                    {
                        Q.Cast(minion);
                    }
                }
				var tarnunu = TargetSelector.GetTarget(900, DamageType.Magical);

                var allys = Helpers.AllyInRange(W.Range).OrderByDescending(h => h.FlatPhysicalDamageMod).ToList();
                if (W.IsReady() && allys.Count > 0 && HarassConfig["Harass.W"].Cast<CheckBox>().CurrentValue)
                {
                    W.Cast(allys.FirstOrDefault());
                }

                if (W.IsReady() && tarnunu.LSIsValidTarget(AttackRange) && HarassConfig["Harass.W"].Cast<CheckBox>().CurrentValue)
                {
                    W.Cast(Player);
                }

                if (E.IsReady() && tarnunu.LSIsValidTarget(E.Range) && HarassConfig["Harass.E"].Cast<CheckBox>().CurrentValue)
                {
                    E.Cast(tarnunu);
                }
            }
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (E.CastCheck(gapcloser.Sender, "Gapcloser.E"))
            {
                E.Cast(gapcloser.Sender);

                if (W.IsReady())
                {
                    W.Cast(Player);
                }
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("Combo.Q", "Use Q", true);
            config.AddBool("Combo.W", "Use W", true);
            config.AddBool("Combo.E", "Use E", true);
            config.AddSlider("Combo.Q.Health", "Consume below %HP", 50, 1, 100);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("Harass.Q", "Use Q", true);
            config.AddBool("Harass.W", "Use W", false);
            config.AddBool("Harass.E", "Use E", true);
            config.AddSlider("Harass.Q.Health", "Consume below %HP", 50, 1, 100);
        }

        public override void MiscMenu(Menu config)
        {
            config.AddList("Misc.Laugh", "Laugh Emote", new[] { "OFF", "ON", "ON + Mute" });
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("Gapcloser.E", "Use E to Interrupt Gapcloser", true);
        }
    }
}