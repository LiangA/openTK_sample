using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.IO;

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
    class Order
    {
        private static IList<Task> tasks = new List<Task>();
        
        private static IList<List<Task>> tasksForPlant = new List<List<Task>>();

        public IList<List<Task>> TasksForPlant { get { return tasksForPlant; } }

        public static void FromFile(string fileName, string listingRule = null)
        {
            tasks.Clear();

            FileInfo file = new FileInfo(fileName);
            if (!file.Exists)
                throw new FileNotFoundException("File '" + fileName + "' is not exists.");

            StreamReader reader = new StreamReader(file.OpenRead());
            switch (listingRule)
            {
                default:
                    orderIsList(reader);
                    break;
                case "orderRule1":  // you can add more rule function and add it into switch
                    orderRule1(reader);
                    break;

            }
            List<Task> temp = new List<Task>(tasks);
            tasksForPlant.Add(temp);
            reader.Close();
        }

        public void FromFiles(List<string> fileName, string listingRule = null)
        {
            foreach (var i in fileName)
                FromFile(i, listingRule);
        }

        private static void orderIsList(StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                string[] line = reader.ReadLine().Split(',');
                if (line.Length == 3)
                    tasks.Add(new Task(new Vector2d(Double.Parse(line[0]), Double.Parse(line[1])), Int32.Parse(line[2])));
                else
                    Console.WriteLine("One or more invalid order(s) was input.");
            }
        }

        private static void orderRule1(StreamReader reader)
        {

        }
    }
}
