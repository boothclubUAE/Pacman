using System.Globalization;
using System.Text.RegularExpressions;

namespace SkinPacker;

// Display 2 is the portrait game: Main Camera (target display 1) plus the 1080x1920 canvas.
sealed class BoardMap
{
    public readonly List<(int X, int Y, int Sprite)> Walls = new();
    public readonly List<string> WallSprites = new();
    public readonly List<(int X, int Y)> Pellets = new();
    public readonly List<(int X, int Y, int Slot)> Powers = new();
    public readonly List<(int Slot, float X, float Y)> Ghosts = new();
    public float PacmanX = 0f;
    public float PacmanY = -9.5f;

    public static BoardMap Load(string projectRoot)
    {
        var board = new BoardMap();
        board.Ghosts.Add((0, 0f, 2.5f));
        board.Ghosts.Add((1, 0f, -0.5f));
        board.Ghosts.Add((2, -2f, -0.5f));
        board.Ghosts.Add((3, 2f, -0.5f));
        if (string.IsNullOrEmpty(projectRoot))
            return board;

        string scenePath = Path.Combine(projectRoot, "Assets", "Scenes", "Pacman.unity");
        if (!File.Exists(scenePath))
            return board;

        string scene = File.ReadAllText(scenePath);
        var sprites = GuidPaths(projectRoot);
        foreach (string guid in ReadSpriteGuids(scene, "Walls"))
            board.WallSprites.Add(sprites.TryGetValue(guid, out string path) ? path : null);
        foreach (var cell in ReadTiles(scene, "Walls"))
            board.Walls.Add((cell.X, cell.Y, cell.Sprite));

        foreach (var cell in ReadTiles(scene, "Pellets"))
        {
            int slot = PowerSlot(cell.Tile);
            if (slot >= 0)
                board.Powers.Add((cell.X, cell.Y, slot));
            else
                board.Pellets.Add((cell.X, cell.Y));
        }

        ReadActors(scene, board);
        return board;
    }

    // Pellet tile assets, in the order stored on the Pellets tilemap.
    static int PowerSlot(int tile)
    {
        switch (tile)
        {
            case 1: return 0; // PowerPellet
            case 0: return 1; // PowerPellet 1
            case 3: return 2; // PowerPellet 2
            case 4: return 3; // PowerPellet 3
            default: return -1;
        }
    }

    static List<(int X, int Y, int Tile, int Sprite)> ReadTiles(string scene, string objectName)
    {
        var cells = new List<(int X, int Y, int Tile, int Sprite)>();
        int nameAt = IndexOfName(scene, objectName);
        if (nameAt < 0)
            return cells;

        int tilesAt = scene.IndexOf("m_Tiles:", nameAt, StringComparison.Ordinal);
        int end = scene.IndexOf("m_AnimatedTiles:", tilesAt, StringComparison.Ordinal);
        if (tilesAt < 0 || end < 0)
            return cells;

        string block = scene.Substring(tilesAt, end - tilesAt);
        var matches = Regex.Matches(block, @"first: \{x: (-?\d+), y: (-?\d+), z: 0\}[\s\S]*?m_TileIndex: (\d+)[\s\S]*?m_TileSpriteIndex: (\d+)");
        foreach (Match match in matches)
        {
            long sprite = long.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
            cells.Add((
                int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
                int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture),
                int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture),
                sprite > int.MaxValue ? -1 : (int)sprite));
        }
        return cells;
    }

    static List<string> ReadSpriteGuids(string scene, string objectName)
    {
        var guids = new List<string>();
        int nameAt = IndexOfName(scene, objectName);
        if (nameAt < 0)
            return guids;
        int start = scene.IndexOf("m_TileSpriteArray:", nameAt, StringComparison.Ordinal);
        int end = scene.IndexOf("m_TileColorArray:", start, StringComparison.Ordinal);
        if (start < 0 || end < 0)
            return guids;
        foreach (Match match in Regex.Matches(scene.Substring(start, end - start), @"guid: ([0-9a-fA-F]{32})"))
            guids.Add(match.Groups[1].Value);
        return guids;
    }

    static int IndexOfName(string scene, string objectName)
    {
        int nameAt = scene.IndexOf("m_Name: " + objectName + "\n", StringComparison.Ordinal);
        if (nameAt < 0)
            nameAt = scene.IndexOf("m_Name: " + objectName + "\r", StringComparison.Ordinal);
        return nameAt;
    }

    static Dictionary<string, string> GuidPaths(string projectRoot)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string assets = Path.Combine(projectRoot, "Assets");
        if (!Directory.Exists(assets))
            return map;
        foreach (string meta in Directory.EnumerateFiles(assets, "*.meta", SearchOption.AllDirectories))
        {
            string guid = null;
            using (var reader = new StreamReader(meta))
            {
                reader.ReadLine();
                string line = reader.ReadLine();
                if (line != null && line.StartsWith("guid: ", StringComparison.Ordinal))
                    guid = line.Substring(6).Trim();
            }
            if (guid != null)
                map[guid] = meta.Substring(0, meta.Length - 5);
        }
        return map;
    }

    static void ReadActors(string scene, BoardMap board)
    {
        var ghostGuid = new Dictionary<string, int>
        {
            ["25f5544974e8f0b409f1c5070cbb2c0e"] = 0,
            ["87e861fcfe4201c4d8213cba69c0f024"] = 1,
            ["8398b287235293d469d1683369c48531"] = 2,
            ["0d1f951e4ac24fe478d688dbf86a5c39"] = 3
        };

        var found = new Dictionary<int, (float X, float Y)>();
        int cursor = 0;
        const string marker = "--- !u!1001";
        while (cursor < scene.Length)
        {
            int start = scene.IndexOf(marker, cursor, StringComparison.Ordinal);
            if (start < 0)
                break;
            int next = scene.IndexOf(marker, start + marker.Length, StringComparison.Ordinal);
            if (next < 0)
                next = scene.Length;
            string block = scene.Substring(start, next - start);
            cursor = next;

            if (IsSourcePrefab(block, "b6894cca0173e8041bba0e4a8be5fde1")
                && TryPosition(block, out float px, out float py))
            {
                board.PacmanX = px;
                board.PacmanY = py;
            }

            foreach (var pair in ghostGuid)
            {
                if (!IsSourcePrefab(block, pair.Key))
                    continue;
                if (TryPosition(block, out float x, out float y))
                    found[pair.Value] = (x, y);
            }
        }

        if (found.Count == 0)
            return;

        board.Ghosts.Clear();
        foreach (var pair in found.OrderBy(item => item.Key))
            board.Ghosts.Add((pair.Key, pair.Value.X, pair.Value.Y));
    }

    static bool IsSourcePrefab(string block, string guid)
    {
        return block.Contains("m_SourcePrefab: {fileID: 100100000, guid: " + guid + ",", StringComparison.Ordinal);
    }

    static bool TryPosition(string block, out float x, out float y)
    {
        x = 0f;
        y = 0f;
        var match = Regex.Match(
            block,
            @"propertyPath: m_LocalPosition\.x\s+value: (-?[\d.]+)\s+objectReference: \{fileID: 0\}\s+- target:[\s\S]*?propertyPath: m_LocalPosition\.y\s+value: (-?[\d.]+)");
        if (!match.Success)
            return false;
        x = float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        y = float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        return true;
    }
}
