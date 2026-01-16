using Avalonia.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MyAvaloniaApp.Shapes;

namespace MyAvaloniaApp
{
    public class Layer
    {
        public string Name { get; set; } = "Layer";
        public bool IsVisible { get; set; } = true;
        public Color Color { get; set; } = Colors.Red;

        public ObservableCollection<ShapeBase> Shapes { get; set; } = new();
        
        public override string ToString()
        {
            return Name;
        }
    }
}