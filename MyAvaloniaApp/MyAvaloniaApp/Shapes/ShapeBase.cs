using System.Collections.Generic;

namespace MyAvaloniaApp.Shapes
{
    public abstract class ShapeBase
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Scale { get; set; } = 1.0;
        public double Rotation { get; set; } = 0.0;

        public const double MinScale = 0.2;
        public const double MaxScale = 10.0;
    }

    public class CircleShape : ShapeBase
    {
        public double Radius { get; set; } = 30;
    }

    public class RectangleShape : ShapeBase
    {
        public double Radius { get; set; } = 30;
    }

    public class PointData
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class PolygonShape : ShapeBase
    {
        public List<PointData> Points { get; set; } = new();
    }
}