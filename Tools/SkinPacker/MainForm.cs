using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SkinPacker;

sealed class MainForm : Form
{
    readonly RSA rsa;
    readonly TextBox textPrimary = Box("#E40A6D");
    readonly TextBox textSecondary = Box("#0099C4");
    readonly TextBox wall = Box("#FFFFFF");
    readonly TextBox ghost1 = Box("#F65285");
    readonly TextBox ghost2 = Box("#E7C2F4");
    readonly TextBox ghost3 = Box("#FEA4C0");
    readonly TextBox ghost4 = Box("#F19884");
    readonly TextBox logo = new TextBox();
    readonly TextBox pellet = new TextBox();
    readonly TextBox power0 = new TextBox();
    readonly TextBox power1 = new TextBox();
    readonly TextBox power2 = new TextBox();
    readonly TextBox power3 = new TextBox();
    readonly TextBox pacman = new TextBox();
    readonly Dictionary<TextBox, Button> swatches = new();
    readonly Dictionary<TextBox, PictureBox> previews = new();
    readonly GamePreview gamePreview = new();

    public MainForm(RSA rsa, string startupMessage)
    {
        this.rsa = rsa;
        Text = "Skin Packer";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1180, 760);
        Size = new Size(1360, 920);
        Font = new Font("Segoe UI", 9f);

        var split = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 480));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        split.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        gamePreview.Dock = DockStyle.Fill;
        gamePreview.Margin = new Padding(0);

        var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 12, 16, 12) };
        var buttons = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 44,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0, 8, 0, 0)
        };
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        var open = new Button { Text = "Open", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 0) };
        var export = new Button { Text = "Export", Dock = DockStyle.Fill, Margin = new Padding(6, 0, 0, 0) };
        open.Click += (_, __) => OpenPack();
        export.Click += (_, __) => ExportPack();
        buttons.Controls.Add(open, 0, 0);
        buttons.Controls.Add(export, 1, 0);

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        var stack = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0)
        };
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stack.Controls.Add(Grid(3,
            ColorCard("Text color 1", textPrimary),
            ColorCard("Text color 2", textSecondary),
            ColorCard("Wall color", wall)), 0, 0);
        stack.Controls.Add(Grid(4,
            ColorCard("Ghost 1", ghost1),
            ColorCard("Ghost 2", ghost2),
            ColorCard("Ghost 3", ghost3),
            ColorCard("Ghost 4", ghost4)), 0, 1);
        stack.Controls.Add(Grid(3,
            FileCard("Client logo", logo),
            FileCard("Pellet", pellet),
            FileCard("Pac-Man", pacman)), 0, 2);
        stack.Controls.Add(Grid(4,
            FileCard("Collectable 1", power0),
            FileCard("Collectable 2", power1),
            FileCard("Collectable 3", power2),
            FileCard("Collectable 4", power3)), 0, 3);
        scroll.Controls.Add(stack);
        scroll.Resize += (_, __) => stack.Width = Math.Max(320, scroll.ClientSize.Width - 4);

        right.Controls.Add(scroll);
        right.Controls.Add(buttons);
        split.Controls.Add(gamePreview, 0, 0);
        split.Controls.Add(right, 1, 0);
        split.Resize += (_, __) =>
        {
            int height = split.ClientSize.Height;
            if (height > 0)
                split.ColumnStyles[0].Width = (int)Math.Round(height * 1080.0 / 1920.0);
        };
        Controls.Add(split);
        RefreshGame();

        if (!string.IsNullOrEmpty(startupMessage))
            Shown += (_, __) => MessageBox.Show(this, startupMessage, "Skin Packer");
    }

    static TextBox Box(string hex)
    {
        return new TextBox { Text = hex, Anchor = AnchorStyles.Left | AnchorStyles.Right };
    }

    static TableLayoutPanel Grid(int columns, params Control[] cells)
    {
        int rows = (cells.Length + columns - 1) / columns;
        var grid = new TableLayoutPanel
        {
            ColumnCount = columns,
            RowCount = rows,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 0, 0, 22)
        };
        for (int i = 0; i < columns; i++)
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
        for (int i = 0; i < rows; i++)
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        for (int i = 0; i < cells.Length; i++)
            grid.Controls.Add(cells[i], i % columns, i / columns);
        return grid;
    }

    Control ColorCard(string name, TextBox box)
    {
        var card = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = 3,
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(6, 12, 6, 12)
        };
        card.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        card.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        card.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        card.Controls.Add(new Label { Text = name, AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 6) }, 0, 0);

        var pick = new Button
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 0, 6),
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false
        };
        pick.FlatAppearance.BorderColor = Color.FromArgb(90, 90, 90);
        pick.Click += (_, __) => PickColor(box);
        swatches.Add(box, pick);
        card.Controls.Add(pick, 0, 1);

        box.Dock = DockStyle.Fill;
        box.Margin = new Padding(0, 4, 0, 0);
        box.TextChanged += (_, __) => RefreshSwatch(box);
        card.Controls.Add(box, 0, 2);
        RefreshSwatch(box);
        return card;
    }

    Control FileCard(string name, TextBox box)
    {
        var card = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = 4,
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(6, 12, 6, 12)
        };
        card.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        card.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        card.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        card.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        card.Controls.Add(new Label { Text = name, AutoSize = true, Dock = DockStyle.Fill }, 0, 0);
        var preview = new PictureBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 0, 4),
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(32, 32, 32)
        };
        previews.Add(box, preview);
        card.Controls.Add(preview, 0, 1);
        var browse = new Button { Text = "Browse", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 4) };
        browse.Click += (_, __) => Browse(box);
        card.Controls.Add(browse, 0, 2);
        box.ReadOnly = true;
        box.Dock = DockStyle.Fill;
        box.Margin = new Padding(0);
        box.TextChanged += (_, __) => RefreshPreview(box);
        card.Controls.Add(box, 0, 3);
        return card;
    }

    void RefreshSwatch(TextBox box)
    {
        if (!swatches.TryGetValue(box, out Button swatch))
            return;
        Color color = TryHex(box.Text, out string hex)
            ? ColorTranslator.FromHtml(hex)
            : SystemColors.Control;
        swatch.Text = string.IsNullOrEmpty(hex) ? box.Text : hex;
        swatch.BackColor = color;
        swatch.ForeColor = color.R * 0.299f + color.G * 0.587f + color.B * 0.114f > 150f ? Color.Black : Color.White;
        RefreshGame();
    }

    void RefreshPreview(TextBox box)
    {
        if (!previews.TryGetValue(box, out PictureBox preview))
            return;
        Image previous = preview.Image;
        preview.Image = null;
        previous?.Dispose();
        if (string.IsNullOrWhiteSpace(box.Text) || !File.Exists(box.Text))
        {
            RefreshGame();
            return;
        }

        byte[] bytes = File.ReadAllBytes(box.Text);
        using var stream = new MemoryStream(bytes);
        using Image loaded = Image.FromStream(stream);
        preview.Image = new Bitmap(loaded);
        RefreshGame();
    }

    void RefreshGame()
    {
        if (!IsHandleCreated && gamePreview.Parent == null)
            return;

        gamePreview.Show(new SkinLook
        {
            TextPrimary = ColorOrDefault(textPrimary.Text, "#E40A6D"),
            TextSecondary = ColorOrDefault(textSecondary.Text, "#0099C4"),
            Wall = ColorOrDefault(wall.Text, "#FFFFFF"),
            Ghosts = new[]
            {
                ColorOrDefault(ghost1.Text, "#F65285"),
                ColorOrDefault(ghost2.Text, "#E7C2F4"),
                ColorOrDefault(ghost3.Text, "#FEA4C0"),
                ColorOrDefault(ghost4.Text, "#F19884")
            },
            Logo = logo.Text,
            Pellet = pellet.Text,
            Powers = new[] { power0.Text, power1.Text, power2.Text, power3.Text },
            Pacman = pacman.Text
        });
    }

    static Color ColorOrDefault(string text, string fallback)
    {
        if (!TryHex(text, out string hex))
            hex = fallback;
        return ColorTranslator.FromHtml(hex);
    }

    void OpenPack()
    {
        using var dialog = new OpenFileDialog();
        dialog.Filter = "Data (*.bin)|*.bin";
        dialog.FileName = "data.bin";
        string root = KeyStore.FindProjectRoot();
        if (root != null)
            dialog.InitialDirectory = root;
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        byte[] file = File.ReadAllBytes(dialog.FileName);
        if (!PackFormat.TryRead(file, rsa, out Dictionary<string, byte[]> entries))
        {
            MessageBox.Show(this, "This pack could not be opened. It was not signed with this project's key.", "Skin Packer");
            return;
        }

        if (entries.TryGetValue("colors", out byte[] colorBytes))
            ApplyLoadedColors(Encoding.UTF8.GetString(colorBytes));

        LoadImage(entries, "logo", logo);
        LoadImage(entries, "pellet", pellet);
        LoadImage(entries, "power0", power0);
        LoadImage(entries, "power1", power1);
        LoadImage(entries, "power2", power2);
        LoadImage(entries, "power3", power3);
        LoadImage(entries, "pacman", pacman);
    }

    void ApplyLoadedColors(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;
        SetHex(textPrimary, root, "textPrimary");
        SetHex(textSecondary, root, "textSecondary");
        SetHex(wall, root, "wall");
        if (!root.TryGetProperty("ghosts", out JsonElement ghosts))
            return;
        TextBox[] boxes = { ghost1, ghost2, ghost3, ghost4 };
        for (int i = 0; i < boxes.Length && i < ghosts.GetArrayLength(); i++)
            boxes[i].Text = ghosts[i].GetString();
    }

    static void SetHex(TextBox box, JsonElement root, string name)
    {
        if (root.TryGetProperty(name, out JsonElement value))
            box.Text = value.GetString();
    }

    void LoadImage(Dictionary<string, byte[]> entries, string name, TextBox box)
    {
        if (!entries.TryGetValue(name, out byte[] data) || data == null || data.Length == 0)
        {
            box.Text = "";
            return;
        }

        string folder = Path.Combine(Path.GetTempPath(), "SkinPacker");
        Directory.CreateDirectory(folder);
        string extension = data.Length > 3 && data[0] == 0xFF && data[1] == 0xD8 ? ".jpg" : ".png";
        string path = Path.Combine(folder, name + extension);
        File.WriteAllBytes(path, data);
        box.Text = "";
        box.Text = path;
    }

    static void PickColor(TextBox box)
    {
        if (!TryHex(box.Text, out string hex))
            hex = "#FFFFFF";
        using var dialog = new ColorDialog();
        dialog.FullOpen = true;
        dialog.Color = ColorTranslator.FromHtml(hex);
        if (dialog.ShowDialog() != DialogResult.OK)
            return;
        box.Text = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
    }

    static void Browse(TextBox box)
    {
        using var dialog = new OpenFileDialog();
        dialog.Filter = "Images (*.png;*.jpg)|*.png;*.jpg;*.jpeg";
        if (dialog.ShowDialog() == DialogResult.OK)
            box.Text = dialog.FileName;
    }

    void ExportPack()
    {
        if (!TryHex(textPrimary.Text, out string primary)
            || !TryHex(textSecondary.Text, out string secondary)
            || !TryHex(wall.Text, out string wallHex)
            || !TryHex(ghost1.Text, out string g1)
            || !TryHex(ghost2.Text, out string g2)
            || !TryHex(ghost3.Text, out string g3)
            || !TryHex(ghost4.Text, out string g4))
        {
            MessageBox.Show(this, "Enter each color as #RRGGBB.", "Skin Packer");
            return;
        }

        var entries = new List<(string Name, byte[] Data)>();
        if (!TryAddImage(entries, "logo", logo.Text)) return;
        if (!TryAddImage(entries, "pellet", pellet.Text)) return;
        if (!TryAddImage(entries, "power0", power0.Text)) return;
        if (!TryAddImage(entries, "power1", power1.Text)) return;
        if (!TryAddImage(entries, "power2", power2.Text)) return;
        if (!TryAddImage(entries, "power3", power3.Text)) return;
        if (!TryAddImage(entries, "pacman", pacman.Text)) return;

        string json = "{\"textPrimary\":\"" + primary
            + "\",\"textSecondary\":\"" + secondary
            + "\",\"wall\":\"" + wallHex
            + "\",\"ghosts\":[\"" + g1 + "\",\"" + g2 + "\",\"" + g3 + "\",\"" + g4 + "\"]}";
        entries.Add(("colors", Encoding.UTF8.GetBytes(json)));

        using var dialog = new SaveFileDialog();
        dialog.Filter = "Data (*.bin)|*.bin";
        dialog.FileName = "data.bin";
        string root = KeyStore.FindProjectRoot();
        if (root != null)
            dialog.InitialDirectory = root;
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        File.WriteAllBytes(dialog.FileName, PackFormat.Build(entries, rsa));
    }

    bool TryAddImage(List<(string Name, byte[] Data)> entries, string name, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return true;
        if (!File.Exists(path))
        {
            MessageBox.Show(this, "Missing file: " + path, "Skin Packer");
            return false;
        }

        byte[] data = File.ReadAllBytes(path);
        if (!IsImage(data))
        {
            MessageBox.Show(this, "Use a PNG or JPG for " + name + ".", "Skin Packer");
            return false;
        }

        entries.Add((name, data));
        return true;
    }

    static bool IsImage(byte[] data)
    {
        if (data.Length > 8 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
            return true;
        return data.Length > 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF;
    }

    static bool TryHex(string text, out string hex)
    {
        hex = null;
        if (string.IsNullOrWhiteSpace(text))
            return false;
        string value = text.Trim();
        if (value.StartsWith("#"))
            value = value.Substring(1);
        if (value.Length != 6)
            return false;
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            bool digit = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
            if (!digit)
                return false;
        }
        hex = "#" + value.ToUpperInvariant();
        return true;
    }
}
