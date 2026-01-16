using System.Collections.Generic;

namespace MyAvaloniaApp.Shapes
{
    public class PointData
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class PolygonShape : ShapeBase
    {
        public List<PointData> Points { get; set; } = new List<PointData>();
    }
}