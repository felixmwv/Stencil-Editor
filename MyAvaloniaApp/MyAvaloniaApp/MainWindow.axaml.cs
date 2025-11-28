using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Interactivity;
using System;
using MyAvaloniaApp.Shapes;

namespace MyAvaloniaApp
{
    public partial class MainWindow : Window
    {
        private ProjectData currentProject = new ProjectData();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void RedrawCanvas()
        {
            DrawingCanvas.Children.Clear();
            foreach (var c in currentProject.Circles)
            {
                var ellipse = new Ellipse
                {
                    Width = c.Radius * 2,
                    Height = c.Radius * 2,
                    Fill = Brushes.Red,
                };
                Canvas.SetLeft(ellipse, c.X - c.Radius);
                Canvas.SetTop(ellipse, c.Y - c.Radius);
                DrawingCanvas.Children.Add(ellipse);
            }
        }

        private void OnSpawnCircleClicked(object? sender, RoutedEventArgs e)
        {
            var random = new Random();
            var circle = new CircleShape
            {
                X = random.Next(50, 750),
                Y = random.Next(50, 400),
                Radius = 30
            };
            currentProject.Circles.Add(circle);
            RedrawCanvas();
        }

        private async void OnSaveClicked(object? sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                DefaultExtension = "json",
                Filters =
                {
                    new FileDialogFilter { Name = "Stencil Project", Extensions = { "json" } }
                }
            };
            var filePath = await dialog.ShowAsync(this);
            if (filePath == null)
                return;
            await SaveManager.SaveAsync(currentProject, filePath);
        }

        private async void OnLoadClicked(object? sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Filters =
                {
                    new FileDialogFilter { Name = "Stencil Project", Extensions = { "json" } }
                }
            };
            var result = await dialog.ShowAsync(this);
            if (result == null || result.Length == 0)
                return;
            string filePath = result[0];
            var loaded = await SaveManager.LoadAsync(filePath);
            if (loaded != null)
            {
                currentProject = loaded;
                RedrawCanvas();
            }
        }
    }
}