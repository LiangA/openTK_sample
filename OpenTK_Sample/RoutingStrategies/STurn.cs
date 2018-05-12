using System;
using System.Collections.Generic;
using OpenTK;

namespace OpenTK_Sample.RoutingStrategies
{
    class STurn
    {
        public static IList<Task> FindRoute(Plant plant, IList<Task> jobs)
        {
            List<Double> entries = new List<Double>();
            foreach (var path in plant.Paths)
            {
                if (path.V1.X == path.V2.X)
                    entries.Add(path.V1.X);
            }
            entries.Sort();
            List<Task> tasks = new List<Task>();
            bool dir = true;
            foreach(var x in entries)
            {
                List<Task> repos = new List<Task>();
                repos.Add(new Task(new Vector2d(x, plant.MinY), 0, 5));
                repos.Add(new Task(new Vector2d(x, plant.MaxY), 0, 5));
                foreach (var j in jobs)
                {
                    if (j.Target.X == x)
                        repos.Add(j);
                }
                if (dir)
                {
                    repos.Sort(delegate (Task t1, Task t2) {
                        if (t1.Target.Y < t2.Target.Y)
                            return -1;
                        else if (t1.Target.Y > t2.Target.Y)
                            return 1;
                        else
                            return 0;
                    });
                }
                else
                {
                    repos.Sort(delegate (Task t1, Task t2) {
                        if (t1.Target.Y < t2.Target.Y)
                            return 1;
                        else if (t1.Target.Y > t2.Target.Y)
                            return -1;
                        else
                            return 0;
                    });
                }
                foreach (var task in repos)
                    tasks.Add(task);
                dir ^= true;    
            }
            tasks.RemoveAt(0);
            tasks.RemoveAt(tasks.Count - 1);
            tasks.Add(new Task(new Vector2d(entries[entries.Count - 1], (dir) ? plant.MinY : plant.MaxY)));
            return tasks;
        }
    }
}
