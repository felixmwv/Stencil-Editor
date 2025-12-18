using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Interactivity;
using System;
using MyAvaloniaApp.Shapes;
using MyAvaloniaApp.Commands;
using System.Collections.Generic;
using System.Linq;
using Avalonia;


namespace MyAvaloniaApp
{
    public partial class MainWindow : Window
    {
        private ProjectData currentProject = new ProjectData();
        private Stack<ICommand> undoStack = new();
        private Stack<ICommand> redoStack = new();

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
            foreach (var r in currentProject.Rectangles)
            {
                var rectangle = new Rectangle
                {
                    Width = r.Radius * 2,
                    Height = r.Radius * 2,
                    Fill = Brushes.Red,
                };
                Canvas.SetLeft(rectangle, r.X - r.Radius);
                Canvas.SetTop(rectangle, r.Y - r.Radius);
                DrawingCanvas.Children.Add(rectangle);
            }
            foreach (var p in currentProject.Polygons)
            {
                var polygon = new Polygon
                {
                    Fill = Brushes.Red,
                    Points = new Points(
                        p.Points.Select(pt => new Avalonia.Point(pt.X, pt.Y))
                    )
                };

                Canvas.SetLeft(polygon, p.X);
                Canvas.SetTop(polygon, p.Y);

                DrawingCanvas.Children.Add(polygon);
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

            var command = new AddCircleCommand(currentProject, circle);

            command.Execute();
            undoStack.Push(command);
            redoStack.Clear();

            RedrawCanvas();
        }

        private void OnSpawnRectangleClicked(object? sender, RoutedEventArgs e)
        {
            var random = new Random();

            var rectangle = new RectangleShape
            {
                X = random.Next(50, 750),
                Y = random.Next(50, 400),
                Radius = 30
            };
            
            var command = new AddRectangleCommand(currentProject, rectangle);
            
            command.Execute();
            undoStack.Push(command);
            redoStack.Clear();
            
            RedrawCanvas();
        }

        private void OnSpawnPolygonClicked(object? sender, RoutedEventArgs e)
        {
            var random = new Random();

            var polygon = new PolygonShape
            {
                X = random.Next(50, 750),
                Y = random.Next(50, 400),
                Points =
                {
                    new PointData { X = 0, Y = 60 },
                    new PointData { X = 30, Y = 0 },
                    new PointData { X = 60, Y = 60 }
                }
            };
            var command = new AddPolygonCommand(currentProject, polygon);
            
            command.Execute();
            undoStack.Push(command);
            redoStack.Clear();
            
            RedrawCanvas();
        }
        private void OnUndoClicked(object? sender, RoutedEventArgs e)
        {
            if (undoStack.Count == 0)
                return;

            var command = undoStack.Pop();
            command.Undo();
            redoStack.Push(command);

            RedrawCanvas();
        }

        private void OnRedoClicked(object? sender, RoutedEventArgs e)
        {
            if (redoStack.Count == 0)
                return;

            var command = redoStack.Pop();
            command.Execute();
            undoStack.Push(command);

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