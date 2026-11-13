using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace MediaRemote;

/// <summary>Draws a small TV-remote-shaped tray icon so there's no external .ico asset to ship.</summary>
internal static class RemoteIcon
{
    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    public static Icon Create()
    {
        using var bitmap = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            using var body = RoundedRect(new Rectangle(6, 1, 20, 30), 6);
            using var bodyBrush = new SolidBrush(Color.FromArgb(255, 48, 48, 52));
            g.FillPath(bodyBrush, body);

            using var topButtonBrush = new SolidBrush(Color.FromArgb(255, 0, 180, 255));
            g.FillEllipse(topButtonBrush, 12, 5, 8, 8);

            using var barBrush = new SolidBrush(Color.FromArgb(255, 220, 220, 224));
            g.FillRectangle(barBrush, 10, 16, 12, 4);
            g.FillRectangle(barBrush, 10, 22, 12, 4);
            g.FillEllipse(barBrush, 12, 27, 8, 4);
        }

        var hIcon = bitmap.GetHicon();
        try
        {
            using var temp = Icon.FromHandle(hIcon);
            return (Icon)temp.Clone();
        }
        finally
        {
            DestroyIcon(hIcon);
        }
    }

    private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var diameter = radius * 2;
        var path = new GraphicsPath();
        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}
