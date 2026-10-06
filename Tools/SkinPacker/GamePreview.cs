using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace SkinPacker;

sealed class SkinLook
{
    public Color TextPrimary = ColorTranslator.FromHtml("#E40A6D");
    public Color TextSecondary = ColorTranslator.FromHtml("#0099C4");
    public Color Wall = Color.White;
    public Color Background = Color.Black;
    public Color[] Ghosts = new Color[4];
    public string Logo;
    public string Pellet;
    public string Pacman;
    public string[] Powers = new string[4];
}

sealed class GamePreview : UserControl
{
    const int CanvasW = 1080;
    const int CanvasH = 1920;
    const float PxPerWorld = CanvasH / 52f;

    readonly BoardMap board;
    readonly string projectRoot;
    readonly Dictionary<string, Bitmap> images = new();
    readonly PrivateFontCollection fonts = new();
    FontFamily pixelFamily;
    Bitmap frame;
    SkinLook look = new();

    public GamePreview()
    {
        projectRoot = KeyStore.FindProjectRoot();
        board = BoardMap.Load(projectRoot);
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.FromArgb(18, 18, 18);
        LoadPixelFont();
    }

    public void Show(SkinLook next)
    {
        look = next ?? new SkinLook();
        Rebuild();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
        if (frame == null)
        {
            e.Graphics.Clear(Color.Black);
            return;
        }
        e.Graphics.DrawImage(frame, ClientRectangle);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            frame?.Dispose();
            foreach (Bitmap image in images.Values)
                image.Dispose();
            images.Clear();
            fonts.Dispose();
        }
        base.Dispose(disposing);
    }

    void Rebuild()
    {
        var next = new Bitmap(CanvasW, CanvasH);
        using (var g = Graphics.FromImage(next))
        {
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.None;
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            g.Clear(look.Background);
            DrawBoard(g);
            DrawHud(g);
        }

        Bitmap previous = frame;
        frame = next;
        previous?.Dispose();
        Invalidate();
    }

    void DrawBoard(Graphics g)
    {
        foreach (var cell in board.Walls)
        {
            Bitmap sprite = null;
            if (cell.Sprite >= 0 && cell.Sprite < board.WallSprites.Count)
                sprite = LoadCached(board.WallSprites[cell.Sprite], false);
            if (sprite == null)
                continue;
            DrawWorld(g, sprite, cell.X + 0.5f, cell.Y + 0.5f, sprite.Width / 24f, look.Wall, true);
        }

        Bitmap pellet = ImageOrDefault(look.Pellet, "Assets/Sprites/Pellet_Medium.png", true);
        foreach (var cell in board.Pellets)
            DrawWorld(g, pellet, cell.X + 0.5f, cell.Y + 0.5f, 0.42f, Color.White, false);

        for (int i = 0; i < board.Powers.Count; i++)
        {
            var power = board.Powers[i];
            string chosen = power.Slot >= 0 && power.Slot < look.Powers.Length ? look.Powers[power.Slot] : null;
            Bitmap image = ImageOrDefault(chosen, "Assets/Sprites/Pellet_Large" + (power.Slot == 0 ? "" : " " + power.Slot) + ".png", true);
            DrawWorld(g, image, power.X + 0.5f, power.Y + 0.5f, 1.15f, Color.White, false);
        }

        Bitmap body = ImageOrDefault(null, "Assets/Sprites/Ghost_Body_01.png", false);
        Bitmap eyes = ImageOrDefault(null, "Assets/Sprites/Ghost_Eyes_Right.png", false);
        float ghostSize = WorldWidth(body, 70f, 1.7f);
        foreach (var ghost in board.Ghosts)
        {
            Color tint = ghost.Slot >= 0 && ghost.Slot < look.Ghosts.Length ? look.Ghosts[ghost.Slot] : Color.White;
            DrawWorld(g, body, ghost.X, ghost.Y, ghostSize, tint, true);
            DrawWorld(g, eyes, ghost.X, ghost.Y, ghostSize, Color.White, false);
        }

        Bitmap pacman = ImageOrDefault(look.Pacman, "Assets/Sprites/Pacman_02.png", true);
        DrawWorld(g, pacman, board.PacmanX, board.PacmanY, WorldWidth(pacman, 8f, 2f), Color.White, false);
    }

    void DrawHud(Graphics g)
    {
        Bitmap logo = ImageOrDefault(look.Logo, "Assets/Sprites/epex logo 11 new.png", true);
        DrawFit(g, logo, new RectangleF(309.7f, 66.2f, 460.6f, 150.6f));

        DrawLabel(g, "HIGH SCORE 0", new RectangleF(19f, 283f, 691f, 29f), look.TextPrimary, 26f, false);
        DrawLabel(g, "YOUR SCORE 0", new RectangleF(19f, 332f, 691f, 29f), look.TextPrimary, 26f, false);
        DrawLabel(g, "TIME:60", new RectangleF(350f, 332f, 380f, 36f), look.TextSecondary, 30f, true);

        Bitmap pacman = ImageOrDefault(look.Pacman, "Assets/Sprites/Pacman_02.png", true);
        DrawLives(g, pacman, new RectangleF(998f, 293f, 64f, 64f));
        DrawLabel(g, "x3", new RectangleF(911f, 293f, 106f, 64f), look.TextPrimary, 32f, true);

        DrawLabel(g, "Collectables:", new RectangleF(194.5f, 1608f, 691f, 29f), look.TextSecondary, 25f, true);

        float icon = 120.5f;
        float gap = 69.6f;
        float left = 194.5f;
        float top = 1668f;
        for (int i = 0; i < 4; i++)
        {
            string chosen = look.Powers != null && i < look.Powers.Length ? look.Powers[i] : null;
            string fallback = "Assets/Sprites/Pellet_Large" + (i == 0 ? "" : " " + i) + ".png";
            DrawFit(g, ImageOrDefault(chosen, fallback, true), new RectangleF(left + i * (icon + gap), top, icon, 184f));
        }
    }

    void DrawLives(Graphics g, Bitmap image, RectangleF box)
    {
        if (image == null)
            return;
        var flipped = (Bitmap)image.Clone();
        flipped.RotateFlip(RotateFlipType.Rotate180FlipNone);
        DrawFit(g, flipped, box);
        flipped.Dispose();
    }

    void DrawLabel(Graphics g, string text, RectangleF box, Color color, float size, bool center)
    {
        using Font font = MakeFont(size);
        using var brush = new SolidBrush(color);
        var format = new StringFormat
        {
            Alignment = center ? StringAlignment.Center : StringAlignment.Near,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter
        };
        g.DrawString(text, font, brush, box, format);
    }

    void DrawWorld(Graphics g, Bitmap image, float worldX, float worldY, float worldWidth, Color tint, bool multiply)
    {
        if (image == null || worldWidth <= 0f)
            return;
        float worldHeight = worldWidth * image.Height / image.Width;
        PointF center = ToScreen(worldX, worldY);
        var dest = new RectangleF(
            center.X - worldWidth * PxPerWorld * 0.5f,
            center.Y - worldHeight * PxPerWorld * 0.5f,
            worldWidth * PxPerWorld,
            worldHeight * PxPerWorld);
        if (!multiply)
        {
            g.DrawImage(image, dest);
            return;
        }

        float r = tint.R / 255f;
        float gv = tint.G / 255f;
        float b = tint.B / 255f;
        using var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix(new[]
        {
            new[] { r, 0f, 0f, 0f, 0f },
            new[] { 0f, gv, 0f, 0f, 0f },
            new[] { 0f, 0f, b, 0f, 0f },
            new[] { 0f, 0f, 0f, 1f, 0f },
            new[] { 0f, 0f, 0f, 0f, 1f }
        }));
        g.DrawImage(
            image,
            Rectangle.Round(dest),
            0,
            0,
            image.Width,
            image.Height,
            GraphicsUnit.Pixel,
            attributes);
    }

    static void DrawFit(Graphics g, Bitmap image, RectangleF box)
    {
        if (image == null)
            return;
        float scale = Math.Min(box.Width / image.Width, box.Height / image.Height);
        float width = image.Width * scale;
        float height = image.Height * scale;
        g.DrawImage(image, box.X + (box.Width - width) * 0.5f, box.Y + (box.Height - height) * 0.5f, width, height);
    }

    static PointF ToScreen(float worldX, float worldY)
    {
        const float halfW = 26f * CanvasW / (float)CanvasH;
        const float left = -halfW;
        const float top = -1f + 26f;
        return new PointF((worldX - left) * PxPerWorld, (top - worldY) * PxPerWorld);
    }

    static float WorldWidth(Bitmap image, float pixelsPerUnit, float fallback)
    {
        if (image == null || pixelsPerUnit <= 0f)
            return fallback;
        return image.Width / pixelsPerUnit;
    }

    Bitmap ImageOrDefault(string chosen, string projectRelative, bool trimMargins)
    {
        Bitmap image = LoadCached(chosen, trimMargins);
        if (image != null)
            return image;
        if (string.IsNullOrEmpty(projectRoot) || string.IsNullOrEmpty(projectRelative))
            return null;
        return LoadCached(Path.Combine(projectRoot, projectRelative), trimMargins);
    }

    Bitmap LoadCached(string path, bool trimMargins)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;
        string key = trimMargins ? path + "\ntrim" : path;
        if (images.TryGetValue(key, out Bitmap cached))
            return cached;

        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            using var stream = new MemoryStream(bytes);
            using Image loaded = Image.FromStream(stream);
            var bitmap = new Bitmap(loaded);
            if (trimMargins)
            {
                Bitmap cropped = TrimMargins(bitmap);
                if (!ReferenceEquals(cropped, bitmap))
                    bitmap.Dispose();
                bitmap = cropped;
            }
            images[key] = bitmap;
            return bitmap;
        }
        catch (Exception)
        {
            return null;
        }
    }

    static Bitmap TrimMargins(Bitmap source)
    {
        Bitmap bitmap = source.PixelFormat == PixelFormat.Format32bppArgb
            ? source
            : source.Clone(new Rectangle(0, 0, source.Width, source.Height), PixelFormat.Format32bppArgb);
        var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        int minX = bitmap.Width;
        int minY = bitmap.Height;
        int maxX = -1;
        int maxY = -1;
        var row = new byte[Math.Abs(data.Stride)];
        for (int y = 0; y < bitmap.Height; y++)
        {
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0 + y * data.Stride, row, 0, row.Length);
            for (int x = 0; x < bitmap.Width; x++)
            {
                int i = x * 4;
                if (i + 3 >= row.Length)
                    break;
                byte b = row[i];
                byte g = row[i + 1];
                byte r = row[i + 2];
                byte a = row[i + 3];
                bool empty = a < 16 || (a > 240 && r > 248 && g > 248 && b > 248);
                if (empty)
                    continue;
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }
        }
        bitmap.UnlockBits(data);
        if (maxX < minX || maxY < minY)
            return bitmap;

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;
        if (width > bitmap.Width * 0.92f && height > bitmap.Height * 0.92f)
            return bitmap;

        Bitmap cropped = bitmap.Clone(new Rectangle(minX, minY, width, height), PixelFormat.Format32bppArgb);
        if (!ReferenceEquals(bitmap, source))
            bitmap.Dispose();
        return cropped;
    }

    void LoadPixelFont()
    {
        if (string.IsNullOrEmpty(projectRoot))
            return;
        string path = Path.Combine(projectRoot, "Assets", "Fonts", "PressStart2P.ttf");
        if (!File.Exists(path))
            return;
        try
        {
            fonts.AddFontFile(path);
            if (fonts.Families.Length > 0)
                pixelFamily = fonts.Families[0];
        }
        catch (Exception)
        {
            pixelFamily = null;
        }
    }

    Font MakeFont(float pixels)
    {
        if (pixelFamily != null)
            return new Font(pixelFamily, pixels, FontStyle.Regular, GraphicsUnit.Pixel);
        return new Font("Courier New", pixels * 0.75f, FontStyle.Bold, GraphicsUnit.Pixel);
    }
}
