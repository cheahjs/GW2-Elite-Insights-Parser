﻿using System.Collections.Generic;
using GW2EIEvtcParser.Extensions;
using GW2EIEvtcParser.ParsedData;
using System.Linq;
using static GW2EIEvtcParser.ArcDPSEnums;
using static GW2EIEvtcParser.EIData.Buff;
using static GW2EIEvtcParser.EIData.DamageModifier;
using static GW2EIEvtcParser.ParserHelper;
using static GW2EIEvtcParser.SkillIDs;

namespace GW2EIEvtcParser.EIData
{
    internal static class RevenantHelper
    {
        internal static readonly List<InstantCastFinder> InstantCastFinder = new List<InstantCastFinder>()
        {
            new BuffGainCastFinder(LegendaryAssassinStanceSkill, LegendaryAssassinStanceEffect), // Legendary Assassin Stance
            new BuffGainCastFinder(LegendaryDemonStanceSkill, LegendaryDemonStanceEffect), // Legendary Demon Stance
            new BuffGainCastFinder(LegendaryDwarfStanceSkill, LegendaryDwarfStanceEffect), // Legendary Dwarf Stance
            new BuffGainCastFinder(LegendaryCentaurStanceSkill, LegendaryCentaurStanceEffect), // Legendary Centaur Stance
            new BuffGainCastFinder(ImpossibleOddsSkill, ImpossibleOddsEffect).UsingICD(500), // Impossible Odds
            new BuffLossCastFinder(RelinquishPower, ImpossibleOddsEffect).UsingICD(500), // Relinquish Power
            new BuffGainCastFinder(VengefulHammersSkill, VengefulHammersEffect), // Vengeful Hammers
            new BuffLossCastFinder(ReleaseHammers, VengefulHammersEffect), // Release Hammers
            new BuffLossCastFinder(ResistTheDarkness, EmbraceTheDarkness), // Release Hammers
            new DamageCastFinder(InvokingTorment, InvokingTorment).WithBuilds(GW2Builds.February2020Balance), // Invoking Torment
            new DamageCastFinder(CallOfTheAssassin, CallOfTheAssassin), // Call of the Assassin
            new DamageCastFinder(CallOfTheDwarf, CallOfTheDwarf), // Call of the Dwarf
            new DamageCastFinder(CallOfTheDemon, CallOfTheDemon), // Call of the Demon
            new EXTHealingCastFinder(CallOfTheCentaur, CallOfTheCentaur), // Call of the Centaur
            new EffectCastFinder(ProjectTranquility, EffectGUIDs.RevenantTabletAutoHeal).UsingChecker((evt, combatData, agentData, skillData) => evt.Src.ID == (int)MinionID.VentariTablet),
            new EffectCastFinderByDstFromMinion(VentarisWill, EffectGUIDs.RevenantTabletVentarisWill).UsingChecker((evt, combatData, agentData, skillData) => evt.Dst.ID == (int)MinionID.VentariTablet),
            new EffectCastFinderByDstFromMinion(NaturalHarmony, EffectGUIDs.RevenantNaturalHarmony).UsingChecker((evt, combatData, agentData, skillData) => evt.Dst.ID == (int)MinionID.VentariTablet),
            new EffectCastFinderFromMinion(PurifyingEssence, EffectGUIDs.RevenantPurifyingEssence).UsingChecker((evt, combatData, agentData, skillData) => evt.Src.ID == (int)MinionID.VentariTablet),
            new EffectCastFinderFromMinion(EnergyExpulsion, EffectGUIDs.RevenantEnergyExpulsion).UsingChecker((evt, combatData, agentData, skillData) => evt.Src.ID == (int)MinionID.VentariTablet),
            new EffectCastFinder(ProtectiveSolace, EffectGUIDs.RevenantProtectiveSolace).UsingChecker((evt, combatData, agentData, skillData) => evt.Src.BaseSpec == Spec.Revenant && evt.IsAroundDst && evt.Dst.ID == (int)MinionID.VentariTablet),
        };


        internal static readonly List<DamageModifier> DamageMods = new List<DamageModifier>
        {
            // Retribution
            new BuffDamageModifierTarget(Weakness, "Dwarven Battle Training", "10% on weakened target", DamageSource.NoPets, 10.0, DamageType.Strike, DamageType.All, Source.Revenant, ByPresence, "https://wiki.guildwars2.com/images/5/50/Dwarven_Battle_Training.png", DamageModifierMode.All).WithBuilds(GW2Builds.December2018Balance),
            new BuffDamageModifier(Retaliation, "Vicious Reprisal", "10% under retaliation", DamageSource.NoPets, 10.0, DamageType.Strike, DamageType.All, Source.Revenant, ByPresence, "https://wiki.guildwars2.com/images/c/cf/Vicious_Reprisal.png", DamageModifierMode.All).WithBuilds(GW2Builds.StartOfLife, GW2Builds.May2021Balance),
            new BuffDamageModifier(Resolution, "Vicious Reprisal", "10% under resolution", DamageSource.NoPets, 10.0, DamageType.StrikeAndCondition, DamageType.All, Source.Revenant, ByPresence, "https://wiki.guildwars2.com/images/c/cf/Vicious_Reprisal.png", DamageModifierMode.All).WithBuilds(GW2Builds.May2021Balance),
            // Invocation
            new BuffDamageModifier(Fury, "Ferocious Aggression", "7% under fury", DamageSource.NoPets, 7.0, DamageType.StrikeAndCondition, DamageType.All, Source.Revenant, ByPresence, "https://wiki.guildwars2.com/images/e/ec/Ferocious_Aggression.png", DamageModifierMode.All).WithBuilds(GW2Builds.StartOfLife, GW2Builds.May2021Balance),
            new BuffDamageModifier(Fury, "Ferocious Aggression", "7% under fury", DamageSource.NoPets, 7.0, DamageType.StrikeAndConditionAndLifeLeech, DamageType.All, Source.Revenant, ByPresence, "https://wiki.guildwars2.com/images/e/ec/Ferocious_Aggression.png", DamageModifierMode.All).WithBuilds(GW2Builds.May2021Balance, GW2Builds.August2022Balance),
            new BuffDamageModifier(Fury, "Ferocious Aggression", "7% under fury", DamageSource.NoPets, 7.0, DamageType.StrikeAndConditionAndLifeLeech, DamageType.All, Source.Revenant, ByPresence, "https://wiki.guildwars2.com/images/e/ec/Ferocious_Aggression.png", DamageModifierMode.sPvPWvW).WithBuilds(GW2Builds.August2022Balance),
            new BuffDamageModifier(Fury, "Ferocious Aggression", "10% under fury", DamageSource.NoPets, 10.0, DamageType.StrikeAndConditionAndLifeLeech, DamageType.All, Source.Revenant, ByPresence, "https://wiki.guildwars2.com/images/e/ec/Ferocious_Aggression.png", DamageModifierMode.PvE).WithBuilds(GW2Builds.August2022Balance),
            new DamageLogDamageModifier("Rising Tide", "7% if hp >=90%", DamageSource.NoPets, 7.0, DamageType.Strike, DamageType.All, Source.Revenant,"https://wiki.guildwars2.com/images/0/0c/Rising_Tide.png", (x, log) => x.IsOverNinety, ByPresence, DamageModifierMode.All).WithBuilds(GW2Builds.StartOfLife, GW2Builds.August2022Balance),
            new DamageLogDamageModifier("Rising Tide", "7% if hp >=90%", DamageSource.NoPets, 7.0, DamageType.Strike, DamageType.All, Source.Revenant,"https://wiki.guildwars2.com/images/0/0c/Rising_Tide.png", (x, log) => x.IsOverNinety, ByPresence, DamageModifierMode.sPvPWvW).WithBuilds(GW2Builds.August2022Balance),
            new DamageLogDamageModifier("Rising Tide", "10% if hp >=90%", DamageSource.NoPets, 10.0, DamageType.Strike, DamageType.All, Source.Revenant,"https://wiki.guildwars2.com/images/0/0c/Rising_Tide.png", (x, log) => x.IsOverNinety, ByPresence, DamageModifierMode.PvE).WithBuilds(GW2Builds.August2022Balance, GW2Builds.November2022Balance),
            new DamageLogDamageModifier("Rising Tide", "10% if hp >=75%", DamageSource.NoPets, 10.0, DamageType.Strike, DamageType.All, Source.Revenant,"https://wiki.guildwars2.com/images/0/0c/Rising_Tide.png", (x, log) => x.From.GetCurrentHealthPercent(log, x.Time) >= 75.0, ByPresence, DamageModifierMode.PvE).WithBuilds(GW2Builds.November2022Balance).UsingApproximate(true),
            // Devastation
            new BuffDamageModifier(ViciousLacerations, "Vicious Lacerations", "3% per Stack", DamageSource.NoPets, 3.0, DamageType.Strike, DamageType.All, Source.Revenant, ByStack, "https://wiki.guildwars2.com/images/c/cd/Vicious_Lacerations.png", DamageModifierMode.PvE).WithBuilds(GW2Builds.October2018Balance, GW2Builds.February2020Balance),
            new BuffDamageModifier(ViciousLacerations, "Vicious Lacerations", "2% per Stack", DamageSource.NoPets, 2.0, DamageType.Strike, DamageType.All, Source.Revenant, ByStack, "https://wiki.guildwars2.com/images/c/cd/Vicious_Lacerations.png", DamageModifierMode.PvE).WithBuilds(GW2Builds.StartOfLife, GW2Builds.October2018Balance),
            new DamageLogDamageModifier("Unsuspecting Strikes", "25% if target hp > 80%", DamageSource.NoPets, 25.0, DamageType.Strike, DamageType.All, Source.Revenant, "https://wiki.guildwars2.com/images/c/cd/Vicious_Lacerations.png", (x,log) =>
            {
                double foeHP = x.To.GetCurrentHealthPercent(log, x.Time);
                if (foeHP < 0.0)
                {
                    return false;
                }
                return foeHP > 80.0;
            }, ByPresence, DamageModifierMode.PvE ).UsingApproximate(true).WithBuilds(GW2Builds.February2020Balance, GW2Builds.May2021BalanceHotFix),
            new DamageLogDamageModifier("Unsuspecting Strikes", "20% if target hp > 80%", DamageSource.NoPets, 20.0, DamageType.Strike, DamageType.All, Source.Revenant, "https://wiki.guildwars2.com/images/c/cd/Vicious_Lacerations.png", (x,log) =>
            {
                double foeHP = x.To.GetCurrentHealthPercent(log, x.Time);
                if (foeHP < 0.0)
                {
                    return false;
                }
                return foeHP > 80.0;
            }, ByPresence, DamageModifierMode.PvE ).UsingApproximate(true).WithBuilds(GW2Builds.May2021BalanceHotFix),
            new DamageLogDamageModifier("Unsuspecting Strikes", "10% if target hp > 80%", DamageSource.NoPets, 10.0, DamageType.Strike, DamageType.All, Source.Revenant, "https://wiki.guildwars2.com/images/c/cd/Vicious_Lacerations.png", (x,log) =>
            {
                double foeHP = x.To.GetCurrentHealthPercent(log, x.Time);
                if (foeHP < 0.0)
                {
                    return false;
                }
                return foeHP > 80.0;
            }, ByPresence, DamageModifierMode.sPvPWvW ).UsingApproximate(true).WithBuilds(GW2Builds.February2020Balance),
            new BuffDamageModifierTarget(Vulnerability, "Targeted Destruction", "0.5% per stack vuln", DamageSource.NoPets, 0.5, DamageType.Strike, DamageType.All, Source.Revenant, ByStack, "https://wiki.guildwars2.com/images/e/ed/Targeted_Destruction.png", DamageModifierMode.All).WithBuilds(GW2Builds.March2019Balance),
            new BuffDamageModifierTarget(Vulnerability, "Targeted Destruction", "10.0% if vuln", DamageSource.NoPets, 10.0, DamageType.Strike, DamageType.All, Source.Revenant, ByPresence, "https://wiki.guildwars2.com/images/e/ed/Targeted_Destruction.png", DamageModifierMode.PvE).WithBuilds(GW2Builds.October2018Balance, GW2Builds.March2019Balance),
            new BuffDamageModifierTarget(Vulnerability, "Targeted Destruction", "7.0% if vuln", DamageSource.NoPets, 7.0, DamageType.Strike, DamageType.All, Source.Revenant, ByPresence, "https://wiki.guildwars2.com/images/e/ed/Targeted_Destruction.png", DamageModifierMode.PvE).WithBuilds(GW2Builds.StartOfLife, GW2Builds.October2018Balance),
            new DamageLogDamageModifier("Swift Termination", "20% if target <50%", DamageSource.NoPets, 20.0, DamageType.Strike, DamageType.All, Source.Revenant,"https://wiki.guildwars2.com/images/b/bb/Swift_Termination.png", (x, log) => x.AgainstUnderFifty, ByPresence, DamageModifierMode.All),
        };

        internal static readonly List<Buff> Buffs = new List<Buff>
        {
                new Buff("Vengeful Hammers", VengefulHammersEffect, Source.Revenant, BuffClassification.Other,"https://wiki.guildwars2.com/images/c/c8/Vengeful_Hammers.png"),
                new Buff("Rite of the Great Dwarf", RiteOfTheGreatDwarf, Source.Revenant, BuffClassification.Defensive, "https://wiki.guildwars2.com/images/6/69/Rite_of_the_Great_Dwarf.png"),
                new Buff("Rite of the Great Dwarf (Traited)", RiteOfTheGreatDwarfTraited, Source.Revenant, BuffClassification.Defensive, "https://wiki.guildwars2.com/images/6/69/Rite_of_the_Great_Dwarf.png"),
                new Buff("Embrace the Darkness", EmbraceTheDarkness, Source.Revenant, BuffClassification.Other, "https://wiki.guildwars2.com/images/5/51/Embrace_the_Darkness.png"),
                new Buff("Enchanted Daggers", EnchantedDaggers, Source.Revenant, BuffStackType.Stacking, 25, BuffClassification.Other, "https://wiki.guildwars2.com/images/f/fa/Enchanted_Daggers.png"),
                new Buff("Phase Traversal", PhaseTraversal, Source.Revenant, BuffStackType.Stacking, 25, BuffClassification.Other, "https://wiki.guildwars2.com/images/f/f2/Phase_Traversal.png"),
                new Buff("Tranquil", Tranquil, Source.Revenant, BuffStackType.Stacking, 25, BuffClassification.Other, "https://wiki.guildwars2.com/images/e/e7/Project_Tranquility.png"),
                new Buff("Impossible Odds", ImpossibleOddsEffect, Source.Revenant, BuffClassification.Other, "https://wiki.guildwars2.com/images/8/87/Impossible_Odds.png"),
                new Buff("Legendary Centaur Stance",LegendaryCentaurStanceEffect, Source.Revenant, BuffClassification.Other, "https://wiki.guildwars2.com/images/8/8a/Legendary_Centaur_Stance.png"),
                new Buff("Legendary Dwarf Stance",LegendaryDwarfStanceEffect, Source.Revenant, BuffClassification.Other, "https://wiki.guildwars2.com/images/b/b2/Legendary_Dwarf_Stance.png"),
                new Buff("Legendary Demon Stance",LegendaryDemonStanceEffect, Source.Revenant, BuffClassification.Other, "https://wiki.guildwars2.com/images/d/d1/Legendary_Demon_Stance.png"),
                new Buff("Legendary Assassin Stance",LegendaryAssassinStanceEffect, Source.Revenant, BuffClassification.Other, "https://wiki.guildwars2.com/images/0/02/Legendary_Assassin_Stance.png"),
                //traits
                new Buff("Vicious Lacerations",ViciousLacerations, Source.Revenant, BuffStackType.Stacking, 3, BuffClassification.Other, "https://wiki.guildwars2.com/images/c/cd/Vicious_Lacerations.png", 0, GW2Builds.February2020Balance),
                new Buff("Assassin's Presence", AssassinsPresence, Source.Revenant, BuffClassification.Offensive, "https://wiki.guildwars2.com/images/5/54/Assassin%27s_Presence.png", GW2Builds.StartOfLife, GW2Builds.June2022Balance),
                new Buff("Expose Defenses", ExposeDefenses, Source.Revenant, BuffClassification.Other, "https://wiki.guildwars2.com/images/5/5c/Mutilate_Defenses.png"),
                new Buff("Invoking Harmony",InvokingHarmony, Source.Revenant, BuffClassification.Other, "https://wiki.guildwars2.com/images/e/ec/Invoking_Harmony.png"),
                new Buff("Unyielding Devotion",UnyieldingDevotion, Source.Revenant, BuffClassification.Other, "https://wiki.guildwars2.com/images/4/4f/Unyielding_Devotion.png", GW2Builds.April2019Balance, GW2Builds.EndOfLife),
                new Buff("Selfless Amplification",SelflessAmplification, Source.Revenant, BuffClassification.Other, "https://wiki.guildwars2.com/images/2/23/Selfless_Amplification.png"),
                new Buff("Battle Scars", BattleScars, Source.Revenant, BuffStackType.StackingConditionalLoss, 25, BuffClassification.Other, "https://wiki.guildwars2.com/images/3/30/Thrill_of_Combat.png", GW2Builds.February2020Balance, GW2Builds.EndOfLife),
                new Buff("Steadfast Rejuvenation",SteadfastRejuvenation, Source.Revenant, BuffStackType.Stacking, 10, BuffClassification.Other, "https://wiki.guildwars2.com/images/b/bf/Steadfast_Rejuvenation.png"),
        };

        private static readonly HashSet<long> _legendSwaps = new HashSet<long>
        {
            LegendaryAssassinStanceSkill, // Assassin
            LegendaryDemonStanceSkill, // Demon
            LegendaryDwarfStanceSkill, // Dwarf
            LegendaryCentaurStanceSkill, // Centaur
            LegendaryDragonStanceSkill, // Dragon
            LegendaryRenegadeStanceSkill, // Renegade
            LegendaryAllianceStanceSkill, // Alliance
            //LegendaryAllianceStanceUWSkill, // Alliance (UW)
        };

        public static bool IsLegendSwap(long id)
        {
            return _legendSwaps.Contains(id);
        }

        private static HashSet<long> Minions = new HashSet<long>()
        {
            (long)MinionID.VentariTablet
        };
        internal static bool IsKnownMinionID(long id)
        {
            return Minions.Contains(id);
        }

        public static void ProcessGadgets(IReadOnlyList<Player> players, CombatData combatData, AgentData agentData)
        {
            EffectGUIDEvent tabletAutoHealEffect = combatData.GetEffectGUIDEvent(EffectGUIDs.RevenantTabletAutoHeal);
            if (tabletAutoHealEffect != null)
            {
                var allTablets = new HashSet<AgentItem>(combatData.GetEffectEventsByEffectID(tabletAutoHealEffect.ContentID).Select(x => x.Src));
                foreach (AgentItem tablet in allTablets)
                {
                    tablet.OverrideType(AgentItem.AgentType.NPC);
                    tablet.OverrideID(MinionID.VentariTablet);
                }
                if (allTablets.Any())
                {
                    agentData.Refresh();
                }
            }
        }

    }
}
