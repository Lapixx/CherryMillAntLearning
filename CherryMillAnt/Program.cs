using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ants
{
    class Program
    {
        public static void Main(string[] args)
        {
            //#if DEBUG
            //System.Diagnostics.Debugger.Launch();
            //while (!System.Diagnostics.Debugger.IsAttached) { }
            //#endif
            new Ants().PlayGame(new MyBot());
        }

        public static void Main2(string[] args)
        {
            RewardLog rewardLog = new RewardLog("statetransitions.txt");
            for (int i = 0; i < RewardLog._x; i++)
            {
                double[] x = rewardLog.GetDesirabilities(State.FromInt(i));
                Console.WriteLine("State " + State.FromInt(i).Description());
                for(int j = 0; j<RewardLog._y; j++)
                {
                    if(x[j] > 0)
                        Console.WriteLine((Action)j + ": " + x[j]);
                }
                Console.WriteLine("------------------------------------------");
            }
            Console.ReadLine();
        }
    }
}
