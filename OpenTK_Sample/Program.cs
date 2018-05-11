﻿using System;
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
        public Updater(Plant plant)
        {
            this.plant = plant;
            thread = new Thread(new ThreadStart(Update));
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
                    Thread.Sleep(10);
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
            // Test message
            
            Order order = new Order();
            order.FromFiles(orderFiles);

            foreach (var i in order.TasksForPlant)
            {
                foreach (var j in i)
                {
                    plant.Tasks.Add(j.Target);
                }
            }

            ////Read tasks
            //FileInfo tasksFile = new FileInfo(@"./Plant1/Orders/Tasks.csv");
            //StreamReader reader = new StreamReader(tasksFile.OpenRead());
            //while (!reader.EndOfStream)
            //{
            //    string[] line = reader.ReadLine().Split(',');
            //    if (line.Length == 3)
            //        plant.Tasks.Add(new Vector2d(Double.Parse(line[0]), Double.Parse(line[1])));
            //}

            Visualize display = new Visualize(plant, 400, 200);

            Updater updater = new Updater(plant);
            updater.Controle.Start();

            ControlPanel panel = new ControlPanel(plant);
            panel.Show();
            display.Run();
            updater.Controle.Abort();
        }
    }
}
