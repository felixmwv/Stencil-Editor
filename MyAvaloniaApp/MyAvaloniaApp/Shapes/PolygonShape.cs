using System.Collections.Generic;

namespace MyAvaloniaApp.Shapes;

public class PolygonShape
{
    public double X { get; set; }
    public double Y { get; set; }
    public List<PointData> Points { get; set; } = new();
}