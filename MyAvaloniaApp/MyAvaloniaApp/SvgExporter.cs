using System.Linq;
using MyAvaloniaApp;
using MyAvaloniaApp.Shapes;

public static class SvgExporter
{
    public static void Export(ProjectData project, string path, bool perLayer = false)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<svg xmlns='http://www.w3.org/2000/svg'>");

        if (perLayer)
        {
            int layerIndex = 1;
            foreach (var layer in project.Layers)
            {
                sb.AppendLine($"<g id='layer{layerIndex}' fill='rgb({layer.Color.R},{layer.Color.G},{layer.Color.B})'>");
                foreach (var shape in layer.Shapes)
                {
                    AppendShapeSvg(sb, shape, layer.Color);
                }
                sb.AppendLine("</g>");
                layerIndex++;
            }
        }
        else
        {
            foreach (var layer in project.Layers)
            {
                foreach (var shape in layer.Shapes)
                {
                    AppendShapeSvg(sb, shape, layer.Color);
                }
            }
        }

        sb.AppendLine("</svg>");
        System.IO.File.WriteAllText(path, sb.ToString());
    }

    private static void AppendShapeSvg(System.Text.StringBuilder sb, ShapeBase shape, Avalonia.Media.Color color)
    {
        string colorStr = $"rgb({color.R},{color.G},{color.B})";

        if (shape is CircleShape c)
        {
            sb.AppendLine(
                $"<circle cx='{c.X}' cy='{c.Y}' r='{c.Radius * c.Scale}' " +
                $"transform='rotate({c.Rotation} {c.X} {c.Y})' fill='{colorStr}' />");
        }
        else if (shape is RectangleShape r)
        {
            double size = r.Radius * 2 * r.Scale;
            sb.AppendLine(
                $"<rect x='{r.X - size / 2}' y='{r.Y - size / 2}' " +
                $"width='{size}' height='{size}' " +
                $"transform='rotate({r.Rotation} {r.X} {r.Y})' fill='{colorStr}' />");
        }
        else if (shape is PolygonShape p)
        {
            double centerX = p.Points.Average(pt => pt.X);
            double centerY = p.Points.Average(pt => pt.Y);
            var pointsStr = string.Join(" ", p.Points.Select(pt => $"{pt.X},{pt.Y}"));

            sb.AppendLine(
                $"<polygon points='{pointsStr}' fill='{colorStr}' " +
                $"transform='translate({p.X},{p.Y}) translate({centerX},{centerY}) rotate({p.Rotation}) scale({p.Scale}) translate({-centerX},{-centerY})' />");
        }
    }
}
