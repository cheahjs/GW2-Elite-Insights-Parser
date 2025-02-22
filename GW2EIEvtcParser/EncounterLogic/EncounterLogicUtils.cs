﻿using System;
using System.Collections.Generic;
using System.Linq;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.Extensions;
using GW2EIEvtcParser.ParsedData;

namespace GW2EIEvtcParser.EncounterLogic
{
    internal static class EncounterLogicUtils
    {
        internal static void RegroupTargetsByID(int id, AgentData agentData, IReadOnlyList<CombatItem> combatItems, IReadOnlyDictionary<uint, AbstractExtensionHandler> extensions)
        {
            IReadOnlyList<AgentItem> agents = agentData.GetNPCsByID(id);
            if (agents.Count > 1)
            {
                AgentItem firstItem = agents.First();
                var agentValues = new HashSet<ulong>(agents.Select(x => x.Agent));
                var newTargetAgent = new AgentItem(firstItem);
                newTargetAgent.OverrideAwareTimes(agents.Min(x => x.FirstAware), agents.Max(x => x.LastAware));
                agentData.SwapMasters(new HashSet<AgentItem>(agents), newTargetAgent);
                agentData.ReplaceAgentsFromID(newTargetAgent);
                foreach (CombatItem c in combatItems)
                {
                    if (agentValues.Contains(c.SrcAgent) && c.SrcIsAgent(extensions))
                    {
                        c.OverrideSrcAgent(newTargetAgent.Agent);
                    }
                    if (agentValues.Contains(c.DstAgent) && c.DstIsAgent(extensions))
                    {
                        c.OverrideDstAgent(newTargetAgent.Agent);
                    }
                }
            }
        }

        internal static void NegateDamageAgainstBarrier(CombatData combatData, IReadOnlyList<AgentItem> agentItems)
        {
            var dmgEvts = new List<AbstractHealthDamageEvent>();
            foreach (AgentItem agentItem in agentItems)
            {
                dmgEvts.AddRange(combatData.GetDamageTakenData(agentItem));
            }
            foreach (AbstractHealthDamageEvent de in dmgEvts)
            {
                if (de.ShieldDamage > 0)
                {
                    de.NegateShieldDamage();
                }
            }
        }

        /*protected static void AdjustTimeRefreshBuff(Dictionary<AgentItem, List<AbstractBuffEvent>> buffsByDst, Dictionary<long, List<AbstractBuffEvent>> buffsById, long id)
        {
            if (buffsById.TryGetValue(id, out List<AbstractBuffEvent> buffList))
            {
                var agentsToSort = new HashSet<AgentItem>();
                foreach (AbstractBuffEvent be in buffList)
                {
                    if (be is AbstractBuffRemoveEvent abre)
                    {
                        // to make sure remove events are before applications
                        abre.OverrideTime(abre.Time - 1);
                        agentsToSort.Add(abre.To);
                    }
                }
                if (buffList.Count > 0)
                {
                    buffsById[id].Sort((x, y) => x.Time.CompareTo(y.Time));
                }
                foreach (AgentItem a in agentsToSort)
                {
                    buffsByDst[a].Sort((x, y) => x.Time.CompareTo(y.Time));
                }
            }
        }*/

        internal static List<ErrorEvent> GetConfusionDamageMissingMessage(int arcdpsVersion)
        {
            if (arcdpsVersion > ArcDPSEnums.ArcDPSBuilds.ProperConfusionDamageSimulation)
            {
                return new List<ErrorEvent>();
            }
            return new List<ErrorEvent>()
            {
                new ErrorEvent("Missing confusion damage")
            };
        }
        internal static List<AbstractBuffEvent> GetFilteredList(CombatData combatData, long buffID, AbstractSingleActor target, bool beginWithStart, bool padEnd)
        {
            bool needStart = beginWithStart;
            var main = combatData.GetBuffData(buffID).Where(x => x.To == target.AgentItem && (x is BuffApplyEvent || x is BuffRemoveAllEvent)).ToList();
            var filtered = new List<AbstractBuffEvent>();
            for (int i = 0; i < main.Count; i++)
            {
                AbstractBuffEvent c = main[i];
                if (needStart && c is BuffApplyEvent)
                {
                    needStart = false;
                    filtered.Add(c);
                }
                else if (!needStart && c is BuffRemoveAllEvent)
                {
                    // consider only last remove event before another application
                    if ((i == main.Count - 1) || (i < main.Count - 1 && main[i + 1] is BuffApplyEvent))
                    {
                        needStart = true;
                        filtered.Add(c);
                    }
                }
            }
            if (padEnd && filtered.Any() && filtered.Last() is BuffApplyEvent)
            {
                AbstractBuffEvent last = filtered.Last();
                filtered.Add(new BuffRemoveAllEvent(ParserHelper._unknownAgent, last.To, target.LastAware, int.MaxValue, last.BuffSkill, BuffRemoveAllEvent.FullRemoval, int.MaxValue));
            }
            return filtered;
        }

        internal static bool AtLeastOnePlayerAlive(CombatData combatData, FightData fightData, long timeToCheck, IReadOnlyCollection<AgentItem> playerAgents)
        {
            int playerDeadOrDCCount = 0;
            foreach (AgentItem playerAgent in playerAgents)
            {
                var deads = new List<Segment>();
                var downs = new List<Segment>();
                var dcs = new List<Segment>();
                playerAgent.GetAgentStatus(deads, downs, dcs, combatData, fightData);
                if (deads.Any(x => x.ContainsPoint(timeToCheck)))
                {
                    playerDeadOrDCCount++;
                }
                else if (dcs.Any(x => x.ContainsPoint(timeToCheck)))
                {
                    playerDeadOrDCCount++;
                }
            }
            if (playerDeadOrDCCount == playerAgents.Count)
            {
                return false;
            }
            return true;
        }


        internal delegate bool ChestAgentChecker(AgentItem agent);

        internal static bool FindChestGadget(ArcDPSEnums.ChestID chestID, AgentData agentData, IReadOnlyList<CombatItem> combatData, Point3D chestPosition, ChestAgentChecker chestChecker)
        {
            if (chestID == ArcDPSEnums.ChestID.None)
            {
                return false;
            }
            AgentItem chest = combatData.Where(evt =>
            {
                if (evt.IsStateChange != ArcDPSEnums.StateChange.Position)
                {
                    return false;
                }
                AgentItem agent = agentData.GetAgent(evt.SrcAgent, evt.Time);
                if (agent.Type != AgentItem.AgentType.Gadget)
                {
                    return false;
                }
                (float x, float y, float z) = AbstractMovementEvent.UnpackMovementData(evt.DstAgent, evt.Value);
                if (Math.Abs(x - chestPosition.X) < 5 && Math.Abs(y - chestPosition.Y) < 5)
                {
                    return true;
                }
                return false;
            }
            ).Select(x => agentData.GetAgent(x.SrcAgent, x.Time)).FirstOrDefault(x => chestChecker(x));
            if (chest != null)
            {
                chest.OverrideID(chestID);
                return true;
            }
            return false;
        }
    }
}
