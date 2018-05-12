using System;
using System.Collections.Generic;
using OpenTK;

namespace OpenTK_Sample.RoutingStrategies
{
    class Midpoint
    {
        public static IList<Task> FindRoute(Plant plant, IList<Task> jobs)
        {
            double midpoint = (plant.MaxY + plant.MinY) * 0.5;
            List<double> X = new List<double>();
            foreach (var path in plant.Paths)
            {
                if (path.V1.X == path.V2.X)
                    X.Add(path.V1.X);
            }
            // upper half
            X.Sort();
            List<Task> tasks = new List<Task>();
            foreach (var x in X)
            {
                List<Task> repos = new List<Task>();
                foreach (var job in jobs)
                {
                    if (job.Target.X == x && job.Target.Y < midpoint)
                        repos.Add(job);
                }
                if (x != plant.MinX)
                    tasks.Add(new Task(new Vector2d(x, plant.MinY), 0, (repos.Count == 0 && x != plant.MaxX) ? 0 : 5));
                repos.Sort(delegate (Task t1, Task t2)
                {
                    if (t1.Target.Y < t2.Target.Y)
                        return -1;
                    else if (t1.Target.Y > t2.Target.Y)
                        return 1;
                    else
                        return 0;
                });
                foreach (var repo in repos)
                    tasks.Add(repo);
                if (repos.Count > 0 && x != plant.MaxX)
                    tasks.Add(new Task(new Vector2d(x, plant.MinY), 0, (x == plant.MaxX) ? 0 : 5));
            }
            // lower half
            X.Sort(delegate (double n1, double n2) {
                if (n1 < n2)
                    return 1;
                else if (n1 > n2)
                    return -1;
                else
                    return 0;
            });
            foreach (var x in X)
            {
                List<Task> repos = new List<Task>();
                foreach (var job in jobs)
                {
                    if (job.Target.X == x && job.Target.Y >= midpoint)
                        repos.Add(job);
                }
                if (x != plant.MaxX)
                    tasks.Add(new Task(new Vector2d(x, plant.MaxY), 0, (repos.Count == 0) ? 0 : 5));
                repos.Sort(delegate (Task t1, Task t2)
                {
                    if (t1.Target.Y < t2.Target.Y)
                        return -1;
                    else if (t1.Target.Y > t2.Target.Y)
                        return 1;
                    else
                        return 0;
                });
                foreach (var repo in repos)
                    tasks.Add(repo);
                if (repos.Count > 0 || x == plant.MaxX)
                    tasks.Add(new Task(new Vector2d(x, plant.MaxY), 0, (x == plant.MinX)?0:5));
            }
            return tasks;
        }
    }
}
