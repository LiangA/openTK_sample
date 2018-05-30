using System;
using System.IO;
using System.Threading.Tasks;
using OpenTK;
using System.Collections.Generic;
using System.Threading;

namespace OpenTK_Sample
{
    class ProgramPlant2
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // load plant data
            Plant plant = new Plant();
            plant = Plant.FromFile(@"./Plant2/layout.csv");

            // load orders
            List<string> orderFiles = new List<string>();
            DirectoryInfo di = new DirectoryInfo(@"./Plant2/Orders");
            foreach (var i in di.GetFiles("*.csv"))
                orderFiles.Add(i.FullName);

            ProgramUpdater updater = new ProgramUpdater(plant);
            updater.SleepInterval = 2;

            Order agent = new Order(plant);
            agent.Velocity = 7; // different speed should be appointed to different plant
            agent.AddOrders(di.GetFiles("*.csv"), OrderRule.FirstInFirstServe, RoutingStrategies.STurn.FindRoute);
            agent.AppointMode = AppointMode.WhenHasSpace;

            Visualize display = new Visualize(plant, 300, 600);

            updater.Control.Start();
            agent.Agent.Start();
            //ControlPanel panel = new ControlPanel(plant, agent);
            //panel.Show();

            display.Run();
            agent.Agent.Abort();
            updater.Control.Abort();
        }
    }
}
