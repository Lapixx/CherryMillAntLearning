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


        public static State FromAction(State state, Action action)
        {
            switch (action)
            {
                case Action.AttackEnemyAnt:
                    state.EnemyAnt = false;
                    // Dood?
                    break;

                case Action.AttackEnemyHill:
                    state.EnemyHill = false;
                    break;

                case Action.DefendHill:

                    break;

                case Action.GoToFriend:

                    break;

                case Action.RandomMove:

                    break;

                case Action.RunAwayFromEnemy:
                    state.EnemyAnt = false;
                    break;

                case Action.RunAwayFromFriend:
                    state.MyAnt = false;
                    break;

                case Action.StandStill:

                    break;

                case Action.TakeFood:
                    state.Food = false;
                    break;
            }
            return state;
        }


        public override int GetHashCode()
        {
            return (Food ? 1 : 0) + (EnemyAnt ? 2 : 0) + (MyAnt ? 4 : 0) + (EnemyHill ? 8 : 0) + (MyHill ? 16 : 0) + (AirSuperiority ? 32 : 0);
        }
    }

    class StateLog
    {
        int[,,] StateTransitions;
        int[,,] NewStateTransitions;
        float[, ,] Probabilities;

        int _x = 64;
        int _y = Enum.GetValues(typeof(Action)).Length;
        string fname;

        public StateLog(string fname)
        {
            StateTransitions = new int[_x, _y, _x];
            NewStateTransitions = new int[_x, _y, _x];
            Probabilities = new float[_x, _y, _x];

            this.fname = fname;
            StreamReader sr = new StreamReader(fname);
            string line;
            string[] parts;
            while ((line = sr.ReadLine()) != null)
            {
                parts = line.Split();
                StateTransitions[int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2])] = int.Parse(parts[3]);
            }
            sr.Close();


            for(int x = 0; x < _x; x++)
                for (int y = 0; y < _y; y++)
                {
                    float n = 0;
                    for (int z = 0; z < _x; z++)
                        n += StateTransitions[x, y, z];
                    for (int z = 0; z < _x; z++)
                        Probabilities[x, y, z] = StateTransitions[x, y, z] / n;
                }
        }

        public void Save()
        {
            AddNewResults();
            StreamWriter sw = new StreamWriter(fname);
            for (int x = 0; x < _x; x++)
                for (int y = 0; y < _y; y++)
                    for (int z = 0; z < _x; z++)
                        if (StateTransitions[x, y, z] != 0)
                            sw.WriteLine(x + " " + y + " " + z + " " + StateTransitions[x, y, z]);
            sw.Close();
        }

        public void AddNewResults()
        {
            for (int x = 0; x < _x; x++)
                for (int y = 0; y < _y; y++)
                    for (int z = 0; z < _x; z++)
                        StateTransitions[x, y, z] += NewStateTransitions[x, y, z];
        }

        public void Increment(State s1, Action a, State s2)
        {
            NewStateTransitions[s1.GetHashCode(), (int)a, s2.GetHashCode()]++;
        }

        public float[] GetProbabilities(State s1, Action a)
        {
            float[] p = new float[_x];
            int s1hash = s1.GetHashCode();
            for (int z = 0; z < _x; z++)
                p[z] = Probabilities[s1hash, (int)a, z];
            return p;
        }
    }
}
