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
            var points = string.Join(" ",
                p.Points.Select(pt =>
                    $"{(pt.X + p.X)},{(pt.Y + p.Y)}"));

            sb.AppendLine(
                $"<polygon points='{points}' " +
                $"transform='rotate({p.Rotation} {p.X} {p.Y}) scale({p.Scale})' fill='red' />");
        }

        sb.AppendLine("</svg>");

        File.WriteAllText(path, sb.ToString());
    }
}