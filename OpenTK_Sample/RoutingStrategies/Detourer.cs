using System;
using OpenTK;
using System.Drawing;

namespace OpenTK_Sample.RoutingStrategies
{
    class Detourer
    {
        Plant plant;
        public Detourer(Plant plant)
        {
            this.plant = plant;
        }

        public void Detour(object sender, EventArgs args)
        {
            Vehicle vehicle = (Vehicle)sender;
            if (vehicle.State != VehicleState.Waiting)
                return;
            Vector2d diff = vehicle.CurrentTask.Value.Target - vehicle.Location;
            if (diff.X != 0 || diff.Length < 20)
                return ;
            diff.Normalize();
            double dx = 10, dy = diff.Y * 10;
            foreach (var car in plant.Vehicles)
            {
                if (car.Location == vehicle.Location + new Vector2d(0, dy))
                {
                    if (car.Halt <= Math.Ceiling(20 + (50/vehicle.Velocity)) )
                        return;
                    else
                        break;
                }
            }

            Task[] tasks = new Task[4];
            tasks[0] = new Task(vehicle.Location + new Vector2d(dx, 0), 5, 5);
            tasks[1] = new Task(vehicle.Location + new Vector2d(dx, dy), 0, 0);
            tasks[2] = new Task(vehicle.Location + new Vector2d(dx, dy*2), 0, 5);
            tasks[3] = new Task(vehicle.Location + new Vector2d(0, dy*2), 0, 5);
            bool occupied = false;
            foreach (var task in tasks)
            {
                if (plant.IsOccupied(task.Target, vehicle.Id))
                {
                    occupied = true;
                    break;
                }
            }
            if (occupied)
                return;
            foreach (var task in tasks)
            {
                Vehicle booking = new Vehicle(plant, task.Target, vehicle.Id);
                booking.Color = vehicle.Color;
                plant.Vehicles.Add(booking);
            }
            vehicle.InsertTasks(tasks);
        }
    }
}
