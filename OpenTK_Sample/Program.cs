using System;
using System.IO;
using System.Threading.Tasks;
using OpenTK;

namespace OpenTK_Sample
{
    class Program
    {
        public static void Main(string[] args)
        {
            // Prepare plant data
            Plant plant = new Plant();
            for (int x = 10; x <= 190; x += 60)
                plant.Paths.Add(new Path(x, 10, x, 90, 1));
            plant.Paths.Add(new Path(10, 10, 190, 10, 1));
            plant.Paths.Add(new Path(10, 90, 190, 90, 1));

            // Read tasks
            FileInfo tasksFile = new FileInfo("./Tasks.csv");
            StreamReader reader = new StreamReader(tasksFile.OpenRead());
            while (!reader.EndOfStream)
            {
                string[] line = reader.ReadLine().Split(',');
                if (line.Length == 2)
                    plant.Tasks.Add(new Vector2d(Double.Parse(line[0]), Double.Parse(line[1])));
            }

            Visualize display = new Visualize(plant, 400, 200);

            ControlPanel panel = new ControlPanel(plant);
            panel.Show();
            display.Run();
        }
    }
}
