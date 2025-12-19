namespace MyAvaloniaApp.Shapes;

public abstract class ShapeBase
{
    public double X { get; set; }
    public double Y { get; set; }

    public double Scale { get; set; } = 1.0;
    public double Rotation { get; set; } = 0.0;

    public const double MinScale = 0.2;
    public const double MaxScale = 3.0;
}