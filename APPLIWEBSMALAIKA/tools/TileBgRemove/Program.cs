using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

static bool IsBackground(byte r, byte g, byte b)
{
    int mx = Math.Max(r, Math.Max(g, b));
    int mn = Math.Min(r, Math.Min(g, b));
    if (mx - mn > 20) return false;
    double avg = (r + g + b) / 3.0;
    if (avg >= 246) return true;
    if (avg >= 188 && avg <= 232) return true;
    return false;
}

static void Process(string path)
{
    using var src = new Bitmap(path);
    int w = src.Width, h = src.Height;
    var rgb = new byte[w * h * 3];
    var rect = new Rectangle(0, 0, w, h);
    var data = src.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
    try
    {
        int stride = data.Stride;
        nint scan = data.Scan0;
        for (int y = 0; y < h; y++)
        {
            Marshal.Copy(scan + y * stride, rgb, y * w * 3, w * 3);
        }
    }
    finally
    {
        src.UnlockBits(data);
    }

    static (byte r, byte g, byte b) Get(byte[] buf, int w, int x, int y)
    {
        int i = (y * w + x) * 3;
        return (buf[i + 2], buf[i + 1], buf[i]);
    }

    var visited = new bool[w * h];
    var queue = new Queue<int>();

    void Enqueue(int x, int y)
    {
        if (x < 0 || y < 0 || x >= w || y >= h) return;
        int idx = y * w + x;
        if (visited[idx]) return;
        var (r, g, b) = Get(rgb, w, x, y);
        if (!IsBackground(r, g, b)) return;
        visited[idx] = true;
        queue.Enqueue(idx);
    }

    for (int x = 0; x < w; x++) { Enqueue(x, 0); Enqueue(x, h - 1); }
    for (int y = 0; y < h; y++) { Enqueue(0, y); Enqueue(w - 1, y); }

    while (queue.Count > 0)
    {
        int idx = queue.Dequeue();
        int x = idx % w, y = idx / w;
        Enqueue(x + 1, y);
        Enqueue(x - 1, y);
        Enqueue(x, y + 1);
        Enqueue(x, y - 1);
    }

    using var dst = new Bitmap(w, h, PixelFormat.Format32bppArgb);
    var outData = dst.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
    try
    {
        var row = new byte[w * 4];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int o = x * 4;
                if (visited[y * w + x])
                {
                    row[o] = 0;
                    row[o + 1] = 0;
                    row[o + 2] = 0;
                    row[o + 3] = 0;
                }
                else
                {
                    var (r, g, b) = Get(rgb, w, x, y);
                    row[o] = b;
                    row[o + 1] = g;
                    row[o + 2] = r;
                    row[o + 3] = 255;
                }
            }
            Marshal.Copy(row, 0, outData.Scan0 + y * outData.Stride, row.Length);
        }
    }
    finally
    {
        dst.UnlockBits(outData);
    }

    var dir = Path.GetDirectoryName(path)!;
    var name = Path.GetFileNameWithoutExtension(path);
    var finalPath = Path.Combine(dir, name + ".png");
    var tmp = Path.Combine(dir, name + ".tmp.png");
    dst.Save(tmp, ImageFormat.Png);
    try
    {
        if (File.Exists(finalPath))
            File.Delete(finalPath);
        File.Move(tmp, finalPath);
    }
    catch (IOException)
    {
        var alt = Path.Combine(dir, name + "-nobg.png");
        File.Move(tmp, alt, true);
        Console.WriteLine($"Locked — wrote {alt}");
        return;
    }
    Console.WriteLine($"Processed {finalPath}");
}

foreach (var path in args)
{
    if (!File.Exists(path))
    {
        Console.Error.WriteLine($"Missing: {path}");
        continue;
    }
    Process(path);
}
