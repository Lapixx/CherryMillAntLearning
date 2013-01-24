using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ants
{
    class Agent
    {
        public DecisionLog decisionLog;
        public List<Location> path = new List<Location>();
        public Location location;
        public Action currentAction;

        public Agent(Location loc)
        {
            location = loc;
            decisionLog = new DecisionLog();
        }

        public void PerformAction(State s, Action a)
        {
            decisionLog.AddDecision(s, a);
            currentAction = a;
        }
    }
}
