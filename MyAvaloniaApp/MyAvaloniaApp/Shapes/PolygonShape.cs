using System.Collections.Generic;

namespace MyAvaloniaApp.Shapes;

public class PolygonShape : ShapeBase
{
    public List<PointData> Points { get; set; } = new();
}