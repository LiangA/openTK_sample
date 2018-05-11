using System.Drawing;

namespace OpenTK_Sample
{
    class ColorPeeker
    {
        public static Color PeekColor(int i)
        {
            i %= 8;
            switch (i)
            {
                case 0: return Color.Red;
                case 2: return Color.Orange;
                case 4: return Color.Yellow;
                case 6: return Color.YellowGreen;
                case 1: return Color.Green;
                case 3: return Color.Blue;
                case 5: return Color.Purple;
                case 7: return Color.Cyan;
                default:
                    return Color.Gray;
            }
        }
    }
}
