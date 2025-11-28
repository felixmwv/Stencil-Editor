using System.Collections.Generic;
using MyAvaloniaApp.Shapes;

namespace MyAvaloniaApp;

public class ProjectData
{
    //public string ProjectName { get; set; }
    public List<CircleShape> Circles { get; set; } = new List<CircleShape>();
}