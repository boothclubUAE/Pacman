using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public static class SkinApplier
{
    static readonly List<UnityEngine.Object> alive = new List<UnityEngine.Object>();
    static string appliedKey;

    static readonly Color[] pinkText =
    {
        new Color(0.8941177f, 0.039215688f, 0.427451f, 1f),
        new Color(0.8980392f, 0f, 0.42745098f, 1f),
        new Color(0.98039216f, 0.34901962f, 0.54509807f, 1f)
    };

    static readonly Color blueText = new Color(0f, 0.6f, 0.76862746f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void ApplyFirstScene()
    {
        try
        {
            Apply(SceneManager.GetActiveScene());
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Skin pack was not applied. " + ex.Message);
        }
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        try
        {
            Apply(scene);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Skin pack was not applied. " + ex.Message);
        }
    }

    static void Apply(Scene scene)
    {
        if (!scene.IsValid())
            return;

        string key = scene.path + ":" + Time.frameCount;
        if (appliedKey == key)
            return;
        appliedKey = key;

        if (!TryLoadPack(out Dictionary<string, byte[]> entries))
            return;

        var previous = alive.ToArray();
        alive.Clear();

        if (entries.TryGetValue("colors", out byte[] colorBytes))
            ApplyColors(scene, System.Text.Encoding.UTF8.GetString(colorBytes));

        ApplyLogo(scene, entries);
        ApplyPellets(scene, entries);
        ApplyPacman(scene, entries);

        for (int i = 0; i < previous.Length; i++)
        {
            if (previous[i] != null)
                UnityEngine.Object.Destroy(previous[i]);
        }
    }

    // A pack next to the exe wins. Otherwise the copy shipped in StreamingAssets is the default.
    static bool TryLoadPack(out Dictionary<string, byte[]> entries)
    {
        entries = null;
        string external = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "data.bin");
        if (File.Exists(external))
        {
            if (SkinFile.TryRead(File.ReadAllBytes(external), out entries))
                return true;
            Debug.LogWarning("Rejected data.bin.");
        }

        string bundled = Path.Combine(Application.streamingAssetsPath, "data.bin");
        if (File.Exists(bundled) && SkinFile.TryRead(File.ReadAllBytes(bundled), out entries))
            return true;

        entries = null;
        return false;
    }

    public static void ApplyColors(Scene scene, string json)
    {
        SkinColorSet colors = JsonUtility.FromJson<SkinColorSet>(json);
        if (colors == null)
            return;

        if (TryParse(colors.wall, out Color wall))
        {
            foreach (var tilemap in Objects<Tilemap>(scene))
            {
                if (tilemap.gameObject.name == "Walls")
                    tilemap.color = wall;
            }
        }

        bool hasPrimary = TryParse(colors.textPrimary, out Color primary);
        bool hasSecondary = TryParse(colors.textSecondary, out Color secondary);
        if (hasPrimary || hasSecondary)
        {
            foreach (var text in Objects<TMP_Text>(scene))
            {
                string objectName = text.gameObject.name;
                if (hasSecondary && (objectName == "Timer" || objectName == "CollectablesText"))
                {
                    text.color = secondary;
                    continue;
                }

                if (hasPrimary && IsPink(text.color))
                    text.color = primary;
                else if (hasSecondary && IsBlue(text.color))
                    text.color = secondary;
            }
        }

        if (colors.ghosts == null)
            return;

        var named = new Dictionary<int, Ghost>();
        foreach (var ghost in Objects<Ghost>(scene))
        {
            int slot = GhostSlot(ghost);
            if (slot >= 0 && !named.ContainsKey(slot))
                named.Add(slot, ghost);
        }

        for (int i = 0; i < 4; i++)
        {
            if (named.TryGetValue(i, out Ghost ghost))
                TintGhost(ghost, colors.ghosts, i);
        }
    }

    static void TintGhost(Ghost ghost, string[] colors, int slot)
    {
        if (colors == null || slot < 0 || slot >= colors.Length)
            return;
        if (!TryParse(colors[slot], out Color color))
            return;

        Transform body = ghost.transform.Find("Body");
        if (body == null)
            return;
        SpriteRenderer renderer = body.GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.color = color;
    }

    static int GhostSlot(Ghost ghost)
    {
        string name = ghost.gameObject.name.ToLowerInvariant();
        if (name.Contains("blinky")) return 0;
        if (name.Contains("pinky")) return 1;
        if (name.Contains("inky")) return 2;
        if (name.Contains("clyde")) return 3;
        return -1;
    }

    static void ApplyLogo(Scene scene, Dictionary<string, byte[]> entries)
    {
        if (!entries.TryGetValue("logo", out byte[] png))
            return;
        Sprite sprite = LoadSprite(png, 100f);
        if (sprite == null)
            return;

        foreach (var image in Objects<Image>(scene))
        {
            if (!IsClientLogo(image.gameObject.name))
                continue;
            image.sprite = sprite;
            image.preserveAspect = true;
        }
    }

    static void ApplyPellets(Scene scene, Dictionary<string, byte[]> entries)
    {
        if (entries.TryGetValue("pellet", out byte[] pelletPng))
        {
            Sprite sprite = null;
            foreach (var pellet in Objects<Pellet>(scene))
            {
                if (pellet is PowerPellet)
                    continue;
                if (sprite == null)
                    sprite = LoadSprite(pelletPng, MatchingPpu(pellet.GetComponent<SpriteRenderer>(), pelletPng));
                if (sprite != null)
                    AssignSprite(pellet, sprite);
            }
        }

        FitPowerPellets(scene, entries);

        ApplyCollectableIcons(scene, entries);
    }

    // A tall collectable covers three pellet cells. The upper pair stays centered on its
    // pellet. The lower pair keeps its bottom on that pellet and grows upward only.
    static void FitPowerPellets(Scene scene, Dictionary<string, byte[]> entries)
    {
        var powers = new List<PowerPellet>();
        foreach (var pellet in Objects<PowerPellet>(scene))
            powers.Add(pellet);
        powers.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));
        int topCount = powers.Count / 2;

        for (int i = 0; i < powers.Count; i++)
        {
            PowerPellet pellet = powers[i];
            string key = "power" + pellet.id;
            if (!entries.TryGetValue(key, out byte[] png))
                continue;

            Texture2D texture = LoadTexture(png);
            if (texture == null)
                continue;
            bool tall = texture.height > texture.width;
            bool top = i < topCount;
            // A little under a full cell so a wide picture does not cover the pellet beside it.
            const float fit = 0.85f;
            float world = (tall ? 3f : 1f) * fit;
            float ppu = tall
                ? texture.height / world
                : Mathf.Max(texture.width, texture.height) / world;
            Vector2 pivot = tall && !top ? new Vector2(0.5f, 0.5f / world) : new Vector2(0.5f, 0.5f);
            UnityEngine.Object.Destroy(texture);

            Sprite sprite = LoadSprite(png, ppu, pivot);
            if (sprite == null)
                continue;
            AssignSprite(pellet, sprite);
            if (tall)
                ClearCoveredPellets(pellet, top);
        }
    }

    static void ClearCoveredPellets(PowerPellet pellet, bool top)
    {
        Tilemap tilemap = pellet.GetComponentInParent<Tilemap>();
        if (tilemap == null)
            return;
        Vector3Int cell = tilemap.WorldToCell(pellet.transform.position);
        if (top)
        {
            ClearPellet(tilemap, cell + new Vector3Int(0, 1, 0));
            ClearPellet(tilemap, cell + new Vector3Int(0, -1, 0));
        }
        else
        {
            ClearPellet(tilemap, cell + new Vector3Int(0, 1, 0));
            ClearPellet(tilemap, cell + new Vector3Int(0, 2, 0));
        }
    }

    static void ClearPellet(Tilemap tilemap, Vector3Int cell)
    {
        GameObject occupied = tilemap.GetInstantiatedObject(cell);
        if (occupied != null && occupied.GetComponent<PowerPellet>() != null)
            return;
        if (tilemap.HasTile(cell))
            tilemap.SetTile(cell, null);
    }

    static void ApplyCollectableIcons(Scene scene, Dictionary<string, byte[]> entries)
    {
        var sprites = new Dictionary<int, Sprite>();
        foreach (var image in Objects<Image>(scene))
        {
            Transform parent = image.transform.parent;
            if (parent == null || parent.name != "Collectables")
                continue;

            int index = image.transform.GetSiblingIndex();
            string key = "power" + index;
            if (!entries.TryGetValue(key, out byte[] png))
                continue;
            // Keep the scale already on the icon. These images also drive the maze pellets,
            // which are imported much larger than 100 pixels per unit, and the layout group
            // sizes the icons from that value.
            float ppu = image.sprite != null ? image.sprite.pixelsPerUnit : 100f;
            if (!sprites.TryGetValue(index, out Sprite sprite))
            {
                sprite = LoadSprite(png, ppu);
                sprites.Add(index, sprite);
            }
            if (sprite == null)
                continue;
            image.sprite = sprite;
            image.preserveAspect = true;
        }
    }

    static bool IsClientLogo(string name)
    {
        return name == "Client Logo" || name == "ClientLogo";
    }

    static void AssignSprite(Component owner, Sprite sprite)
    {
        SpriteRenderer renderer = owner.GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.sprite = sprite;

        AnimatedSprite animated = owner.GetComponent<AnimatedSprite>();
        if (animated != null)
            animated.sprites = new[] { sprite };
    }

    static void ApplyPacman(Scene scene, Dictionary<string, byte[]> entries)
    {
        if (!entries.TryGetValue("pacman", out byte[] png))
            return;

        Pacman pacman = First<Pacman>(scene);
        if (pacman == null)
            return;

        Texture2D source = LoadTexture(png);
        if (source == null)
            return;
        alive.Add(source);

        AnimatedSprite chomp = pacman.GetComponent<AnimatedSprite>();
        float ppu = 8f;
        SpriteRenderer body = pacman.GetComponent<SpriteRenderer>();
        if (body != null && body.sprite != null)
            ppu = MatchingPpu(body, source.width);

        Texture2D[] frames = PacmanFrames.Chomp(source);
        Sprite[] sprites = ToSprites(frames, ppu);
        if (chomp != null)
            chomp.sprites = sprites;
        if (body != null && sprites.Length > 0)
            body.sprite = sprites[0];

        Sprite[] lifeSprites = ToSprites(new[] { PacmanFrames.LivesIcon(source) }, ppu);
        if (lifeSprites.Length > 0)
        {
            foreach (var image in Objects<Image>(scene))
            {
                if (image.gameObject.name == "LivesIndicator")
                    image.sprite = lifeSprites[0];
            }
        }

        AnimatedSprite death = null;
        foreach (var animated in pacman.GetComponentsInChildren<AnimatedSprite>(true))
        {
            if (animated.gameObject != pacman.gameObject)
            {
                death = animated;
                break;
            }
        }
        if (death == null)
            return;

        int frameCount = 0;
        if (death.sprites != null)
        {
            for (int i = 0; i < death.sprites.Length; i++)
            {
                if (death.sprites[i] != null)
                    frameCount++;
            }
        }
        Texture2D[] deathFrames = PacmanFrames.Shrink(source, frameCount);
        death.sprites = ToSprites(deathFrames, ppu);
    }

    static Sprite[] ToSprites(Texture2D[] frames, float ppu)
    {
        var sprites = new Sprite[frames.Length];
        for (int i = 0; i < frames.Length; i++)
        {
            alive.Add(frames[i]);
            sprites[i] = Sprite.Create(
                frames[i],
                new Rect(0f, 0f, frames[i].width, frames[i].height),
                new Vector2(0.5f, 0.5f),
                ppu,
                0,
                SpriteMeshType.FullRect);
            sprites[i].hideFlags = HideFlags.HideAndDontSave;
            alive.Add(sprites[i]);
        }
        return sprites;
    }

    static float MatchingPpu(SpriteRenderer renderer, byte[] png)
    {
        Texture2D texture = LoadTexture(png);
        if (texture == null)
            return renderer != null && renderer.sprite != null ? renderer.sprite.pixelsPerUnit : 24f;
        float ppu = MatchingPpu(renderer, texture.width);
        UnityEngine.Object.Destroy(texture);
        return ppu;
    }

    static float MatchingPpu(SpriteRenderer renderer, int pixelWidth)
    {
        if (renderer == null || renderer.sprite == null || renderer.sprite.rect.width <= 0f || pixelWidth <= 0)
            return 24f;
        float worldWidth = renderer.sprite.rect.width / renderer.sprite.pixelsPerUnit;
        return pixelWidth / worldWidth;
    }

    static Sprite LoadSprite(byte[] png, float ppu)
    {
        return LoadSprite(png, ppu, new Vector2(0.5f, 0.5f));
    }

    static Sprite LoadSprite(byte[] png, float ppu, Vector2 pivot)
    {
        Texture2D texture = LoadTexture(png);
        if (texture == null)
            return null;
        alive.Add(texture);
        var sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            pivot,
            ppu,
            0,
            SpriteMeshType.FullRect);
        sprite.texture.filterMode = FilterMode.Point;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        alive.Add(sprite);
        return sprite;
    }

    static Texture2D LoadTexture(byte[] png)
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(png))
        {
            UnityEngine.Object.Destroy(texture);
            return null;
        }
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.HideAndDontSave;
        return texture;
    }

    static bool IsPink(Color color)
    {
        for (int i = 0; i < pinkText.Length; i++)
        {
            if (Near(color, pinkText[i]))
                return true;
        }
        return false;
    }

    static bool IsBlue(Color color)
    {
        return Near(color, blueText);
    }

    static bool Near(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.03f
            && Mathf.Abs(a.g - b.g) < 0.03f
            && Mathf.Abs(a.b - b.b) < 0.03f;
    }

    static bool TryParse(string hex, out Color color)
    {
        color = Color.white;
        if (string.IsNullOrWhiteSpace(hex))
            return false;
        string value = hex.Trim();
        if (!value.StartsWith("#"))
            value = "#" + value;
        return ColorUtility.TryParseHtmlString(value, out color);
    }

    static T First<T>(Scene scene) where T : Component
    {
        foreach (var item in Objects<T>(scene))
            return item;
        return null;
    }

    static IEnumerable<T> Objects<T>(Scene scene) where T : Component
    {
        T[] found = UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < found.Length; i++)
        {
            if (found[i] != null && found[i].gameObject.scene == scene)
                yield return found[i];
        }
    }

    [Serializable]
    public class SkinColorSet
    {
        public string textPrimary;
        public string textSecondary;
        public string wall;
        public string[] ghosts;
    }
}
