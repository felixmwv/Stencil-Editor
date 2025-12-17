namespace MyAvaloniaApp.Commands
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}