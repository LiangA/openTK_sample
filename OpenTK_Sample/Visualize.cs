using System;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using System.Drawing;

namespace OpenTK_Sample
{
    class Visualize
    {
        GameWindow window;
        GL2DUtil helper;
        uint frameCount;
        long frameTick;

        Plant plant;

        public Visualize(Plant plant, GameWindow wnd)
        {
            this.plant = plant;
            window = wnd;
            window.Resize += WindowResize;
        }

        public Visualize(Plant plant, int width, int height)
        {
            this.plant = plant;
            window = new GameWindow(width, height);
            window.Resize += WindowResize;
        }

        private void WindowResize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, window.Width, window.Height);
            helper.Client = window.ClientSize;
        }

        public void Run()
        {
            window.Load += Preload;
            window.RenderFrame += DrawScene;
            window.Run();
        }

        private void DrawScene(object sender, FrameEventArgs e)
        {
            // Clear Screen
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Draw plant
            foreach (var path in plant.Paths)
                helper.DrawPolygon(path.Rectangle, Color.LightGray);

            // Draw tasks
            foreach (var job in plant.Tasks)
                helper.DrawCircle(job.Location, 5, job.Color, 4);

            // Draw vehicles
            foreach (var car in plant.Vehicles)
                helper.DrawCircle(car.Location, 5, car.Color);

            // Flush the drawed data to current screen
            window.SwapBuffers();

            // Calculate FPS
            ++frameCount;
            if (frameCount == 30)
            {
                long nextTick = DateTime.Now.Ticks;
                double fps = 300000000.0 / (nextTick - frameTick);
                frameCount = 0;
                frameTick = nextTick;
                window.Title = "FPS: " + fps.ToString(); 
            }
        }

        void Preload(object sender, EventArgs e)
        {
            // Setup background color
            GL.ClearColor(Color.Black);
            // Prepare FPS calculation
            frameCount = 0;
            frameTick = DateTime.Now.Ticks;

            // Prepare OpenGL Helper
            helper = new GL2DUtil(window.ClientSize, plant.Size, true);
            helper.MarginRight = 20;
            helper.MarginBottom = 20;
        }
    }
}
