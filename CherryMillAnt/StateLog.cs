using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Ants
{
    public enum Action { TakeFood, AttackEnemyAnt, AttackEnemyHill, RunAwayFromEnemy, GoToFriend, RunAwayFromFriend, DefendHill, RandomMove, StandStill };

    public struct State
    {
        public bool Food;
        public bool EnemyAnt;
        public bool MyAnt;
        public bool EnemyHill;
        public bool MyHill;

        public bool AirSuperiority;
        //public bool HillInDanger; 

        public string Description()
        {
            string x = "";
            if (Food)
                x += "Food ";
            if (EnemyAnt)
                x += "Enemy ";
            if (MyAnt)
                x += "Friend ";
            if (EnemyHill)
                x += "EnemyHill ";
            if (MyHill)
                x += "MyHill ";
            if (AirSuperiority)
                x += "AirSupr ";
            return x;
        }

        public static State FromInt(int n)
        {
            State x = new State();

            x.Food = (n % 2 == 1);
            n /= 2;
            x.EnemyAnt = (n % 2 == 1);
            n /= 2;
            x.MyAnt = (n % 2 == 1);
            n /= 2;
            x.EnemyHill = (n % 2 == 1);
            n /= 2;
            x.MyHill = (n % 2 == 1);
            n /= 2;
            x.AirSuperiority = (n % 2 == 1);
            n /= 2;

            return x;
        }

        public override int GetHashCode()
        {
            return (Food ? 1 : 0) + (EnemyAnt ? 2 : 0) + (MyAnt ? 4 : 0) + (EnemyHill ? 8 : 0) + (MyHill ? 16 : 0) + (AirSuperiority ? 32 : 0);
        }

        public HashSet<Action> GetActions()
        {
            HashSet<Action> result = new HashSet<Action>();

            if (Food)
                result.Add(Action.TakeFood);
            if(EnemyAnt)
            {
                result.Add(Action.AttackEnemyAnt);
                result.Add(Action.RunAwayFromEnemy);
            }
            if(MyAnt)
            {
                result.Add(Action.GoToFriend);
                result.Add(Action.RunAwayFromFriend);
            }
            if(EnemyHill)
                result.Add(Action.AttackEnemyHill);
            if(MyHill)
                result.Add(Action.DefendHill);
            result.Add(Action.StandStill);
            result.Add(Action.RandomMove);

            return result;
        }
    }

    class RewardLog
    {
        public static int _x = 64;
        public static int _y = Enum.GetValues(typeof(Action)).Length;

        double[,] ExpectedReward;
        int[,] Frequencies;
        double[][] Desirability;

        string fname;

        public RewardLog(string fname)
        {
            ExpectedReward = new double[_x, _y];
            Frequencies = new int[_x, _y];
            Desirability = new double[_x][];

            this.fname = fname;
            if (!File.Exists(fname))
                (File.CreateText(fname)).Close();
            StreamReader sr = new StreamReader(fname);
            
            string line;
            string[] parts;
            while ((line = sr.ReadLine()) != null)
            {
                parts = line.Split();
                ExpectedReward[int.Parse(parts[0]), int.Parse(parts[1])] = double.Parse(parts[2]);
                Frequencies[int.Parse(parts[0]), int.Parse(parts[1])] = int.Parse(parts[3]);
            }
            sr.Close();

            for (int x = 0; x < _x; x++)
            {
                Desirability[x] = new double[_y];
                double sum = 0;
                double low = 0;
                HashSet<Action> xs = State.FromInt(x).GetActions();
                for (int y = 0; y < _y; y++)
                    if (ExpectedReward[x, y] < low)
                        low = ExpectedReward[x, y];
                for(int y = 0; y < _y; y++)
                    if(xs.Contains((Action)y))
                        sum += ExpectedReward[x, y] - low;
                for (int y = 0; y < _y; y++)
                    if(xs.Contains((Action)y))
                        Desirability[x][y] = (ExpectedReward[x, y] - low) / sum;
            }
        }

        public void Save()
        {
            StreamWriter sw = new StreamWriter(fname);
            for (int x = 0; x < _x; x++)
                for (int y = 0; y < _y; y++)
                        if (Frequencies[x, y] != 0)
                            sw.WriteLine(x + " " + y + " " + ExpectedReward[x, y] + " " + Frequencies[x, y]);
            sw.Close();
        }

        public void AddResult(State s1, Action a, double reward, int freq)
        {
            int hash = s1.GetHashCode();
            double n = (double)Frequencies[hash, (int)a];
            double n2 = n + (double)freq;
            ExpectedReward[hash, (int)a] = ExpectedReward[hash, (int)a] * (n / n2) + reward * ((double)freq / n2);
            Frequencies[hash, (int)a] += freq;
        }

        public double[] GetDesirabilities(State s1)
        {
            return Desirability[s1.GetHashCode()];
        }
    }

    class DecisionLog
    {
        float DiscountFactor = 0.9f;
        LinkedList<Tuple<State, Action>> Decisions;
        Dictionary<State, Dictionary<Action, double>> Rewards;
        Dictionary<State, Dictionary<Action, int>> Frequencies;

        public DecisionLog()
        {
            Decisions = new LinkedList<Tuple<State,Action>>();
            Rewards = new Dictionary<State, Dictionary<Action, double>>();
            Frequencies = new Dictionary<State, Dictionary<Action, int>>();
        }

        public void AddDecision(State s1, Action a)
        {
            Decisions.AddLast(new Tuple<State, Action>(s1, a));
            if (!Rewards.ContainsKey(s1))
            {
                Rewards.Add(s1, new Dictionary<Action, double>());
                Frequencies.Add(s1, new Dictionary<Action, int>());
            }
            if (!Rewards[s1].ContainsKey(a))
            {
                Rewards[s1].Add(a, 0);
                Frequencies[s1].Add(a, 0);
            }
            Frequencies[s1][a]++;
        }

        public void AddReward(double reward)
        {
            if(Decisions.Count == 0)
                return;
            LinkedListNode<Tuple<State, Action>> node = Decisions.Last;
            do
            {
                Frequencies[node.Value.Item1][node.Value.Item2] += 1;
                Rewards[node.Value.Item1][node.Value.Item2] += reward;
                reward *= DiscountFactor;
                node = node.Previous;
            }
            while (node != null);
        }

        public void AddResults(RewardLog rl)
        {
            int f;
            foreach (State s in Rewards.Keys)
            {
                foreach (Action a in Rewards[s].Keys)
                {
                    f = Frequencies[s][a];
                    double rew = Rewards[s][a];
                    rl.AddResult(s, a, Rewards[s][a] / (double)f, f);
                }
            }
        }
    }
}
