using System;
using System.Collections.Generic;
using System.IO;

namespace Ants
{

	class MyBot : Bot
    {

        RewardLog rewardLog;
        List<DecisionLog> decisionLogs;

        public MyBot()
        {
            decisionLogs = new List<DecisionLog>();
            rewardLog = new RewardLog("statetransitions.txt");

            DecisionLog dl1 = new DecisionLog();
            DecisionLog dl2 = new DecisionLog();
            decisionLogs.Add(dl1);
            decisionLogs.Add(dl2);

            dl1.AddDecision(State.FromInt(1), Action.StandStill);
            dl1.AddDecision(State.FromInt(2), Action.AttackEnemyAnt);
            dl1.AddDecision(State.FromInt(3), Action.AttackEnemyHill);
            dl1.AddReward(-2f);

            dl2.AddDecision(State.FromInt(1), Action.TakeFood);
            dl2.AddDecision(State.FromInt(2), Action.RunAwayFromEnemy);
            dl2.AddDecision(State.FromInt(3), Action.AttackEnemyHill);
            dl2.AddReward(10);

            UpdateRewardLog();
        }


		// DoTurn is run once per turn
		public override void DoTurn (IGameState state)
        {
            foreach (Ant a in state.MyAnts)
            {
                
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