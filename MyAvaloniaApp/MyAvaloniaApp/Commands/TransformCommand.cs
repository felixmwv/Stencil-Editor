using MyAvaloniaApp.Shapes;

namespace MyAvaloniaApp.Commands;

public class TransformCommand : ICommand
{
    private readonly ShapeBase shape;

    private readonly double oldX, oldY, oldScale, oldRotation;
    private readonly double newX, newY, newScale, newRotation;

    public TransformCommand(
        ShapeBase shape,
        double oldX, double oldY,
        double oldScale, double oldRotation,
        double newX, double newY,
        double newScale, double newRotation)
    {
        this.shape = shape;

        this.oldX = oldX;
        this.oldY = oldY;
        this.oldScale = oldScale;
        this.oldRotation = oldRotation;

        this.newX = newX;
        this.newY = newY;
        this.newScale = newScale;
        this.newRotation = newRotation;
    }

    public void Execute()
    {
        shape.X = newX;
        shape.Y = newY;
        shape.Scale = newScale;
        shape.Rotation = newRotation;
    }

    public void Undo()
    {
        shape.X = oldX;
        shape.Y = oldY;
        shape.Scale = oldScale;
        shape.Rotation = oldRotation;
    }
}