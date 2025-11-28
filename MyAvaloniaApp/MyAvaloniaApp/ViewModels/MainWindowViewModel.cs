using ReactiveUI;

namespace MyAvaloniaApp.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private string message = "Hello from the ViewModel!";
    public string Message
    {
        get => message;
        set => this.RaiseAndSetIfChanged(ref message, value);
    }
}