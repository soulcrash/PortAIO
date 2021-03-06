#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace Marksman.Champions
{
    internal class Urgot : Champion
    {
        private const string vSpace = "     ";
        public static Spell Q, QEx, W, E, R;

        public Urgot()
        {
            Utils.Utils.PrintMessage("Urgot loaded.");

            Q = new Spell(SpellSlot.Q, 1000);
            QEx = new Spell(SpellSlot.Q, 1200);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 900);
            R = new Spell(SpellSlot.R, 700);

            Q.SetSkillshot(0.10f, 100f, 1600f, true, SkillshotType.SkillshotLine);
            QEx.SetSkillshot(0.10f, 60f, 1600f, false, SkillshotType.SkillshotLine);

            E.SetSkillshot(0.25f, 120f, 1500f, false, SkillshotType.SkillshotCircle);

            R.SetTargetted(1f, 100f);
        }

        public static int UnderTurretEnemyMinion
        {
            get
            {
                return ObjectManager.Get<Obj_AI_Minion>().Count(xMinion => xMinion.IsEnemy && UnderAllyTurret(xMinion));
            }
        }

        private static Obj_AI_Hero getInfectedEnemy
        {
            get
            {
                return
                    (from enemy in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                enemy =>
                                    enemy.IsEnemy && ObjectManager.Player.LSDistance(enemy) <= QEx.Range &&
                                    enemy.HasBuff("urgotcorrosivedebuff"))
                        select enemy).FirstOrDefault();
            }
        }

        public void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (W.IsReady() && gapcloser.Sender.LSIsValidTarget(250f))
                W.Cast();
        }

        public static bool UnderAllyTurret(Obj_AI_Base unit)
        {
            return ObjectManager.Get<Obj_AI_Turret>().Where<Obj_AI_Turret>(turret =>
            {
                if (turret == null || !turret.IsValid || turret.Health <= 0f)
                {
                    return false;
                }
                if (!turret.IsEnemy)
                {
                    return true;
                }
                return false;
            })
                .Any<Obj_AI_Turret>(
                    turret =>
                        Vector2.Distance(unit.Position.LSTo2D(), turret.Position.LSTo2D()) < 900f && turret.IsAlly);
        }

        public static bool TeleportTurret(Obj_AI_Hero vTarget)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Any(player => !player.IsDead && player.IsMe && UnderAllyTurret(ObjectManager.Player));
        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            Spell[] spellList = {Q, E, R};
            foreach (var spell in spellList)
            {
                var menuItem = GetValue<Circle>("Draw" + spell.Slot);
                if (menuItem.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }

            var drawQEx = GetValue<Circle>("DrawQEx");
            if (drawQEx.Active)
            {
                if (getInfectedEnemy != null)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, QEx.Range, Color.GreenYellow);
                    Render.Circle.DrawCircle(getInfectedEnemy.Position, 125f, Color.GreenYellow);
                }
            }
        }

        private static void UseSpells(bool useQ, bool useW, bool useE)
        {
            Obj_AI_Hero t;

            if (W.IsReady() && useW)
            {
                t = TargetSelector.GetTarget(ObjectManager.Player.AttackRange - 30, TargetSelector.DamageType.Physical);
                if (t != null)
                    W.Cast();
            }
        }

        private static void UltUnderTurret()
        {
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            Drawing.DrawText(Drawing.Width*0.41f, Drawing.Height*0.80f, Color.GreenYellow,
                "Teleport enemy to under ally turret active!");

            if (R.IsReady() && Program.ChampionClass.GetValue<bool>("UseRC"))
            {
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                if (t != null && UnderAllyTurret(ObjectManager.Player) && !UnderAllyTurret(t) &&
                    ObjectManager.Player.LSDistance(t) > 200)
                {
                    R.CastOnUnit(t);
                }
            }

            UseSpells(Program.ChampionClass.GetValue<bool>("UseQC"), Program.ChampionClass.GetValue<bool>("UseWC"),
                Program.ChampionClass.GetValue<bool>("UseEC"));
        }

        private static void UltInMyTeam()
        {
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            Drawing.DrawText(Drawing.Width*0.42f, Drawing.Height*0.80f, Color.GreenYellow,
                "Teleport enemy to my team active!");

            var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (R.IsReady() && t != null)
            {
                var Ally =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            ally =>
                                ally.IsAlly && !ally.IsDead && ObjectManager.Player.LSDistance(ally) <= R.Range &&
                                t.LSDistance(ally) > t.LSDistance(ObjectManager.Player));

                if (Ally.Count() >= Program.ChampionClass.GetValue<Slider>("UltOp2Count").Value)
                    R.CastOnUnit(t);
            }

            UseSpells(Program.ChampionClass.GetValue<bool>("UseQC"), Program.ChampionClass.GetValue<bool>("UseWC"),
                Program.ChampionClass.GetValue<bool>("UseEC"));
        }

        private static void CastQ(Obj_AI_Hero t)
        {
            
            var Qpredict = Q.GetPrediction(t);
            var hithere = Qpredict.CastPosition.LSExtend(ObjectManager.Player.Position, -20);

            if (Qpredict.Hitchance >= HitChance.High)
            {
                if (W.IsReady())
                    W.Cast();
                Q.Cast(hithere);
            }
        }

        public override void Game_OnGameUpdate(EventArgs args)
        {
            if (R.Level > 0)
                R.Range = 150*R.Level + 400;

            if (GetValue<KeyBind>("UltOp1").Active)
            {
                UltUnderTurret();
            }

            if (GetValue<KeyBind>("UltOp2").Active)
            {
                UltInMyTeam();
            }

            if (!ComboActive)
            {
                var t = TargetSelector.GetTarget(QEx.Range, TargetSelector.DamageType.Physical);
                if (!t.LSIsValidTarget())
                    return;

                if (HarassActive && GetValue<bool>("UseQH"))
                    CastQ(t);

                if (GetValue<KeyBind>("UseQTH").Active)
                    CastQ(t);
            }

            if (ComboActive)
            {
                var t = TargetSelector.GetTarget(QEx.Range, TargetSelector.DamageType.Physical);

                if (E.IsReady() && GetValue<bool>("UseEC"))
                {
                    if (t.LSIsValidTarget(E.Range))
                    {
                        E.CastIfHitchanceEquals(t, HitChance.Medium);
                    }
                }

                if (Q.IsReady() && GetValue<bool>("UseQC"))
                {
                    if (getInfectedEnemy != null)
                    {
                        if (W.IsReady())
                            W.Cast();
                        QEx.Cast(getInfectedEnemy);
                    }
                    else
                    {
                        if (t.LSIsValidTarget(Q.Range))
                            CastQ(t);
                    }
                }
            }

            if (LaneClearActive)
            {
                var useQ = GetValue<bool>("UseQL");

                if (Q.IsReady() && useQ)
                {
                    var vMinions = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range);
                    foreach (
                        var minions in
                            vMinions.Where(
                                minions => minions.Health < ObjectManager.Player.GetSpellDamage(minions, SpellSlot.Q)))
                        Q.Cast(minions);
                }
            }
        }

        public override bool ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQC" + Id, "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWC" + Id, "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEC" + Id, "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseRC" + Id, "Use R").SetValue(true));


            config.AddItem(new MenuItem("UltOpt1", "Ult Option 1"));
            config.AddItem(
                new MenuItem("UltOp1" + Id, vSpace + "Teleport Ally Turrent").SetValue(new KeyBind(
                    "T".ToCharArray()[0], KeyBindType.Press)));

            config.AddItem(new MenuItem("UltOpt2", "Ult Option 2"));
            config.AddItem(
                new MenuItem("UltOp2" + Id, vSpace + "Teleport My Team").SetValue(new KeyBind("G".ToCharArray()[0],
                    KeyBindType.Press)));
            config.AddItem(new MenuItem("UltOp2Count" + Id, vSpace + "Min. Ally Count").SetValue(new Slider(1, 1, 5)));


            config.AddSubMenu(new Menu("Don't Use Ult on", "DontUlt"));
            foreach (
                var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
            {
                config.SubMenu("DontUlt")
                    .AddItem(
                        new MenuItem(string.Format("DontUlt{0}", enemy.CharData.BaseSkinName), enemy.CharData.BaseSkinName).SetValue(false));
            }

            return true;
        }

        public override bool HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQH" + Id, "Use Q").SetValue(true));
            config.AddItem(
                new MenuItem("UseQTH" + Id, "Use Q (Toggle)").SetValue(new KeyBind("H".ToCharArray()[0],
                    KeyBindType.Toggle)));

            return true;
        }

        public override bool DrawingMenu(Menu config)
        {
            config.AddItem(new MenuItem("DrawQ" + Id, "Q range").SetValue(new Circle(true, Color.LightGray)));
            config.AddItem(new MenuItem("DrawE" + Id, "E range").SetValue(new Circle(false, Color.LightGray)));
            config.AddItem(new MenuItem("DrawR" + Id, "R range").SetValue(new Circle(false, Color.LightGray)));
            config.AddItem(new MenuItem("DrawQEx" + Id, "Corrosive Charge").SetValue(new Circle(true, Color.LightGray)));

            return true;
        }

        public override bool MiscMenu(Menu config)
        {
            return true;
        }

        public override bool LaneClearMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQL" + Id, "Use Q").SetValue(true));
            return true;
        }
        public override bool JungleClearMenu(Menu config)
        {
            return false;
        }
    }
}