using MyAvaloniaApp.Shapes;

namespace MyAvaloniaApp.Commands
{
    public class AddCircleCommand : ICommand
    {
        private readonly ProjectData project;
        private readonly CircleShape circle;

        public AddCircleCommand(ProjectData project, CircleShape circle)
        {
            this.project = project;
            this.circle = circle;
        }

        public void Execute()
        {
            project.Circles.Add(circle);
        }

        public void Undo()
        {
            project.Circles.Remove(circle);
        }
    }
}