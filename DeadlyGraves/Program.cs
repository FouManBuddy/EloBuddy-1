using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace DeadlyGraves
{
    internal class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Menu DeadlyGraves, ComboMenu, DrawMenu, LaneMenu;
        public static AIHeroClient Me = ObjectManager.Player;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoaded;
        }

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void OnLoaded(EventArgs args)
        {
            Bootstrap.Init(null);
            Q = new Spell.Skillshot(SpellSlot.Q, (int)720f, SkillShotType.Linear, (int)0.25f, (int)2000f, (int)(15f * (float)Math.PI / 180));
            W = new Spell.Skillshot(SpellSlot.W, (int)850f, SkillShotType.Circular, (int)0.25f, (int)1650f, (int)250f);
            E = new Spell.Skillshot(SpellSlot.E, (uint)_Player.GetAutoAttackRange(), SkillShotType.Circular);
            R = new Spell.Skillshot(SpellSlot.R, (int)1100f, SkillShotType.Linear, (int)0.25f, (int)2100f, (int)100f);

            DeadlyGraves = MainMenu.AddMenu("DeadlyGraves", "deadlygraves");
            DeadlyGraves.AddGroupLabel("DeadlyGraves");
            DeadlyGraves.AddSeparator();
            DeadlyGraves.AddLabel("Made by Clyde for EloBuddy users! :)");

            ComboMenu = DeadlyGraves.AddSubMenu("Combo", "SpaceBarToWin");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("useQCombo", new CheckBox("Use Q"));
            ComboMenu.Add("useWCombo", new CheckBox("Use W"));
            ComboMenu.Add("useECombo", new CheckBox("Use E"));
            ComboMenu.Add("useRCombo", new CheckBox("Use R"));

            DrawMenu = DeadlyGraves.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawq", new CheckBox("Draw Q"));
            DrawMenu.Add("drawr", new CheckBox("Draw R"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += OnDraw;
        }

        private static void OnDraw(EventArgs args)
        {
            if (!Me.IsDead)
            {
                if (DrawMenu["drawq"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
                {
                    Drawing.DrawCircle(Me.Position, Q.Range, Color.White);
                }
                if (DrawMenu["drawr"].Cast<CheckBox>().CurrentValue && R.IsLearned)
                {
                    Drawing.DrawCircle(Me.Position, R.Range, Color.White);
                }
            }
        }



        private static void Game_OnTick(EventArgs args)
        {
            Orbwalker.ForcedTarget = null;
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo();
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 250, 400, 550 }[Program.R.Level] + 1.5 * _Player.FlatPhysicalDamageMod));
        }

        public static float QDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 60, 90, 120, 150, 180 }[Program.R.Level] + 0.8 * _Player.FlatPhysicalDamageMod));
        }

        public static void Combo()
        {
            var useQ = ComboMenu["useQCombo"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["useWCombo"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["useECombo"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["useRCombo"].Cast<CheckBox>().CurrentValue;

            foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(1200) && !o.IsDead && !o.IsZombie))
            {
                if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.Medium)
                {
                    Q.Cast(target);
                }
                if (useW && W.IsReady() && W.GetPrediction(target).HitChance >= HitChance.Medium && target.IsValidTarget(W.Range))
                {
                    W.Cast(target);
                }
                if (useE && E.IsReady() && target.IsValidTarget(700))
                {
                    E.Cast(Game.CursorPos);
                }
                if (useR && R.IsReady() && R.GetPrediction(target).HitChance >= HitChance.High && target.Health <= RDamage(target) && target.IsValidTarget(R.Range))
                {
                    R.Cast(target);
                }

                if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.High && target.Health <= QDamage(target) && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }


            }
        }
    }
}