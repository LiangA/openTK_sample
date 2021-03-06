﻿using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;

namespace OpenTK_Sample
{
    class ControlPanel: Form
    {
        Plant plant;
        Order order;

        Button[] buttons;

        public ControlPanel(Plant plant, Order order = null)
        {
            this.plant = plant;
            this.order = order;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            buttons = new Button[6];
            for (int i = 0; i < buttons.Length; ++i)
            {
                buttons[i] = new Button();
                buttons[i].Location = new Point(10, 30 * i + 10);
                buttons[i].Size = new Size(110, 20);
                buttons[i].Parent = this;
            }

            buttons[0].Text = "S-turn";
            buttons[0].Click += STurn;

            buttons[1].Text = "Return";
            buttons[1].Click += Return;

            buttons[2].Text = "Mid-point";
            buttons[2].Click += MidPoint;

            buttons[3].Text = "Largest gap";
            buttons[3].Click += LargestGap;

            buttons[4].Text = "Reset";
            buttons[4].Click += Reset;

            buttons[5].Text = "Cancel detour";
            buttons[5].Click += RemoveDetour;

            this.ClientSize = new Size(130, 30 * buttons.Length + 10);
        }

        private void RemoveDetour(object sender, EventArgs args)
        {
            foreach(var vehicle in plant.Vehicles)
            {
                vehicle.StatusUpdate -= order.Detour;
            }
        }

        private Vehicle GenerateVehicle()
        {
            Vehicle vehicle = new Vehicle(plant, new Vector2d(10, 10));
            vehicle.Velocity = 2.5;
            vehicle.Color = Color.Sienna;
            return vehicle;
        }

        private void STurn(object sender, EventArgs e)
        {
            Vehicle vehicle = GenerateVehicle();
            IList<Task> tasks = new List<Task>();
            for (double x = 10; x <= 190; x += 60)
            {
                List<Repo> jobs = new List<Repo>();
                //load all jobs
                foreach (var job in plant.Tasks)
                {
                    if (job.X == x)
                        jobs.Add(job);
                }
                //sort the odd isles except the last isle
                if (Int32.Parse((Math.Floor(x / 60)).ToString()) % 2 == 0 && x < 190)
                {
                    jobs.Sort(delegate (Repo t1, Repo t2) {   //jobs.Sort(an integer), if the integer > 0 means t1 > t2, = 0 means t1 = t2, < 0 means t1 < t2
                        if (t1.Y < t2.Y)
                            return -1;
                        else if (t1.Y > t2.Y)
                            return 1;
                        else
                            return 0;
                    });
                    foreach (var job in jobs)
                    {
                        tasks.Add(new Task(job.Location, 0, 20));
                    }
                    tasks.Add(new Task(new Vector2d(x, 90), 0, 5));
                    tasks.Add(new Task(new Vector2d(x + 60, 90), 0, 5));
                }

                //sort the even isles except the last isle
                if (Int32.Parse((Math.Floor(x / 60)).ToString()) % 2 == 1 && x < 190)
                {
                    jobs.Sort(delegate (Repo t1, Repo t2) {
                        if (t1.Y < t2.Y)
                            return 1;
                        else if (t1.Y > t2.Y)
                            return -1;
                        else
                            return 0;
                    });
                    foreach (var job in jobs)
                    {
                        tasks.Add(new Task(job.Location, 0, 20));
                    }
                    tasks.Add(new Task(new Vector2d(x, 10), 0, 5));
                    tasks.Add(new Task(new Vector2d(x + 60, 10), 0, 5));
                }

                //sort the last isle
                if (x == 190)
                {
                    //sort the odd isles
                    if (Int32.Parse((Math.Floor(x / 60)).ToString()) % 2 == 0)
                    {
                        jobs.Sort(delegate (Repo t1, Repo t2) {
                            if (t1.Y < t2.Y)
                                return -1;
                            else if (t1.Y > t2.Y)
                                return 1;
                            else
                                return 0;
                        });
                        foreach (var job in jobs)
                        {
                            tasks.Add(new Task(job.Location, 0, 20));
                        }
                        tasks.Add(new Task(new Vector2d(x, 90), 0, 5));
                    }

                    //sort the even isles
                    if (Int32.Parse((Math.Floor(x / 60)).ToString()) % 2 == 1)
                    {
                        jobs.Sort(delegate (Repo t1, Repo t2) {
                            if (t1.Y < t2.Y)
                                return 1;
                            else if (t1.Y > t2.Y)
                                return -1;
                            else
                                return 0;
                        });
                        foreach (var job in jobs)
                        {
                            tasks.Add(new Task(job.Location, 0, 20));
                        }
                        tasks.Add(new Task(new Vector2d(x, 10), 0, 5));
                    }
                }
            }
            vehicle.SetTasks(tasks);
            vehicle.StatusUpdate += RemoveSuspendUpdater;
            vehicle.StatusUpdate += DetourUpdater;
            plant.Vehicles.Add(vehicle);
        }

        private void Return(object sender, EventArgs e)
        {
            Vehicle vehicle = GenerateVehicle();
            IList<Task> tasks = new List<Task>();
            for (double x = 10; x <= 190; x += 60)
            {
                List<Vector2d> jobs = new List<Vector2d>();
                foreach(var job in plant.Tasks)
                {
                    if (job.X == x)
                        jobs.Add(job.Location);
                }
                jobs.Sort(delegate (Vector2d t1, Vector2d t2) {
                    if (t1.Y < t2.Y)
                        return -1;
                    else if (t1.Y > t2.Y)
                        return 1;
                    else
                        return 0;
                });
                foreach (var job in jobs)
                {
                    tasks.Add(new Task(job, 0, 20));
                }
                if (jobs.Count > 0)
                    tasks.Add(new Task(new Vector2d(x, 10), 0, 5));
                if (x < 190)
                    tasks.Add(new Task(new Vector2d(x + 60, 10)));
            }
            vehicle.SetTasks(tasks);
            vehicle.StatusUpdate += RemoveSuspendUpdater;
            vehicle.StatusUpdate += DetourUpdater;
            plant.Vehicles.Add(vehicle);
        }

        private void MidPoint(object sender, EventArgs e)
        {
            Vehicle vehicle = GenerateVehicle();
            IList<Task> tasks = new List<Task>();
            for (double x = 10; x < 190; x += 60)
            {
                List<Vector2d> jobs = new List<Vector2d>();
                foreach (var job in plant.Tasks)
                {
                    if (job.X == x && job.Y <= 50)
                        jobs.Add(job.Location);
                }
                jobs.Sort(delegate (Vector2d t1, Vector2d t2) {
                    if (t1.Y < t2.Y)
                        return -1;
                    else if (t1.Y > t2.Y)
                        return 1;
                    else
                        return 0;
                });
                foreach (var job in jobs)
                {
                    tasks.Add(new Task(job, 0, 20));
                }
                if (jobs.Count > 0)
                    tasks.Add(new Task(new Vector2d(x, 10), 0, 5));
                if (x < 190)
                    tasks.Add(new Task(new Vector2d(x + 60, 10)));
            }

            // last column
            {
                double x = 190;
                List<Vector2d> jobs = new List<Vector2d>();
                foreach (var job in plant.Tasks)
                {
                    if (job.X == x)
                        jobs.Add(job.Location);
                }
                jobs.Sort(delegate (Vector2d t1, Vector2d t2) {
                    if (t1.Y < t2.Y)
                        return -1;
                    else if (t1.Y > t2.Y)
                        return 1;
                    else
                        return 0;
                });
                foreach (var job in jobs)
                {
                    tasks.Add(new Task(job, 0, 20));
                }
                tasks.Add(new Task(new Vector2d(x, 90), 0, 5));
            }
            tasks.Add(new Task(new Vector2d(130, 90), 0, 5));

            for (double x = 130; x >= 10; x -= 60)
            {
                List<Vector2d> jobs = new List<Vector2d>();
                foreach (var job in plant.Tasks)
                {
                    if (job.X == x && job.Y > 50)
                        jobs.Add(job.Location);
                }
                jobs.Sort(delegate (Vector2d t1, Vector2d t2) {
                    if (t1.Y < t2.Y)
                        return 1;
                    else if (t1.Y > t2.Y)
                        return -1;
                    else
                        return 0;
                });
                foreach (var job in jobs)
                {
                    tasks.Add(new Task(job, 0, 20));
                }
                if (jobs.Count > 0)
                    tasks.Add(new Task(new Vector2d(x, 90), 0, 5));
                if (x > 10)
                    tasks.Add(new Task(new Vector2d(x - 60, 90)));
            }
            vehicle.SetTasks(tasks);
            vehicle.StatusUpdate += RemoveSuspendUpdater;
            vehicle.StatusUpdate += DetourUpdater;
            plant.Vehicles.Add(vehicle);
        }

        private void LargestGap(object sender, EventArgs e)
        {
            Vehicle vehicle = GenerateVehicle();
            IList<Task> tasks = new List<Task>();
            IDictionary<double, double> returnPoints = new Dictionary<double, double>();
            // Find return points
            for (double x = 10; x <= 190; x += 60)
            {
                List<double> endPoints = new List<double>();
                endPoints.Add(10);
                endPoints.Add(90);
                foreach (var job in plant.Tasks)
                {
                    if (job.X == x)
                        endPoints.Add(job.Y);
                }
                endPoints.Sort();
                int min = 1;
                for (int i = 2; i < endPoints.Count; ++i)
                {
                    if (endPoints[i] - endPoints[i - 1] > endPoints[min] - endPoints[min - 1])
                        min = i;
                }
                returnPoints[x] = (endPoints[min] + endPoints[min - 1]) * 0.5;
            }
            // Upper half
            for (double x = 10; x < 190; x += 60)
            {
                List<Vector2d> jobs = new List<Vector2d>();
                foreach (var job in plant.Tasks)
                {
                    if (job.X == x && job.Y <= returnPoints[x])
                        jobs.Add(job.Location);
                }
                jobs.Sort(delegate (Vector2d t1, Vector2d t2) {
                    if (t1.Y < t2.Y)
                        return -1;
                    else if (t1.Y > t2.Y)
                        return 1;
                    else
                        return 0;
                });
                foreach (var job in jobs)
                {
                    tasks.Add(new Task(job, 0, 20));
                }
                if (jobs.Count > 0)
                    tasks.Add(new Task(new Vector2d(x, 10), 0, 5));
                if (x < 190)
                    tasks.Add(new Task(new Vector2d(x + 60, 10)));
            }

            // last column
            {
                double x = 190;
                List<Vector2d> jobs = new List<Vector2d>();
                foreach (var job in plant.Tasks)
                {
                    if (job.X == x)
                        jobs.Add(job.Location);
                }
                jobs.Sort(delegate (Vector2d t1, Vector2d t2) {
                    if (t1.Y < t2.Y)
                        return -1;
                    else if (t1.Y > t2.Y)
                        return 1;
                    else
                        return 0;
                });
                foreach (var job in jobs)
                {
                    tasks.Add(new Task(job, 0, 20));
                }
                tasks.Add(new Task(new Vector2d(x, 90), 0, 5));
            }
            tasks.Add(new Task(new Vector2d(130, 90), 0, 5));

            // Lower half
            for (double x = 130; x >= 10; x -= 60)
            {
                List<Vector2d> jobs = new List<Vector2d>();
                foreach (var job in plant.Tasks)
                {
                    if (job.X == x && job.Y > returnPoints[x])
                        jobs.Add(job.Location);
                }
                jobs.Sort(delegate (Vector2d t1, Vector2d t2) {
                    if (t1.Y < t2.Y)
                        return 1;
                    else if (t1.Y > t2.Y)
                        return -1;
                    else
                        return 0;
                });
                foreach (var job in jobs)
                {
                    tasks.Add(new Task(job, 0, 20));
                }
                if (jobs.Count > 0)
                    tasks.Add(new Task(new Vector2d(x, 90), 0, 5));
                if (x > 10)
                    tasks.Add(new Task(new Vector2d(x - 60, 90)));
            }
            vehicle.SetTasks(tasks);
            vehicle.StatusUpdate += RemoveSuspendUpdater;
            vehicle.StatusUpdate += DetourUpdater;
            plant.Vehicles.Add(vehicle);
        }

        private void AddCyclicVehicle(object sender, EventArgs e)
        {
            Vehicle vehicle = new Vehicle(plant, new Vector2d(10, 10));
            vehicle.Velocity = 2.5;
            vehicle.Color = Color.Green;
            vehicle.StatusUpdate += CyclicUpdater;
            plant.Vehicles.Add(vehicle);
        }

        private void Reset(object sender, EventArgs e)
        {
            plant.Vehicles.Clear();
        }

        private void RemoveSuspendUpdater(object sender, EventArgs e)
        {
            Vehicle vehicle = (Vehicle)sender;
            switch (vehicle.State)
            {
                case VehicleState.Moving:
                    vehicle.Color = Color.Sienna;
                    break;
                case VehicleState.Working:
                    vehicle.Color = Color.Orange;
                    break;
                case VehicleState.Waiting:
                    vehicle.Color = Color.Red;
                    break;
                case VehicleState.Suspend:
                    vehicle.Color = Color.Purple;
                    vehicle.Fadeout();
                    break;
            }
        }

        private void DetourUpdater(object sender, EventArgs e)
        {
            Vehicle vehicle = (Vehicle)sender;
            if (vehicle.State == VehicleState.Waiting)
            {
                Task[] detour = new Task[1];
                double dx = 0, dy = 0;
                if (true || vehicle.Location.Y != 10 && vehicle.Location.Y != 90)
                    dx = (vehicle.CurrentTask.Value.Target.Y > vehicle.Location.Y) ? vehicle.Velocity : -vehicle.Velocity;
                else
                    dy = (vehicle.CurrentTask.Value.Target.X > vehicle.Location.X) ? vehicle.Velocity : -vehicle.Velocity;
                int waiting = plant.GetWaitingTime(vehicle);
                if (true || waiting >= 10/*5 * 3 + (int)(Math.Ceiling(40.0 / vehicle.Velocity))*/)
                {
                    detour[0] = new Task(vehicle.Location + new Vector2d(dx, dy));
                    vehicle.InsertTasks(detour);
                }
            }
        }

        private void InfoUpdater(object sender, EventArgs e)
        {
            Vehicle vehicle = (Vehicle)sender;
            Console.WriteLine(vehicle.State);
            try
            {
                Console.WriteLine(vehicle.CurrentTask.Value.HaltAfter);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("the vehicle was gone.");
            }
        }

        private void CyclicUpdater(object sender, EventArgs e)
        {
            Vehicle vehicle = (Vehicle)sender;
            if (vehicle.State == VehicleState.Suspend)
            {
                Task[] tasks = new Task[8];
                for (int i = 0; i < 7; i += 2)
                    tasks[i] = new Task(new Vector2d(10 + 30 * i, (i % 4 == 0) ? 90 : 10), 0, 20);
                for (int i = 1; i < 7; i += 2)
                    tasks[i] = new Task(new Vector2d(40 + 30 * i, (i % 4 == 1) ? 90 : 10), 0, 20);
                tasks[7] = new Task(new Vector2d(10, 10), 0, 20);
                vehicle.SetTasks(tasks);
            }
        }
    }
}
