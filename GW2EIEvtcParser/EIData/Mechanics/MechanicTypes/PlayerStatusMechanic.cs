﻿using System;
using System.Collections.Generic;
using System.Linq;
using GW2EIEvtcParser.ParsedData;

namespace GW2EIEvtcParser.EIData
{

    internal class PlayerStatusMechanic<T> : StatusMechanic<T> where T : AbstractStatusEvent
    {
        public PlayerStatusMechanic(string inGameName, MechanicPlotlySetting plotlySetting, string shortName, string description, string fullName, int internalCoolDown, StatusGetter getter, StatusChecker condition = null) : base(inGameName, plotlySetting, shortName, description, fullName, internalCoolDown, getter, condition)
        {
        }

        internal override void CheckMechanic(ParsedEvtcLog log, Dictionary<Mechanic, List<MechanicEvent>> mechanicLogs, Dictionary<int, AbstractSingleActor> regroupedMobs)
        {
            foreach (Player p in log.PlayerList)
            {
                foreach (T c in GetEvents(log, p.AgentItem))
                {
                    if (Keep(c, log))
                    {
                        mechanicLogs[this].Add(new MechanicEvent(c.Time, this, p));
                    }
                }
            }
        }
    }
}
