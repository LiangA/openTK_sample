using System;
using System.Threading;
using System.Collections.Generic;
using OpenTK;
using System.IO;
using System.Drawing;

namespace OpenTK_Sample
{
    public struct Task
    {
        public Task(Vector2d tar, int prehalt = 0, int posthalt = 0)
        {
            target = tar;
            haltBefore = prehalt;
            haltAfter = posthalt;
        }

        public Task(Vector2d tar, int posthalt)
        {
            target = tar;
            haltBefore = 0;
            haltAfter = posthalt;
        }

        int haltBefore, haltAfter;
        Vector2d target;

        public int HaltBefore { get => haltBefore; set => haltBefore = value; }
        public int HaltAfter { get => haltAfter; set => haltAfter = value; }
        public Vector2d Target { get => target; set => target = value; }
    }

    /*
    since orders can be different from tasks, create an order class
    for example, if we do batching for orders, two or more orders could be one tasks, which may refer to List<Tasks>
    class Order means to load order(s) from file, and also produces tasks with different rules as outputs for Vehicle
    note that in the end we should generate List<tasks> for Plant and each tasks is for Vehicle
    */
    enum OrderRule
    {
        FirstInFirstServe, 
        NearestFirstServe
    }

    enum AppointMode
    {
        WhenHasSpace,
        WhenPlantCleared, 
        FixedInterval
    }

    class Order
    {
        private Plant plant;
        private Thread thread;
        public Thread Agent { get => thread; set => thread = value; }
        public AppointMode AppointMode { get => appointMode; set => appointMode = value; }

        private int vehicleCount, orderCount;
        private Vector2d startLocation;
        private AppointMode appointMode;
        Queue<IList<Task>> orders;
        private void GenCar()
        {
            while (true)
            {
                //Console.Write(".");
                try
                {
                    if (ShouldAppoint() && orders.Count > 0) {
                        Vehicle vehicle = new Vehicle(plant, startLocation);
                        plant.Vehicles.Add(vehicle);
                        vehicle.Velocity = 2.5;
                        vehicle.SetTasks(orders.Dequeue());
                        vehicle.StatusUpdate += RemoveSuspendUpdater;
                        vehicle.StatusUpdate += DetourUpdater;
                        vehicle.Color = ColorPeeker.PeekColor(vehicleCount++);
                    }
                    Thread.Sleep(5);
                }
                catch (InvalidOperationException)
                {
                    continue;
                }
            }
        }

        private bool ShouldAppoint()
        {
            switch (appointMode)
            {
                case AppointMode.WhenHasSpace:
                    return (!plant.IsOccupied(startLocation));
                case AppointMode.WhenPlantCleared:
                    return (plant.Vehicles.Count == 0);
                default:
                    return false;
            }
        }

        public Order(Plant plant)
        {
            vehicleCount = 0;
            orderCount = 0;
            startLocation = new Vector2d(10, 10);
            this.plant = plant;
            orders = new Queue<IList<Task>>();
            thread = new Thread(new ThreadStart(GenCar));
            appointMode = AppointMode.WhenHasSpace;
        }

        private static void SortingOrder(IList<Task> order, OrderRule rule)
        {
            return;
        }

        public void AddOrder(FileInfo file, OrderRule rule, Func<Plant, IList<Task>, IList<Task> > router)
        {
            if (!file.Exists)
                throw new FileNotFoundException("File " + file.Name + " not found");
            StreamReader reader = new StreamReader(file.OpenRead());
            List<Task> temp = new List<Task>();
            while (!reader.EndOfStream)
            {
                string[] fields = reader.ReadLine().Trim().Split(',');
                temp.Add(new Task(new Vector2d(Double.Parse(fields[0]), Double.Parse(fields[1])), 0, Int32.Parse(fields[2])));
                plant.Tasks.Add(new Repo(new Vector2d(Double.Parse(fields[0]), Double.Parse(fields[1])), ColorPeeker.PeekColor(orderCount)));
            }
            Order.SortingOrder(temp, rule);
            orders.Enqueue(router(plant, temp));
            ++orderCount;
        }

        public void AddOrders(IList<FileInfo> files, OrderRule rule, Func<Plant, IList<Task>, IList<Task> > router)
        {
            foreach (var file in files)
                AddOrder(file, rule, router);
        }

        private void RemoveSuspendUpdater(object sender, EventArgs e)
        {
            Vehicle vehicle = (Vehicle)sender;
            switch (vehicle.State)
            {
                //case VehicleState.Moving:
                //    vehicle.Color = Color.Sienna;
                //    break;
                //case VehicleState.Working:
                //    vehicle.Color = Color.Orange;
                //    break;
                //case VehicleState.Waiting:
                //    vehicle.Color = Color.Red;
                //    break;
                case VehicleState.Suspend:
                    //vehicle.Color = Color.Purple;
                    vehicle.Fadeout();
                    break;
            }
        }

        private void DetourUpdater(object sender, EventArgs e)
        {
            Vehicle vehicle = (Vehicle)sender;
            if (vehicle.State == VehicleState.Waiting)
            {
                Task[] detour = new Task[1];
                double dx = 0, dy = 0;
                if (true || vehicle.Location.Y != 10 && vehicle.Location.Y != 90)
                    dx = (vehicle.CurrentTask.Value.Target.Y > vehicle.Location.Y) ? vehicle.Velocity : -vehicle.Velocity;
                else
                    dy = (vehicle.CurrentTask.Value.Target.X > vehicle.Location.X) ? vehicle.Velocity : -vehicle.Velocity;
                int waiting = plant.GetWaitingTime(vehicle);
                if (true || waiting >= 10/*5 * 3 + (int)(Math.Ceiling(40.0 / vehicle.Velocity))*/)
                {
                    detour[0] = new Task(vehicle.Location + new Vector2d(dx, dy));
                    vehicle.InsertTasks(detour);
                }
            }
        }
    }
}
