using System.Security.Cryptography;
using System.Text;

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
    readonly TextBox ghost5 = Box("#FFFFFF");
    readonly TextBox logo = new TextBox();
    readonly TextBox pellet = new TextBox();
    readonly TextBox power0 = new TextBox();
    readonly TextBox power1 = new TextBox();
    readonly TextBox power2 = new TextBox();
    readonly TextBox power3 = new TextBox();
    readonly TextBox pacman = new TextBox();

    public MainForm(RSA rsa, string startupMessage)
    {
        this.rsa = rsa;
        Text = "Skin Packer";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(760, 640);
        Size = new Size(820, 720);
        Font = new Font("Segoe UI", 9f);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            Padding = new Padding(16),
            AutoScroll = true
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));

        int row = 0;
        AddNote(root, ref row, "Keep this program and the keys folder. Ship only the game exe and skin.pack.");
        AddColor(root, ref row, "Text color 1", textPrimary);
        AddColor(root, ref row, "Text color 2", textSecondary);
        AddColor(root, ref row, "Wall color", wall);
        AddColor(root, ref row, "Blinky", ghost1);
        AddColor(root, ref row, "Pinky", ghost2);
        AddColor(root, ref row, "Inky", ghost3);
        AddColor(root, ref row, "Clyde", ghost4);
        AddColor(root, ref row, "Ghost 5", ghost5);
        AddFile(root, ref row, "Client logo", logo);
        AddFile(root, ref row, "Pellet", pellet);
        AddFile(root, ref row, "PowerPellet", power0);
        AddFile(root, ref row, "PowerPellet 1", power1);
        AddFile(root, ref row, "PowerPellet 2", power2);
        AddFile(root, ref row, "PowerPellet 3", power3);
        AddFile(root, ref row, "Pac-Man", pacman);

        var export = new Button { Text = "Export skin.pack", AutoSize = true, Padding = new Padding(8, 4, 8, 4) };
        export.Click += (_, __) => ExportPack();
        root.Controls.Add(export, 0, row);
        root.SetColumnSpan(export, 3);
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        Controls.Add(root);

        if (!string.IsNullOrEmpty(startupMessage))
            Shown += (_, __) => MessageBox.Show(this, startupMessage, "Skin Packer");
    }

    static TextBox Box(string hex)
    {
        return new TextBox { Text = hex, Anchor = AnchorStyles.Left | AnchorStyles.Right };
    }

    void AddNote(TableLayoutPanel table, ref int row, string text)
    {
        var label = new Label { Text = text, AutoSize = true, MaximumSize = new Size(740, 0), Margin = new Padding(0, 0, 0, 12) };
        table.Controls.Add(label, 0, row);
        table.SetColumnSpan(label, 3);
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        row++;
    }

    void AddColor(TableLayoutPanel table, ref int row, string name, TextBox box)
    {
        table.Controls.Add(new Label { Text = name, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 8, 4) }, 0, row);
        box.Margin = new Padding(0, 4, 8, 4);
        table.Controls.Add(box, 1, row);
        var pick = new Button { Text = "Pick", AutoSize = true, Margin = new Padding(0, 4, 0, 4) };
        pick.Click += (_, __) => PickColor(box);
        table.Controls.Add(pick, 2, row);
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        row++;
    }

    void AddFile(TableLayoutPanel table, ref int row, string name, TextBox box)
    {
        table.Controls.Add(new Label { Text = name, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 8, 4) }, 0, row);
        box.ReadOnly = true;
        box.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        box.Margin = new Padding(0, 4, 8, 4);
        table.Controls.Add(box, 1, row);
        var browse = new Button { Text = "Browse", AutoSize = true, Margin = new Padding(0, 4, 0, 4) };
        browse.Click += (_, __) => Browse(box);
        table.Controls.Add(browse, 2, row);
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        row++;
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
            || !TryHex(ghost4.Text, out string g4)
            || !TryHex(ghost5.Text, out string g5))
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
            + "\",\"ghosts\":[\"" + g1 + "\",\"" + g2 + "\",\"" + g3 + "\",\"" + g4 + "\",\"" + g5 + "\"]}";
        entries.Add(("colors", Encoding.UTF8.GetBytes(json)));

        using var dialog = new SaveFileDialog();
        dialog.Filter = "Skin pack (*.pack)|*.pack";
        dialog.FileName = "skin.pack";
        string root = KeyStore.FindProjectRoot();
        if (root != null)
            dialog.InitialDirectory = root;
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        File.WriteAllBytes(dialog.FileName, PackFormat.Build(entries, rsa));
        MessageBox.Show(this, "Wrote the skin pack. Put skin.pack in the same folder as the game exe. Do not send SkinPacker or the keys folder with that build.", "Skin Packer");
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
