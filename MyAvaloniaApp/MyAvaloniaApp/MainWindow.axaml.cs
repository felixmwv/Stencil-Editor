using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Interactivity;
using MyAvaloniaApp.Commands;
using MyAvaloniaApp.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls.Primitives;

namespace MyAvaloniaApp
{
    public partial class MainWindow : Window
    {
        private ProjectData currentProject = new ProjectData();
        private Stack<ICommand> undoStack = new();
        private Stack<ICommand> redoStack = new();
        private ShapeBase? selectedShape;

        private Layer activeLayer;

        private double startX, startY, startScale, startRotation;
        private bool isDragging = false;
        private bool isScaling = false;
        private Point lastMousePosition;

        private string selectedShapeType;

        public MainWindow()
        {
            InitializeComponent();
            
            var layer = new Layer { Name = "Layer 1", Color = Colors.Red };
            currentProject.Layers.Add(layer);
            activeLayer = layer;
            ShapeCombo.SelectedIndex = 0;
            selectedShapeType = "Circle";
            
            LayerCombo.ItemsSource = currentProject.Layers;
            LayerCombo.SelectedItem = activeLayer;

            LayerVisibleCheckBox.IsChecked = activeLayer.IsVisible;
            SetLayerSliders(activeLayer.Color);
        }

        #region HITTEST & POLYGON UTILS
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
            return new Point(x / poly.Points.Count, y / poly.Points.Count);
        }

        private ShapeBase? HitTest(Point mousePos)
        {
            if (activeLayer == null) 
                return null;
            
            foreach (var shape in activeLayer.Shapes.AsEnumerable().Reverse())
            {
                if (!activeLayer.IsVisible)
                    continue;

                if (shape is CircleShape c)
                {
                    var dx = mousePos.X - c.X;
                    var dy = mousePos.Y - c.Y;
                    if (Math.Sqrt(dx * dx + dy * dy) <= c.Radius * c.Scale)
                        return c;
                }
                else if (shape is RectangleShape r)
                {
                    var dx = mousePos.X - r.X;
                    var dy = mousePos.Y - r.Y;
                    if (Math.Sqrt(dx * dx + dy * dy) <= r.Radius * r.Scale)
                        return r;
                }
                else if (shape is PolygonShape p)
                {
                    var local = TransformToLocal(mousePos, p);
                    if (IsPointInPolygon(local, p))
                        return p;
                }
            }

            return null;
        }
        #endregion

        #region REDRAW CANVAS
        private void RedrawCanvas()
        {
            DrawingCanvas.Children.Clear();

            foreach (var layer in currentProject.Layers)
            {
                if (!layer.IsVisible)
                    continue;

                foreach (var shape in layer.Shapes)
                {
                    if (shape is CircleShape c)
                    {
                        var ellipse = new Ellipse
                        {
                            Width = c.Radius * 2,
                            Height = c.Radius * 2,
                            Fill = new SolidColorBrush(layer.Color),
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
                    else if (shape is RectangleShape r)
                    {
                        var rect = new Rectangle
                        {
                            Width = r.Radius * 2,
                            Height = r.Radius * 2,
                            Fill = new SolidColorBrush(layer.Color),
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
                        Canvas.SetLeft(rect, r.X - r.Radius);
                        Canvas.SetTop(rect, r.Y - r.Radius);
                        DrawingCanvas.Children.Add(rect);
                    }
                    else if (shape is PolygonShape p)
                    {
                        var polygon = new Polygon
                        {
                            Fill = new SolidColorBrush(layer.Color),
                            Points = new Points(p.Points.Select(pt => new Avalonia.Point(pt.X, pt.Y))),
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
            }
        }
        #endregion

        #region POINTER EVENTS
        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var pos = e.GetPosition(DrawingCanvas);
            selectedShape = HitTest(pos);

            lastMousePosition = pos;

            if (selectedShape != null)
            {
                startX = selectedShape.X;
                startY = selectedShape.Y;
                startScale = selectedShape.Scale;
                startRotation = selectedShape.Rotation;
            }
            
            if (selectedShape != null &&
                e.GetCurrentPoint(this).Properties.IsLeftButtonPressed &&
                e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                var removeCmd = new RemoveShapeCommand(activeLayer, selectedShape);
                removeCmd.Execute();
                undoStack.Push(removeCmd);
                redoStack.Clear();

                selectedShape = null;
                RedrawCanvas();
                return;
            }

            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                isDragging = true;

            if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
                isScaling = true;
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (selectedShape == null) return;

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
                selectedShape.Scale = Math.Clamp(selectedShape.Scale, ShapeBase.MinScale, ShapeBase.MaxScale);
            }

            lastMousePosition = pos;
            RedrawCanvas();
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (selectedShape != null)
            {
                if (startX != selectedShape.X || startY != selectedShape.Y ||
                    startScale != selectedShape.Scale || startRotation != selectedShape.Rotation)
                {
                    var cmd = new TransformCommand(selectedShape, startX, startY, startScale, startRotation,
                                                   selectedShape.X, selectedShape.Y, selectedShape.Scale, selectedShape.Rotation);
                    undoStack.Push(cmd);
                    redoStack.Clear();
                }
            }

            isDragging = false;
            isScaling = false;
            selectedShape = null;
        }

        private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            if (selectedShape == null) return;

            selectedShape.Rotation += e.Delta.Y * 5;
            RedrawCanvas();
        }
        #endregion

        #region SPAWN SHAPES
        
        private void OnShapeChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (ShapeCombo.SelectedItem is ComboBoxItem item && item.Content != null)
                selectedShapeType = item.Content.ToString()!;
        }

        private void OnAddShapeClicked(object? sender, RoutedEventArgs e)
        {
            if (activeLayer == null) return;

            ShapeBase shape = selectedShapeType switch
            {
                "Circle" => new CircleShape { X = 200, Y = 200, Radius = 30 },
                "Rectangle" => new RectangleShape { X = 200, Y = 200, Radius = 30 },
                "Polygon" => new PolygonShape
                {
                    X = 200,
                    Y = 200,
                    Points =
                    {
                        new PointData { X = 0, Y = 60 },
                        new PointData { X = 30, Y = 0 },
                        new PointData { X = 60, Y = 60 }
                    }
                },
                _ => throw new Exception("Unknown shape type")
            };

            var cmd = new AddShapeCommand(activeLayer, shape);
            cmd.Execute();
            undoStack.Push(cmd);
            redoStack.Clear();

            RedrawCanvas();
        }
        #endregion

        #region UNDO / REDO
        private void OnUndoClicked(object? sender, RoutedEventArgs e)
        {
            if (undoStack.Count == 0) return;
            var cmd = undoStack.Pop();
            cmd.Undo();
            redoStack.Push(cmd);
            RedrawCanvas();
        }

        private void OnRedoClicked(object? sender, RoutedEventArgs e)
        {
            if (redoStack.Count == 0) return;
            var cmd = redoStack.Pop();
            cmd.Execute();
            undoStack.Push(cmd);
            RedrawCanvas();
        }
        #endregion

        #region LAYERS
        private void OnAddLayerClicked(object? sender, RoutedEventArgs e)
        {
            var layer = new Layer { Name = $"Layer {currentProject.Layers.Count + 1}", Color = Colors.Gray };
            currentProject.Layers.Add(layer);
            activeLayer = layer;
            LayerCombo.SelectedItem = layer;
            LayerVisibleCheckBox.IsChecked = layer.IsVisible;
            SetLayerSliders(layer.Color);
            RedrawCanvas();
        }

        private void OnDeleteLayerClicked(object? sender, RoutedEventArgs e)
        {
            if (currentProject.Layers.Count <= 1) return;
            if (activeLayer != null)
                currentProject.Layers.Remove(activeLayer);
            activeLayer = currentProject.Layers[0];
            LayerCombo.SelectedItem = activeLayer;
            LayerVisibleCheckBox.IsChecked = activeLayer.IsVisible;
            SetLayerSliders(activeLayer.Color);
            RedrawCanvas();
        }

        private void OnLayerChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (LayerCombo.SelectedItem is Layer layer)
            {
                activeLayer = layer;
                LayerVisibleCheckBox.IsChecked = layer.IsVisible;
                SetLayerSliders(layer.Color);
            }
        }
        
        private void OnLayerVisibilityChanged(object? sender, RoutedEventArgs e)
        {
            if (activeLayer != null)
            {
                activeLayer.IsVisible = LayerVisibleCheckBox.IsChecked ?? true;
                RedrawCanvas();
            }
        }
        #endregion

        #region LAYER COLOR
        private void SetLayerSliders(Color color)
        {
            RSlider.Value = color.R;
            GSlider.Value = color.G;
            BSlider.Value = color.B;
            HexBox.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private void OnLayerColorChanged(object? sender, RoutedEventArgs e)
        {
            if (activeLayer == null)
                return;

            activeLayer.Color = Avalonia.Media.Color.FromRgb(
                (byte)RSlider.Value,
                (byte)GSlider.Value,
                (byte)BSlider.Value
            );

            HexBox.Text = $"#{activeLayer.Color.R:X2}{activeLayer.Color.G:X2}{activeLayer.Color.B:X2}";
            RedrawCanvas();
        }

        private void OnHexChanged(object? sender, RoutedEventArgs e)
        {
            if (activeLayer == null) return;
            try
            {
                var color = Color.Parse(HexBox.Text);
                activeLayer.Color = color;
                SetLayerSliders(color);
                RedrawCanvas();
            }
            catch { }
        }
        #endregion

        #region EXPORT / SAVE / LOAD
        private async void OnExportSvgClicked(object? sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                DefaultExtension = "svg",
                Filters = { new FileDialogFilter { Name = "SVG", Extensions = { "svg" } } }
            };

            var path = await dialog.ShowAsync(this);
            if (path == null) return;
            
            bool perLayer = ExportPerLayerCheckBox.IsChecked ?? false;

            SvgExporter.Export(currentProject, path, perLayer);
        }

        private async void OnSaveClicked(object? sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { DefaultExtension = "json" };
            var path = await dialog.ShowAsync(this);
            if (path != null)
                await SaveManager.SaveAsync(currentProject, path);
        }

        private async void OnLoadClicked(object? sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { AllowMultiple = false };
            var res = await dialog.ShowAsync(this);
            if (res != null && res.Length > 0)
            {
                var loaded = await SaveManager.LoadAsync(res[0]);
                if (loaded != null)
                {
                    currentProject = loaded;
                    activeLayer = currentProject.Layers.First();
                    LayerCombo.ItemsSource = currentProject.Layers;
                    LayerCombo.SelectedItem = activeLayer;
                    SetLayerSliders(activeLayer.Color);
                    LayerVisibleCheckBox.IsChecked = activeLayer.IsVisible;
                    RedrawCanvas();
                }
            }
        }
        #endregion
    }
}
