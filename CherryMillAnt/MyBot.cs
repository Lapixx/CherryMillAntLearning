using System;
using System.Collections.Generic;
using System.IO;

namespace Ants
{
	class MyBot : Bot
    {

        const double DIE = -2;
        const double KILL = 3;
        const double EAT = 2;
        const double WIN = 10;
        const double LOSE = -10;

        double exploration = 0.1;
        RewardLog rewardLog;
        List<DecisionLog> decisionLogs;
        List<Agent> agents;
        int radius;
        Random random;

        public MyBot()
        {
            decisionLogs = new List<DecisionLog>();
            rewardLog = new RewardLog("statetransitions.txt");
            agents = new List<Agent>();
            random = new Random();
        }

		// DoTurn is run once per turn
		public override void DoTurn (IGameState state)
        {
            if(radius == default(int)) // Init
                radius = (int)Math.Sqrt(state.ViewRadius2);

            foreach (Agent agent in new List<Agent>(agents)) // Die
            {
                if (!state.MyAnts.Contains(new Ant(agent.location.Row, agent.location.Col, state.MyAnts[0].Team)))
                {
                    agent.decisionLog.AddReward(DIE);
                    agents.Remove(agent);
                }
            }

            foreach (Ant a in state.MyAnts) //Spawn new ants
            {
                bool spawn = true;
                foreach (Agent agent in agents)
                {
                    if (agent.location.Equals(a))
                    {
                        spawn = false;
                        break;
                    }
                }

                if (spawn)
                {
                    Agent ag = new Agent(a);
                    decisionLogs.Add(ag.decisionLog);
                    agents.Add(ag);

                    foreach (Agent age in agents) // Take food
                    {
                        if(age.path.Count > 0)
                            if (age.currentAction == Action.TakeFood && state.GetDistance(age.location, age.path[age.path.Count - 1]) <= 1)
                                age.decisionLog.AddReward(EAT);
                    }
                }
            }

            foreach(Agent agent in agents){

                if (agent.path.Count == 0)
                {
                    State s = CalculateState(agent.location, state);
                    double[] desirabilities = rewardLog.GetDesirabilities(s);
                    int i = -1;
                    HashSet<Action> aas = s.GetActions();
                    double x = random.NextDouble();
                    if (x <= exploration) // Explore
                    {
                        do
                        {
                            i = random.Next(Enum.GetValues(typeof(Action)).Length);
                        }
                        while (!aas.Contains((Action)i));
                    }
                    else // Exploit
                    {
                        x = random.NextDouble();
                        for (i = 0; i < Enum.GetValues(typeof(Action)).Length; i++)
                        {
                            if (x <= desirabilities[i])
                                break;
                            else
                                x -= desirabilities[i];
                        }
                    }

                    if (i == Enum.GetValues(typeof(Action)).Length)
                    {
                        do
                        {
                            i = random.Next(Enum.GetValues(typeof(Action)).Length);
                        }
                        while (!aas.Contains((Action)i));
                    }

                    PerformAction(agent, s, (Action)i, state);
                }

                if (agent.path.Count == 0)
                    continue;

                if (!state.GetIsPassable(agent.path[0]))
                    agent.path = Pathfinding.FindPath(agent.location, agent.path[agent.path.Count - 1], state);
                Location next = agent.path[0];
                agent.path.RemoveAt(0);

                if (state.EnemyHills.Count > 0)
                    if (state.EnemyHills.Contains(new AntHill(next.Row, next.Col, state.EnemyHills[0].Team)))
                        agent.decisionLog.AddReward(WIN);

                IssueOrder(agent.location, ((List<Direction>)state.GetDirections(agent.location, next))[0]);
                agent.location = next;
            }
		}

        public void PerformAction(Agent agent, State state, Action action, IGameState gamestate)
        {
            Location tgt = null;
            switch (action)
            {
                case Action.RunAwayFromEnemy:
                case Action.AttackEnemyAnt:
                    tgt = GetNearestEnemy(agent.location, gamestate);
                    break;
                case Action.AttackEnemyHill:
                    tgt = GetNearestEnemyHill(agent.location, gamestate);
                    break;
                case Action.DefendHill:
                    tgt = GetNearestHill(agent.location, gamestate);
                    break;
                case Action.RandomMove:
                    tgt = GetRandomLocation(agent.location, gamestate);
                    break;
                case Action.RunAwayFromFriend:
                case Action.GoToFriend:
                    tgt = GetNearestFriend(agent.location, gamestate);
                    break;
                case Action.StandStill:
                    tgt = agent.location;
                    break;
                case Action.TakeFood:
                    tgt = GetNearestFood(agent.location, gamestate);
                    break;
                default:
                    break;
            }

            if (tgt == null)
                throw new Exception("B tgt is null >>> " + action);

            if (action == Action.RunAwayFromEnemy || action == Action.RunAwayFromFriend)
            {
                tgt = (2 * agent.location - tgt) % new Location(gamestate.Height, gamestate.Width);
            }

            if (tgt == null)
                throw new Exception("A tgt is null >>> " + action);
            tgt = GetNearestPassable(tgt, gamestate);

            agent.path = Pathfinding.FindPath(agent.location, tgt, gamestate);
            if (agent.path == null)
                agent.path = new List<Location>();

            agent.PerformAction(state, action);
        }

        public Location GetNearestPassable(Location location, IGameState state)
        {
            Queue<Location> open = new Queue<Location>();
            HashSet<Location> closed = new HashSet<Location>();
            Location lx;

            open.Enqueue(location);

            while(open.Count > 0)
            {
                lx = open.Dequeue();
                if (state.GetIsPassable(lx))
                    return lx;
                closed.Add(lx);
                List<Location> ns = Pathfinding.GetNeighbours(lx, state);
                foreach (Location l in ns)
                    if(!closed.Contains(l))
                        open.Enqueue(l);
            }

            return null;
        }

        public Location GetRandomLocation(Location location, IGameState state)
        {
            Location loc;
            int x, y;
            do
            {
                x = random.Next(radius) - radius / 2;
                y = random.Next(radius) - radius / 2;
                loc = (new Location(y, x) + location) % new Location(state.Height, state.Width);
            }
            while (!state.GetIsPassable(loc));
            return loc;
        }

        public Location GetNearestEnemy(Location location, IGameState state)
        {
            int d = int.MaxValue;
            int x;
            Location l = null;
            foreach (Ant a in state.EnemyAnts)
            {
                x = state.GetDistance(location, a);
                if (x < d)
                {
                    d = x;
                    l = a;
                }
            }
            return l;
        }

        public Location GetNearestHill(Location location, IGameState state)
        {
            int d = int.MaxValue;
            int x;
            Location l = null;
            foreach (AntHill a in state.MyHills)
            {
                x = state.GetDistance(location, a);
                if (x < d)
                {
                    d = x;
                    l = a;
                }
            }
            return l;
        }

        public Location GetNearestEnemyHill(Location location, IGameState state)
        {
            int d = int.MaxValue;
            int x;
            Location l = null;
            foreach (AntHill a in state.EnemyHills)
            {
                x = state.GetDistance(location, a);
                if (x < d)
                {
                    d = x;
                    l = a;
                }
            }
            return l;
        }

        public Location GetNearestFood(Location location, IGameState state)
        {
            int d = int.MaxValue;
            int x;
            Location l = null;
            foreach (Location a in state.FoodTiles)
            {
                x = state.GetDistance(location, a);
                if (x < d)
                {
                    d = x;
                    l = a;
                }
            }
            return l;
        }

        public Location GetNearestFriend(Location location, IGameState state)
        {
            int d = int.MaxValue;
            int x;
            Location l = null;
            foreach (Ant a in state.MyAnts)
            {
                if (a.Equals(location))
                    continue;
                x = state.GetDistance(location, a);
                if (x < d)
                {
                    d = x;
                    l = a;
                }
            }
            return l;
        }

        public State CalculateState(Location a, IGameState state)
        {
            State s = new State();
            Location l;
            Tile t;
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    l = (a + new Location(y, x)) % new Location(state.Height, state.Width);
                    t = state[l];
                    if (t == Tile.Ant)
                    {
                        if(state.MyAnts.Contains(new Ant(l.Row, l.Col, state.MyAnts[0].Team)))
                            s.MyAnt = true;
                        else
                            s.EnemyAnt = true;
                    }
                    else if (t == Tile.Food)
                        s.Food = true;
                    else if (t == Tile.Hill)
                    {
                        if (state.MyHills.Contains(new AntHill(l.Row, l.Col, state.MyHills[0].Team)))
                            s.MyHill = true;
                        else
                            s.EnemyHill = true;
                    }
                }
            }
            s.AirSuperiority = state.MyAnts.Count > state.EnemyAnts.Count;
            return s;
        }

        public void UpdateRewardLog()
        {
            foreach (DecisionLog dl in decisionLogs)
                dl.AddResults(rewardLog);
            rewardLog.Save();
        }

        public override void OnGameEnd(IGameState state)
        {
            UpdateRewardLog();
        }
    }
}