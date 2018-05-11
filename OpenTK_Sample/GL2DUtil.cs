using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenTK_Sample
{
    // Use to transform the coordinate from some field to viewport for OpenGL2D
    // Assume that the left-top corner is (0, 0)
    // Right direction is x+
    // Down direction is y+
    class GL2DUtil
    {
        private bool keep;
        public bool KeepAspectRatio { get => keep; set { keep = value; update(); } }

        private Size client;
        public Size Client { get => client; set { client = value; update(); } }

        private Size field;
        public Size Field { get => field; set { field = value; update(); } }

        int[] margins;
        public int MarginTop { get => margins[0]; set { margins[0] = value; update(); } }
        public int MarginRight { get => margins[1]; set { margins[1] = value; update(); } }
        public int MarginBottom { get => margins[2]; set { margins[2] = value; update(); } }
        public int MarginLeft { get => margins[3]; set { margins[3] = value; update(); } }
        public int[] Margin { get => margins; set { margins = value; update(); } }

        private double glLeft, glTop, glWidth, glHeight;
        private void update()
        {
            glLeft = (2.0 * MarginLeft) / client.Width - 1.0;
            glTop = 1.0 - (2.0 * MarginTop) / client.Height;
            glWidth = 2.0 * (client.Width - MarginLeft - MarginRight) / client.Width;
            glHeight = 2.0 * (client.Height - MarginTop - MarginBottom) / client.Height;

            if (keep)
            {
                if (glWidth * client.Width * field.Height > glHeight * client.Height * field.Width)
                {
                    double scale = (glHeight * client.Height * field.Width) / (glWidth * client.Width * field.Height);
                    glLeft += (1.0 - scale) * glWidth * 0.5;
                    glWidth *= scale;
                } else
                {
                    double scale = (glWidth * client.Width * field.Height) / (glHeight * client.Height * field.Width);
                    glTop -= (1.0 - scale) * glHeight * 0.5;
                    glHeight *= scale;
                }
            }
        }

        public GL2DUtil(Size clientSize, Size fieldSize, bool keepAspectRatio = true)
        {
            client = clientSize;
            field = fieldSize;
            margins = new int[]{ 0, 0, 0, 0 };
            KeepAspectRatio = keepAspectRatio;
        }
        
        public Vector2d Position(double x, double y)
        {
            return new Vector2d(
                (glLeft + (glWidth * x) / field.Width),
                (glTop - (glHeight * y) / field.Height)
            );
        }

        public Vector2d Position(Point pos)
        {
            return Position(pos.X, pos.Y);
        }

        public Vector2d Position(Vector2d src)
        {
            return Position(src.X, src.Y);
        }

        // Gave top-left coordinate and the dimensions to draw the rectangle
        public void DrawRectangle(double x, double y, double width, double height, Color color)
        {
            GL.Begin(BeginMode.Polygon);
            GL.Color4(color);
            GL.Vertex2(Position(x, y));
            GL.Vertex2(Position(x, y + height));
            GL.Vertex2(Position(x + width, y + height));
            GL.Vertex2(Position(x + width, y));
            GL.End();
        }

        public void DrawRectangle(Rectangle rect, Color color)
        {
            DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height, color);
        }

        public void DrawRectangle(Vector2d location, SizeF size, Color color)
        {
            DrawRectangle(location.X, location.Y, size.Width, size.Height, color);
        }

        public void DrawPolygon(Point[] vertices, Color color)
        {
            GL.Begin(BeginMode.Polygon);
            GL.Color4(color);
            foreach (var vertex in vertices)
                GL.Vertex2(Position(vertex));
            GL.End();
        }

        public void DrawPolygon(Vector2d[] vertices, Color color)
        {
            GL.Begin(BeginMode.Polygon);
            GL.Color4(color);
            foreach (var vertex in vertices)
                GL.Vertex2(Position(vertex));
            GL.End();
        }

        public void DrawCircle(double x, double y, double radius, Color color,  int num = 20)
        {
            Vector2d[] vertices = new Vector2d[num];
            for (int i = 0; i < num; ++i)
            {
                double angle = (Math.PI * 2.0 * i) / num;
                vertices[i] = new Vector2d(x + radius * Math.Cos(angle), y + radius * Math.Sin(angle));
            }
            DrawPolygon(vertices, color);
        }

        public void DrawCircle(Vector2d location, double radius, Color color, int num = 20)
        {
            DrawCircle(location.X, location.Y, radius, color, num);
        }
    }
}
