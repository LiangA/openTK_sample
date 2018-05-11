using System;
using OpenTK;

namespace OpenTK_Sample
{
    partial class Plant
    {
        public bool IsOccupied(Vector2d loc)
        {
            foreach(var car in vehicles)
            {
                if ((car.Location - loc).Length < 10)
                    return true;
            }
            return false;
        }

        public bool CanMove(Vehicle vehicle, Vector2d move)
        {
            move += vehicle.Location;
            foreach (var car in vehicles)
            {
                if (Object.ReferenceEquals(car, vehicle))
                    continue;
                if ((car.Location - move).Length <= 10)
                    return false;
            }
            return true;
        }

        public int GetWaitingTime(Vehicle vehicle)
        {
            int maxWait = -1;
            var diff = vehicle.CurrentTask.Value.Target - vehicle.Location;
            if (diff.Length > vehicle.Velocity)
            {
                diff.Normalize();
                diff *= vehicle.Velocity;
            }
            Vector2d location = vehicle.Location + diff;
            foreach (var car in vehicles)
            {
                if (Object.ReferenceEquals(car, vehicle))
                    continue;
                if ((car.Location - vehicle.CurrentTask.Value.Target).Length < 10)
                    return 0;
                if (car.State == VehicleState.Waiting && maxWait < car.Wait)
                    maxWait = car.Wait;
                if (car.State == VehicleState.Working && maxWait < car.Halt)
                    maxWait = car.Halt;
            }
            return maxWait;
        }
    }
}
