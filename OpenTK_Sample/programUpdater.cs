using System;
using System.IO;
using System.Threading.Tasks;
using OpenTK;
using System.Collections.Generic;
using System.Threading;

namespace OpenTK_Sample
{
    class ProgramUpdater
    {
        private Plant plant;
        int turnCount;
        int sleepInterval = 1;
        bool cleared;

        public ProgramUpdater(Plant plant)
        {
            this.plant = plant;
            thread = new Thread(new ThreadStart(Update));
            cleared = true;
        }

        private Thread thread;
        public Thread Control { get => thread; set => thread = value; }
        public int SleepInterval { get => sleepInterval; set => sleepInterval = value; }

        private void Update()
        {
            while (true)
            {
                try
                {
                    foreach (var vehicle in plant.Vehicles)
                    {
                        vehicle.OnStatusUpdate();
                    }


                    Thread.Sleep(sleepInterval);
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
                catch (Exception)
                {
                    Thread.Sleep(1);
                    --turnCount;
                    continue;
                }
            }
        }
    }
}
