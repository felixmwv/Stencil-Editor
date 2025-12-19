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
using Avalonia.Input;


namespace MyAvaloniaApp
{
    public partial class MainWindow : Window
    {
        private ProjectData currentProject = new ProjectData();
        private Stack<ICommand> undoStack = new();
        private Stack<ICommand> redoStack = new();
        private ShapeBase? selectedShape;

        private double startX, startY, startScale, startRotation;

        private bool isDragging = false;
        private bool isScaling = false;

        private Point lastMousePosition;

        private bool IsPointInPolygon(Point p, PolygonShape poly)
        {
            bool inside = false;
            int j = poly.Points.Count - 1;

            for (int i = 0; i < poly.Points.Count; j = i++)
            {
                var pi = poly.Points[i];
                var pj = poly.Points[j];

                bool intersect =
                    ((pi.Y > p.Y) != (pj.Y > p.Y)) &&
                    (p.X < (pj.X - pi.X) * (p.Y - pi.Y) / (pj.Y - pi.Y) + pi.X);

                if (intersect)
                    inside = !inside;
            }

            return inside;
        }
        public MainWindow()
        {
            InitializeComponent();
        }
        private ShapeBase? HitTest(Point mousePos)
        {
            foreach (var c in currentProject.Circles)
            {
                var dx = mousePos.X - c.X;
                var dy = mousePos.Y - c.Y;
                var distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance <= c.Radius * c.Scale)
                    return c;
            }
            foreach (var p in currentProject.Polygons)
            {
                var local = TransformToLocal(mousePos, p);

                if (IsPointInPolygon(local, p))
                    return p;
            }
            foreach (var r in currentProject.Rectangles)
            {
                var dx = mousePos.X - r.X;
                var dy = mousePos.Y - r.Y;
                var distance = Math.Sqrt(dx * dx + dy * dy);
                
                if (distance <= r.Radius * r.Scale)
                    return r;
            }
            return null;
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
                    RenderTransform = new TransformGroup
                    {
                        Children =
                        {
                            new ScaleTransform(c.Scale, c.Scale),
                            new RotateTransform(c.Rotation)
                        }
                    },
                    RenderTransformOrigin = RelativePoint.Center
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
                    RenderTransform = new TransformGroup
                    {
                        Children =
                        {
                            new ScaleTransform(r.Scale, r.Scale),
                            new RotateTransform(r.Rotation)
                        }
                    },
                    RenderTransformOrigin = RelativePoint.Center
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
                    ),
                    RenderTransform = new TransformGroup
                    {
                        Children =
                        {
                            new ScaleTransform(p.Scale, p.Scale),
                            new RotateTransform(p.Rotation)
                        }
                    },
                    RenderTransformOrigin = RelativePoint.Center
                };
                Canvas.SetLeft(polygon, p.X);
                Canvas.SetTop(polygon, p.Y);
                DrawingCanvas.Children.Add(polygon);
            }
        }
        private Point TransformToLocal(Point world, PolygonShape poly)
        {
            var dx = world.X - poly.X;
            var dy = world.Y - poly.Y;

            var center = GetPolygonCenter(poly);
            
            dx -= center.X;
            dy -= center.Y;

            var rad = -poly.Rotation * Math.PI / 180;
            var cos = Math.Cos(rad);
            var sin = Math.Sin(rad);

            var rx = dx * cos - dy * sin;
            var ry = dx * sin + dy * cos;
            
            rx /= poly.Scale;
            ry /= poly.Scale;

            rx += center.X;
            ry += center.Y;

            return new Point(rx, ry);
        }

        private Point GetPolygonCenter(PolygonShape poly)
        {
            double x = 0;
            double y = 0;

            foreach (var p in poly.Points)
            {
                x += p.X;
                y += p.Y;
            }
            return new Point(
                x / poly.Points.Count,
                y / poly.Points.Count
            );
        }
        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var pos = e.GetPosition(DrawingCanvas);
            selectedShape = HitTest(pos);

            if (selectedShape == null)
                return;

            lastMousePosition = pos;

            startX = selectedShape.X;
            startY = selectedShape.Y;
            startScale = selectedShape.Scale;
            startRotation = selectedShape.Rotation;

            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                isDragging = true;

            if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
                isScaling = true;
        }
        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            
            if (selectedShape == null)
                return;
            var pos = e.GetPosition(DrawingCanvas);
            var delta = pos - lastMousePosition;

            if (isDragging)
            {
                selectedShape.X += delta.X;
                selectedShape.Y += delta.Y;
            }
            if (isScaling)
            {
                selectedShape.Scale += delta.X * 0.01;
                selectedShape.Scale = Math.Clamp(
                    selectedShape.Scale,
                    ShapeBase.MinScale,
                    ShapeBase.MaxScale
                );
            }
            lastMousePosition = pos;
            RedrawCanvas();
        }
        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (selectedShape != null)
            {
                if (startX != selectedShape.X ||
                    startY != selectedShape.Y ||
                    startScale != selectedShape.Scale ||
                    startRotation != selectedShape.Rotation)
                {
                    var command = new TransformCommand(
                        selectedShape,
                        startX, startY,
                        startScale, startRotation,
                        selectedShape.X,
                        selectedShape.Y,
                        selectedShape.Scale,
                        selectedShape.Rotation
                    );
                    undoStack.Push(command);
                    redoStack.Clear();
                }
            }
            isDragging = false;
            isScaling = false;
            selectedShape = null;
        }

        private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            if (selectedShape == null || !isDragging)
                return;

            selectedShape.Rotation += e.Delta.Y * 5;
            RedrawCanvas();
        }
        private void OnSpawnCircleClicked(object? sender, RoutedEventArgs e)
        {
            var circle = new CircleShape
            {
                X = 100,
                Y = 100,
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
            var rectangle = new RectangleShape
            {
                X = 100,
                Y = 200,
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
            var polygon = new PolygonShape
            {
                X = 70,
                Y = 270,
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
        private async void OnExportSvgClicked(object? sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                DefaultExtension = "svg",
                Filters = { new FileDialogFilter { Name = "SVG", Extensions = { "svg" } } }
            };

            var path = await dialog.ShowAsync(this);
            if (path != null)
                SvgExporter.Export(currentProject, path);
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