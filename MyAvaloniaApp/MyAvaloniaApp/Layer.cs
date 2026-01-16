using Avalonia.Media;
using System.Collections.Generic;
using MyAvaloniaApp.Shapes;

namespace MyAvaloniaApp
{
    public class Layer
    {
        public string Name { get; set; } = "Layer";
        public bool IsVisible { get; set; } = true;
        public Color Color { get; set; } = Colors.Red;

        public List<ShapeBase> Shapes { get; set; } = new List<ShapeBase>();
        
        public override string ToString()
        {
            return Name;
        }
    }
}