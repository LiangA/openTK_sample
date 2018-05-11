﻿using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;

namespace OpenTK_Sample
{
    public enum VehicleState
    {
        Scheduling,
        Moving,
        Working,
        Suspend,
        Waiting,
        Removing
    }

    public struct Task
    {
        public Task(Vector2d tar, int prehalt = 0, int posthalt = 0)
        {
            target = tar;
            haltBefore = prehalt;
            haltAfter = posthalt;
        }

        int haltBefore, haltAfter;
        Vector2d target;

        public int HaltBefore { get => haltBefore; set => haltBefore = value; }
        public int HaltAfter { get => haltAfter; set => haltAfter = value; }
        public Vector2d Target { get => target; set => target = value; }
    }

    class Vehicle
    {
        private Vector2d? target;
        private Vector2d location;
        private Plant plant;
        private Queue<Task> tasks;
        private int haltBefore, haltAfter;
        private bool moveFailed, fadeout;

        private double velocity;
        public double Velocity { get => velocity; set => velocity = value; }
        private Color color;
        public Color Color { get => color; set => color = value; }

        public delegate void StatusUpdateHandler(object sender, EventArgs args);
        public event StatusUpdateHandler StatusUpdate;

        public Vehicle(Plant plant, Vector2d startLocation, Vector2d? target = null)
        {
            this.plant = plant;
            location = startLocation;
            this.target = target;
            haltBefore = 0;
            haltAfter = 0;
            tasks = new Queue<Task>();
            fadeout = false;
            StatusUpdate += DefaultUpdater;
        }

        public void SetTarget(Vector2d tar) => target = tar;

        public void Fadeout() => fadeout = true;

        private void FindRoute()
        {

        }

        public void SetTasks(IList<Task> Tasks)
        {
            tasks.Clear();
            foreach (var task in Tasks)
                tasks.Enqueue(task);
        }

        public void InsertTasks(IList<Task> Tasks)
        {
            var temp = tasks;
            tasks = new Queue<Task>();
            foreach (var task in Tasks)
                tasks.Enqueue(task);
            while (temp.Count > 0)
                tasks.Enqueue(temp.Dequeue());
            temp = null;
        }

        public void AddTasks(IList<Task> Tasks)
        {
            foreach (var task in Tasks)
                tasks.Enqueue(task);
        }

        public VehicleState State
        {
            get
            {
                if (fadeout)
                    return VehicleState.Removing;
                if (target.HasValue)
                    return VehicleState.Scheduling;
                if (tasks.Count > 0)
                {
                    if (haltBefore < tasks.Peek().HaltBefore || location == tasks.Peek().Target)
                        return VehicleState.Working;
                    else if (moveFailed)
                        return VehicleState.Waiting;
                    else
                        return VehicleState.Moving;
                }
                return VehicleState.Suspend;
            }
        }

        public Vector2d Location { get => location; }

        public Task? CurrentTask { get => ((tasks.Count == 0) ? (Task?)null : tasks.Peek()); }

        public void IgnoreTask() { tasks.Dequeue(); }

        public void OnStatusUpdate()
        {
            if (StatusUpdate == null)
                return;
            EventArgs args = new EventArgs();
            StatusUpdate(this, args);
        }

        protected void DefaultUpdater(object sender, EventArgs args)
        {
            if (fadeout)
            {
                if (color.A < 20)
                    plant.Vehicles.Remove(this);
                else
                    color = Color.FromArgb(color.A - 20, color);
                return;
            }
            if (target.HasValue)
                FindRoute();
            if (tasks.Count == 0)
                return;
            if (haltBefore < tasks.Peek().HaltBefore)
                ++haltBefore;
            else if (location != tasks.Peek().Target)
            {
                var diff = tasks.Peek().Target - location;
                if (diff.Length > velocity)
                {
                    diff.Normalize();
                    diff *= velocity;
                }
                if (plant.CanMove(this, diff))
                {
                    location += diff;
                    moveFailed = false;
                }
                else
                    moveFailed = true;
            }
            else if (haltAfter < tasks.Peek().HaltAfter)
                ++haltAfter;
            if (haltBefore >= tasks.Peek().HaltBefore && location == tasks.Peek().Target && haltAfter >= tasks.Peek().HaltAfter)
            {
                haltBefore = 0;
                haltAfter = 0;
                tasks.Dequeue();
            }

        }
    }
}
