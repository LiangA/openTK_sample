using System;
using System.IO;
using System.Threading.Tasks;
using OpenTK;
using System.Collections.Generic;
using System.Threading;

namespace OpenTK_Sample
{
    class Updater
    {
        private Plant plant;
        int turnCount;
        bool cleared;
        public Updater(Plant plant)
        {
            this.plant = plant;
            thread = new Thread(new ThreadStart(Update));
            cleared = false;
        }
        private Thread thread;
        public Thread Controle { get => thread; set => thread = value; }
        private void Update()
        {
            while (true)
            {
                try
                {
                    foreach(var vehicle in plant.Vehicles)
                        vehicle.OnStatusUpdate();
                    Thread.Sleep(20);
                    ++turnCount;
                    if (!cleared && plant.Vehicles.Count == 0)
                    {
                        Console.WriteLine("Finished the work at round: " + turnCount.ToString());
                        cleared = true;
                    }
                    else if (plant.Vehicles.Count > 0)
                    {
                        cleared = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    continue;
                }
            }
        }
    }
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

            Updater updater = new Updater(plant);
            updater.Controle.Start();

            Order agent = new Order(plant);
            agent.AddOrders(di.GetFiles("*.csv"), OrderRule.FirstInFirstServe, RoutingStrategies.STurn.FindRoute);
            agent.Agent.Start();

            Visualize display = new Visualize(plant, 400, 200);

            ControlPanel panel = new ControlPanel(plant);
            panel.Show();
            display.Run();
            agent.Agent.Abort();
            updater.Controle.Abort();
        }
    }
}
