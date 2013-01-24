using System;
using System.Collections.Generic;
using System.IO;

namespace Ants
{

	class MyBot : Bot
    {

        RewardLog rewardLog;
        List<DecisionLog> decisionLogs;
        Dictionary<Location, Agent> agents;
        int radius;

        public MyBot()
        {
            decisionLogs = new List<DecisionLog>();
            rewardLog = new RewardLog("statetransitions.txt");
            agents = new Dictionary<Location, Agent>();
        }


		// DoTurn is run once per turn
		public override void DoTurn (IGameState state)
        {
            if(radius == default(int))
                radius = (int)Math.Sqrt(state.ViewRadius2);

            foreach (Ant a in state.MyAnts)
            {
                if(!agents.ContainsKey(a)){ // Spawn ant
                    Agent ag = new Agent();
                    decisionLogs.Add(ag.decisionLog);
                    agents.Add(a, ag);
                }

                Agent agent = agents[a];

                if (agent.path.Count != 0)
                {
                    if (!state.GetIsPassable(agent.path[0]))
                        agent.path = Pathfinding.FindPath(a, agent.path[agent.path.Count - 1], state);
                    Location next = agent.path[0];
                    agent.path.RemoveAt(0);
                    IssueOrder(a, ((List<Direction>)state.GetDirections(a, next))[0]);
                }

                if (agent.path.Count == 0)
                {
                    State s = new State();
                    Location l;
                    Tile t;
                    for (int x = -radius; x <= radius; x++)
                    {
                        for (int y = -radius; y <= radius; y++)
                        {
                            l = a + new Location(y, x);
                            t = state[l];
                            if (t == Tile.Ant)
                            {
                                if (agents.ContainsKey(l))
                                    s.MyAnt = true;
                                else
                                    s.EnemyAnt = true;
                            }
                            else if (t == Tile.Food)
                                s.Food = true;
                            else if (t == Tile.Hill)
                            {
                                if (state.MyHills.Contains(l))
                                    s.MyHill = true;
                                else
                                    s.EnemyHill = true;
                            }
                        }
                    }
                }
            }
		}

        public void UpdateRewardLog()
        {
            foreach (DecisionLog dl in decisionLogs)
                dl.AddResults(rewardLog);
            rewardLog.Save();
        }

        public override void OnGameEnd()
        {
            //UpdateRewardLog();
        }
    }
}