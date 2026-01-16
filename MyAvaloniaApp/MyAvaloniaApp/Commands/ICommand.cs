using MyAvaloniaApp.Shapes;

namespace MyAvaloniaApp.Commands
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    public class AddShapeCommand : ICommand
    {
        private Layer layer;
        private ShapeBase shape;

        public AddShapeCommand(Layer layer, ShapeBase shape)
        {
            this.layer = layer;
            this.shape = shape;
        }

        public void Execute()
        { 
            layer.Shapes.Add(shape);
        }

        public void Undo()
        {
            layer.Shapes.Remove(shape);
        } 
    }
    public class RemoveShapeCommand : ICommand
    {
        private Layer layer;
        private ShapeBase shape;

        public RemoveShapeCommand(Layer layer, ShapeBase shape)
        {
            this.layer = layer;
            this.shape = shape;
        }

        public void Execute()
        {
            layer.Shapes.Remove(shape);
        }

        public void Undo()
        {
            layer.Shapes.Add(shape);
        }
    }
    public class TransformCommand : ICommand
    {
        private ShapeBase shape;
        private double oldX, oldY, oldScale, oldRotation;
        private double newX, newY, newScale, newRotation;

        public TransformCommand(ShapeBase shape, double oldX, double oldY, double oldScale, double oldRotation,
                                double newX, double newY, double newScale, double newRotation)
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
}