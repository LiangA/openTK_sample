using System;
using OpenTK;

namespace OpenTK_Sample
{
    partial class Plant
    {
        public bool IsOccupied(Vector2d move, int id = 0)
        {
            foreach(var car in vehicles)
            {
                if (id == car.Id)
                {
                    if (car.Location == move)
                        car.Fadeout();
                    continue;
                }
                if ((car.Location - move).Length < 10)
                    return true;
            }
            return false;
        }

        public bool CanMove(Vehicle vehicle, Vector2d move)
        {
            move += vehicle.Location;
            return (!IsOccupied(move, vehicle.Id));
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
