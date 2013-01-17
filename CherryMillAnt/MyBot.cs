using System;
using System.Collections.Generic;
using System.IO;

namespace Ants
{

	class MyBot : Bot
    {

        public MyBot()
        {
            State s0 = new State();
            State s1 = new State();
            s1.Food = true;
            State s2 = new State();
            s2.EnemyAnt = true;
            StateLog sl = new StateLog("statetransitions.txt");

            sl.Increment(s1, Action.TakeFood, s0);
            sl.Increment(s1, Action.TakeFood, s0);
            sl.Increment(s2, Action.RunAwayFromEnemy, s0);

            sl.Save();
        }


		// DoTurn is run once per turn
		public override void DoTurn (IGameState state)
        {
            foreach (Ant a in state.MyAnts)
            {
               
            }
		}
        
        
    }
}