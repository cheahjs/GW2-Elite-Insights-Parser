﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using GW2EIEvtcParser.EIData.BuffSourceFinders;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.ArcDPSEnums;
using static GW2EIEvtcParser.EIData.Buff;

namespace GW2EIEvtcParser.EIData
{
    public class BuffsContainer
    {

        public IReadOnlyDictionary<long, Buff> BuffsByIds { get; }
        public IReadOnlyDictionary<BuffClassification, IReadOnlyList<Buff>> BuffsByClassification { get; }
        public IReadOnlyDictionary<ParserHelper.Source, IReadOnlyList<Buff>> BuffsBySource { get; }
        private readonly Dictionary<string, Buff> _buffsByName;

        private readonly BuffSourceFinder _buffSourceFinder;


        internal BuffsContainer(CombatData combatData, ParserController operation)
        {
            var AllBuffs = new List<List<Buff>>()
            {
                Boons,
                Conditions,
                Commons,
                Gear,
                NormalFoods,
                AscendedFood,
                FoodProcs,
                Utilities,
                Potions,
                Writs,
                FightSpecific,
                FractalInstabilities,
                //
                RevenantHelper.Buffs,
                HeraldHelper.Buffs,
                RenegadeHelper.Buffs,
                VindicatorHelper.Buffs,
                //
                WarriorHelper.Buffs,
                BerserkerHelper.Buffs,
                SpellbreakerHelper.Buffs,
                BladeswornHelper.Buffs,
                //
                GuardianHelper.Buffs,
                DragonhunterHelper.Buffs,
                FirebrandHelper.Buffs,
                WillbenderHelper.Buffs,
                //
                RangerHelper.Buffs,
                DruidHelper.Buffs,
                SoulbeastHelper.Buffs,
                UntamedHelper.Buffs,
                //
                ThiefHelper.Buffs,
                DaredevilHelper.Buffs,
                DeadeyeHelper.Buffs,
                SpecterHelper.Buffs,
                //
                EngineerHelper.Buffs,
                ScrapperHelper.Buffs,
                HolosmithHelper.Buffs,
                MechanistHelper.Buffs,
                //
                MesmerHelper.Buffs,
                ChronomancerHelper.Buffs,
                MirageHelper.Buffs,
                VirtuosoHelper.Buffs,
                //
                NecromancerHelper.Buffs,
                ReaperHelper.Buffs,
                ScourgeHelper.Buffs,
                HarbingerHelper.Buffs,
                //
                ElementalistHelper.Buffs,
                TempestHelper.Buffs,
                WeaverHelper.Buffs,
                CatalystHelper.Buffs,
            };
            var currentBuffs = new List<Buff>();
            foreach (List<Buff> buffs in AllBuffs)
            {
                currentBuffs.AddRange(buffs.Where(x => x.Available(combatData)));
            }
            _buffsByName = currentBuffs.GroupBy(x => x.Name).ToDictionary(x => x.Key, x =>
            {
                var list = x.ToList();
                if (list.Count > 1)
                {
                    throw new InvalidDataException("Same name present multiple times in buffs - " + x.First().Name);
                }
                return x.First();
            });
            // Unknown consumables
            var buffIDs = new HashSet<long>(currentBuffs.Select(x => x.ID));
            var foodAndUtility = new List<BuffInfoEvent>(combatData.GetBuffInfoEvent(BuffCategory.Enhancement));
            foodAndUtility.AddRange(combatData.GetBuffInfoEvent(BuffCategory.Food));
            foreach (BuffInfoEvent buffInfoEvent in foodAndUtility)
            {
                if (!buffIDs.Contains(buffInfoEvent.BuffID))
                {
                    string name = buffInfoEvent.Category == BuffCategory.Enhancement ? "Utility" : "Food";
                    string link = buffInfoEvent.Category == BuffCategory.Enhancement ? "https://wiki.guildwars2.com/images/2/23/Nourishment_utility.png" : "https://wiki.guildwars2.com/images/c/ca/Nourishment_food.png";
                    operation.UpdateProgressWithCancellationCheck("Creating consumable " + name + " " + buffInfoEvent.BuffID);
                    currentBuffs.Add(CreateCustomConsumable(name, buffInfoEvent.BuffID, link, buffInfoEvent.MaxStacks));
                }
            }
            //
            BuffsByIds = currentBuffs.GroupBy(x => x.ID).ToDictionary(x => x.Key, x =>
            {
                var list = x.ToList();
                if (list.Count > 1 && x.Key != SkillIDs.NoBuff && x.Key != SkillIDs.Unknown)
                {
                    throw new InvalidDataException("Same id present multiple times in buffs - " + x.First().ID);
                }
                return x.First();
            });
            operation.UpdateProgressWithCancellationCheck("Adjusting Buffs");
            BuffInfoSolver.AdjustBuffs(combatData, BuffsByIds, operation);
            foreach (Buff buff in currentBuffs)
            {
                BuffInfoEvent buffInfoEvt = combatData.GetBuffInfoEvent(buff.ID);
                if (buffInfoEvt != null)
                {
                    foreach (BuffFormula formula in buffInfoEvt.Formulas)
                    {
                        if (formula.Attr1 == BuffAttribute.Unknown)
                        {
                            operation.UpdateProgressWithCancellationCheck("Unknown Formula for " + buff.Name + ": " + formula.GetDescription(true, BuffsByIds));
                        }
                    }
                }
            }
            BuffsByClassification = currentBuffs.GroupBy(x => x.Classification).ToDictionary(x => x.Key, x => (IReadOnlyList<Buff>)x.ToList());
            BuffsBySource = currentBuffs.GroupBy(x => x.Source).ToDictionary(x => x.Key, x => (IReadOnlyList<Buff>)x.ToList());
            //
            _buffSourceFinder = GetBuffSourceFinder(combatData, new HashSet<long>(BuffsByClassification[BuffClassification.Boon].Select(x => x.ID)));
        }

        public bool TryGetBuffByName(string name, out Buff buff)
        {
            return _buffsByName.TryGetValue(name, out buff);
        }

        internal AgentItem TryFindSrc(AgentItem dst, long time, long extension, ParsedEvtcLog log, long buffID)
        {
            return _buffSourceFinder.TryFindSrc(dst, time, extension, log, buffID);
        }

        // Non shareable buffs
        public IReadOnlyList<Buff> GetPersonalBuffsList(ParserHelper.Spec spec)
        {
            var result = new List<Buff>();
            foreach (ParserHelper.Source src in ParserHelper.SpecToSources(spec))
            {
                if (BuffsBySource.TryGetValue(src, out IReadOnlyList<Buff> list))
                {
                    result.AddRange(list.Where(x => x.Classification == BuffClassification.Other));
                }
            }
            return result;
        }
    }
}
