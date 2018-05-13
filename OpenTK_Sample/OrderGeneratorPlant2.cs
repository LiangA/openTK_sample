using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;


namespace OpenTK_Sample
{
    class OrderGeneratorPlant2: Random
    {
        public double NextNormal(double mean, double stdDev)
        {
            double u1 = 1.0 - this.NextDouble();
            double u2 = 1.0 - this.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + stdDev * randStdNormal;
        }

        Vector2d[] repos;

        public OrderGeneratorPlant2(Plant plant, int nRepos) : base()
        {
            List<double> X = new List<double>();
            foreach (var path in plant.Paths)
            {
                if (path.V1.X == path.V2.X)
                    X.Add(path.V1.X);
            }
            int columnSize = (nRepos) / X.Count;
            repos = new Vector2d[columnSize * X.Count];
            int idx = 0;
            foreach (var x in X)
            {
                for (int i = 1; i <= columnSize; ++i)
                    repos[idx++] = new Vector2d(x, ((plant.MaxY - plant.MinY) / (columnSize + 1.0)) * i + plant.MinY);
            }
        }

        public void GenerateOrder(FileInfo file)
        {
            int n = (int)Math.Round(NextNormal(7.5, 2));
            if (n < 5)
                n = 5;
            if (n > 11)
                n = 10;
            bool[] choosed = new bool[repos.Length];
            for (int i = 0; i < choosed.Length; ++i)
                choosed[i] = false;
            for (int i = 0; i < n; ++i)
            {
                int target = Next(choosed.Length - i);
                for (int j = 0, k = 0; k <= target; ++j)
                {
                    if (!choosed[j] && k++ == target)
                        choosed[j] = true;
                }
            }
            StreamWriter writer = new StreamWriter(file.OpenWrite());
            string msg = "";
            for (int i = 0; i < choosed.Length; ++i)
            {
                if (choosed[i])
                {
                    if (msg.Length > 0)
                        msg += "\n";
                    msg += repos[i].X.ToString() + "," + repos[i].Y.ToString() + "," + (50).ToString();
                }
            }
            writer.Write(msg);
            writer.Close();
        }

        public static void Main(string[] args)
        {
            Plant plant = Plant.FromFile(@"./Plant2/layout.csv");
            OrderGeneratorPlant2 gen = new OrderGeneratorPlant2(plant, 80);
            for (int i = 1; i <= 1000; ++i)
            {
                FileInfo file = new FileInfo("./Plant2/Orders/" + i.ToString("0000") + ".csv");
                gen.GenerateOrder(file);
            }
        }
    }
}
