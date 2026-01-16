using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MyAvaloniaApp
{
    public class ProjectData
    {
        public ObservableCollection<Layer> Layers { get; set; } = new();
    }
}