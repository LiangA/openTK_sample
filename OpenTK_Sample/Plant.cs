﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using OpenTK;

namespace OpenTK_Sample
{
    struct Path
    {
        public Path(Vector2d vs, Vector2d vt, double width = 1.0)
        {
            v1 = vs;
            v2 = vt;
            this.width = width;
            normalize();
        }

        public Path(double x1, double y1, double x2, double y2, double width = 1.0)
        {
            v1 = new Vector2d(x1, y1);
            v2 = new Vector2d(x2, y2);
            this.width = width;
            normalize();
        }

        private double width;
        private Vector2d v1;
        private Vector2d v2;

        public double Width { get => width; set => width = value; }
        public Vector2d V1 { get => v1; set { v1 = value; normalize(); } }
        public Vector2d V2 { get => v2; set { v2 = value; normalize(); } }

        private void normalize()
        {
            if (v1.Y > v2.Y || (v1.Y == v2.Y && v1.X > v2.X))
            {
                Vector2d temp = v1;
                v1 = v2;
                v2 = temp;
            }
        }

        public Vector2d[] Rectangle
        {
            get
            {
                Vector2d vec = v2 - v1;
                Vector2d ext = vec * (width / vec.Length);
                Vector2d v1e = v1 - 0.5 * ext, v2e = v2 + 0.5 * ext;
                Vector2d ext2 = new Vector2d(ext.Y * -0.5, ext.X * 0.5);
                Vector2d[] result = new Vector2d[4];
                result[0] = v1e + ext2;
                result[1] = v1e - ext2;
                result[2] = v2e - ext2;
                result[3] = v2e + ext2;
                return result;
            }
        }
    }

    class Plant
    {
        public Plant()
        {
            paths = new List<Path>();
            vehicles = new List<Vehicle>();
            tasks = new List<Vector2d>();
        }

        public static Plant FromFile(FileInfo file)
        {
            Plant result = new Plant();
            // TO DO: Add file reader and prepare data for plant
            StreamReader reader = new StreamReader(file.OpenRead());
            reader.Close();
            return result;
        }

        public static Plant FromFile(string fileName)
        {
            FileInfo file = new FileInfo(fileName);
            if (!file.Exists)
                throw new FileNotFoundException("File '" + fileName + "' is not exists.");
            return Plant.FromFile(file);
        }

        private IList<Path> paths;
        private IList<Vehicle> vehicles;
        private IList<Vector2d> tasks;

        public IList<Vehicle> Vehicles { get => vehicles; set => vehicles = value; }
        public IList<Path> Paths { get => paths; set => paths = value; }
        public IList<Vector2d> Tasks { get => tasks; set => tasks = value; }

        public Size Size
        {
            get
            {
                double x = 0, y = 0;
                foreach (Path path in paths)
                {
                    if (x < path.V1.X + 0.5 * path.Width)
                        x = path.V1.X + 0.5 * path.Width;
                    if (x < path.V2.X + 0.5 * path.Width)
                        x = path.V2.X + 0.5 * path.Width;
                    if (y < path.V1.Y + 0.5 * path.Width)
                        y = path.V1.Y + 0.5 * path.Width;
                    if (y < path.V2.Y + 0.5 * path.Width)
                        y = path.V2.Y + 0.5 * path.Width;
                }
                return new Size((int)Math.Ceiling(x), (int)Math.Ceiling(y));
            }
        }

        public bool CanMove(Vehicle vehicle, Vector2d move)
        {
            move += vehicle.Location;
            foreach(var car in vehicles)
            {
                if (Object.ReferenceEquals(car, vehicle))
                    continue;
                if ((car.Location - move).Length <= 10)
                    return false;
            }
            return true;
        }
    }
}
