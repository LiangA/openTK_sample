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
        private Thread thread;
        private Plant plant;
        private Vector2d startLocation;
        private AppointMode appointMode;
        private double velocity;

        private int vehicleCount;
        private Queue<IList<Task>> orders;
        private RoutingStrategies.Detourer detourer;

        public Thread Agent { get => thread; set => thread = value; }
        public AppointMode AppointMode { get => appointMode; set => appointMode = value; }
        public double Velocity { get => velocity; set => velocity = value; }

        public Vehicle.StatusUpdateHandler Detour;

        private void GenCar()
        {
            while (true)
            {
                try
                {
                    if (ShouldAppoint() && orders.Count > 0) {
                        Vehicle vehicle = new Vehicle(plant, startLocation);
                        plant.Vehicles.Add(vehicle);
                        vehicle.Velocity = Velocity;
                        foreach (var task in orders.Peek())
                        {
                            if (task.Target.Y != plant.MinY && task.Target.Y != plant.MaxY)
                                plant.Tasks.Add(new Repo(task.Target, ColorPeeker.PeekColor(vehicleCount)));
                        }
                        vehicle.SetTasks(orders.Dequeue());
                        vehicle.StatusUpdate += RemoveSuspendUpdater;
                        vehicle.StatusUpdate += detourer.Detour;
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
                    return (plant.Vehicles.Count < 8 && !plant.IsOccupied(startLocation));
                case AppointMode.WhenPlantCleared:
                    return (plant.Vehicles.Count == 0);
                default:
                    return false;
            }
        }

        public Order(Plant plant)
        {
            vehicleCount = 0;
            startLocation = new Vector2d(10, 10);
            this.plant = plant;
            detourer = new RoutingStrategies.Detourer(plant);
            Detour = new Vehicle.StatusUpdateHandler(detourer.Detour);
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
                if (fields.Length <= 1)
                    continue;
                temp.Add(new Task(new Vector2d(Double.Parse(fields[0]), Double.Parse(fields[1])), 0, Int32.Parse(fields[2])));
            }
            Order.SortingOrder(temp, rule);
            orders.Enqueue(router(plant, temp));
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
                case VehicleState.Suspend:
                    //vehicle.Color = Color.Purple;
                    vehicle.Fadeout();
                    break;
            }
        }
    }
}
