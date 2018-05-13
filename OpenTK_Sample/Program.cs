using System;
using System.IO;
using System.Threading.Tasks;
using OpenTK;
using System.Collections.Generic;
using System.Threading;

namespace OpenTK_Sample
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // load plant data
            Plant plant = new Plant();
            plant = Plant.FromFile(@"./Plant1/layout.csv");

            // load orders
            List<string> orderFiles = new List<string>();
            DirectoryInfo di = new DirectoryInfo(@"./Plant1/Orders");
            foreach (var i in di.GetFiles("*.csv"))
                orderFiles.Add(i.FullName);

            ProgramUpdater updater = new ProgramUpdater(plant);
            updater.SleepInterval = 1;

            Order agent = new Order(plant);
            agent.Velocity = 2.5; // different speed should be appointed to different plant
            agent.AddOrders(di.GetFiles("*.csv"), OrderRule.FirstInFirstServe, RoutingStrategies.STurn.FindRoute);
            agent.AppointMode = AppointMode.WhenPlantCleared;

            Visualize display = new Visualize(plant, 600, 300);

            updater.Controle.Start();
            agent.Agent.Start();
            //ControlPanel panel = new ControlPanel(plant, agent);
            //panel.Show();

            display.Run();
            agent.Agent.Abort();
            updater.Controle.Abort();
        }
    }
}
