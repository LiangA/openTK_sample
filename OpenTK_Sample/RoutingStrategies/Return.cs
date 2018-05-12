using System;
using System.Collections.Generic;
using OpenTK;

namespace OpenTK_Sample.RoutingStrategies
{
    class Return
    {
        public static IList<Task> FindRoute(Plant plant, IList<Task> jobs)
        {
            List<double> X = new List<double>();
            foreach(var path in plant.Paths)
            {
                if (path.V1.X == path.V2.X)
                    X.Add(path.V1.X);
            }
            X.Sort();
            List<Task> tasks = new List<Task>();
            foreach (var x in X)
            {
                List<Task> repos = new List<Task>();
                foreach (var job in jobs)
                {
                    if (job.Target.X == x)
                        repos.Add(job);
                }
                if (x != plant.MinX)
                    tasks.Add(new Task(new Vector2d(x, plant.MinY), 0, (repos.Count == 0)?0:5));
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
                if (repos.Count > 0)
                    tasks.Add(new Task(new Vector2d(x, plant.MinY), 0, (x == plant.MaxX)?0:5));
            }
            return tasks;
        }
    }
}
