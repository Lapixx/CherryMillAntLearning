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
    }
}
