using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using MyAvaloniaApp;

public static class SvgExporter
{
    public static void Export(ProjectData project, string path)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<svg xmlns='http://www.w3.org/2000/svg'>");

        foreach (var c in project.Circles)
        {
            sb.AppendLine(
                $"<circle cx='{c.X}' cy='{c.Y}' r='{c.Radius * c.Scale}' " +
                $"transform='rotate({c.Rotation} {c.X} {c.Y})' fill='red' />");
        }

        foreach (var r in project.Rectangles)
        {
            var size = r.Radius * 2 * r.Scale;
            sb.AppendLine(
                $"<rect x='{r.X - size / 2}' y='{r.Y - size / 2}' " +
                $"width='{size}' height='{size}' " +
                $"transform='rotate({r.Rotation} {r.X} {r.Y})' fill='red' />");
        }

        foreach (var p in project.Polygons)
        {
            var centerX = p.Points.Average(pt => pt.X);
            var centerY = p.Points.Average(pt => pt.Y);

            var points = string.Join(" ",
                p.Points.Select(pt =>
                    $"{pt.X.ToString(CultureInfo.InvariantCulture)}," +
                    $"{pt.Y.ToString(CultureInfo.InvariantCulture)}"));

            sb.AppendLine(
                $"<polygon points='{points}' fill='red' " + $"transform='" + $"translate({p.X},{p.Y}) " +
                $"translate({centerX},{centerY}) " + $"rotate({p.Rotation}) " + $"scale({p.Scale}) " +
                $"translate({-centerX},{-centerY})" + $"'" + $" />");
        }

        sb.AppendLine("</svg>");

        File.WriteAllText(path, sb.ToString());
    }
}