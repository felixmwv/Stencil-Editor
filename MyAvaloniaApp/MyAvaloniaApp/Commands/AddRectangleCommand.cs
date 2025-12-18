using MyAvaloniaApp.Shapes;

namespace MyAvaloniaApp.Commands
{
    public class AddRectangleCommand : ICommand
    {
        private readonly ProjectData project;
        private readonly RectangleShape _rectangle;

        public AddRectangleCommand(ProjectData project, RectangleShape rectangle)
        {
            this.project = project;
            this._rectangle = rectangle;
        }
        public void Execute()
        {
            project.Rectangles.Add(_rectangle);
        }

        public void Undo()
        {
            project.Rectangles.Remove(_rectangle);
        }
    }
}