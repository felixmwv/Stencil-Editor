using MyAvaloniaApp.Shapes;

namespace MyAvaloniaApp.Commands
{
    public class AddPolygonCommand : ICommand
    {
        private readonly ProjectData project;
        private readonly PolygonShape _polygon;

        public AddPolygonCommand(ProjectData project, PolygonShape polygon)
        {
            this.project = project;
            this._polygon = polygon;
        }
        public void Execute()
        {
            project.Polygons.Add(_polygon);
        }

        public void Undo()
        {
            project.Polygons.Remove(_polygon);
        }
    }
}