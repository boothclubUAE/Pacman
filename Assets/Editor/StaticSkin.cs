using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Writes the current data.bin into sprites and scene objects so the editor
// shows it before Play. A newer data.bin is picked up while the editor is open.
[InitializeOnLoad]
static class StaticSkin
{
    const string ScenePath = "Assets/Scenes/Pacman.unity";
    const string Folder = "Assets/Sprites/Default";

    static bool busy;
    static double nextCheck;
    static string failedHash;

    static StaticSkin()
    {
        EditorApplication.delayCall += BakeIfNeeded;
        EditorApplication.update += Watch;
    }

    static void Watch()
    {
        if (busy || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            return;
        if (EditorApplication.timeSinceStartup < nextCheck)
            return;
        nextCheck = EditorApplication.timeSinceStartup + 1.5;
        BakeIfNeeded();
    }

    static void BakeIfNeeded()
    {
        if (busy || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        string packPath = PackPath();
        if (packPath == null)
            return;

        byte[] file;
        try
        {
            file = File.ReadAllBytes(packPath);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("data.bin was not read. " + ex.Message);
            return;
        }

        if (!SkinFile.TryRead(file, out Dictionary<string, byte[]> entries))
            return;

        string hash = Hash(file);
        if (hash == failedHash || (hash == ReadStampHash() && ReadStampValue(3) == "board4"))
            return;

        if (!SceneReady())
            return;

        busy = true;
        try
        {
            Bake(entries);
            WriteStamp(hash, entries);
            failedHash = null;
            Debug.Log("Applied data.bin to the open scene.");
        }
        catch (Exception ex)
        {
            failedHash = hash;
            Debug.LogWarning("data.bin was not applied in the editor. " + ex.Message);
        }
        finally
        {
            busy = false;
        }
    }

    static void Bake(Dictionary<string, byte[]> entries)
    {
        Directory.CreateDirectory(Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Assets", "Sprites", "Default"));

        if (entries.TryGetValue("pellet", out byte[] pelletPng))
        {
            Sprite existing = PrefabSprite("Assets/Prefabs/Pellet.prefab");
            Sprite sprite = ImportSprite(Folder + "/dot.png", pelletPng, PixelsPerUnit(existing, pelletPng, 24f));
            AssignPrefabSprite("Assets/Prefabs/Pellet.prefab", sprite, singleFrame: false);
        }

        for (int i = 0; i < 4; i++)
        {
            if (!entries.TryGetValue("power" + i, out byte[] png))
                continue;
            string prefab = i == 0 ? "Assets/Prefabs/PowerPellet.prefab" : "Assets/Prefabs/PowerPellet " + i + ".prefab";
            PngSize(png, out int width, out int height);
            bool tall = height > width;
            bool top = i < 2;
            const float fit = 0.85f;
            float world = (tall ? 3f : 1f) * fit;
            float boardPpu = tall ? height / world : Mathf.Max(width, height) / world;
            Vector2 pivot = tall && !top ? new Vector2(0.5f, 0.5f / world) : new Vector2(0.5f, 0.5f);
            ImportSprite(Folder + "/item" + i + ".png", png, width > 0 ? width : 100f);
            Sprite board = ImportSprite(Folder + "/board" + i + ".png", png, boardPpu, pivot);
            AssignPrefabSprite(prefab, board, singleFrame: true);
        }

        if (entries.TryGetValue("pacman", out byte[] pacmanPng))
            BakePacman(pacmanPng);

        Scene scene = LoadedScene();
        if (entries.TryGetValue("colors", out byte[] colorBytes))
        {
            string previousPrimary = ReadStampValue(1);
            string previousSecondary = ReadStampValue(2);
            SkinApplier.ApplyColors(scene, System.Text.Encoding.UTF8.GetString(colorBytes));
            RecolorPrevious(scene, System.Text.Encoding.UTF8.GetString(colorBytes), previousPrimary, previousSecondary);
            BakeGhostPrefabs(System.Text.Encoding.UTF8.GetString(colorBytes));
        }

        ApplySceneImages(scene, entries);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    static void BakePacman(byte[] png)
    {
        var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!source.LoadImage(png))
        {
            UnityEngine.Object.DestroyImmediate(source);
            return;
        }

        Sprite existing = PrefabSprite("Assets/Prefabs/Pacman.prefab");
        float ppu = existing != null && existing.rect.width > 0f
            ? source.width / (existing.rect.width / existing.pixelsPerUnit)
            : 8f;

        Texture2D[] chompFrames = PacmanFrames.Chomp(source);
        int deathCount = DeathFrameCount();
        Texture2D[] deathFrames = PacmanFrames.Shrink(source, deathCount);
        Texture2D lifeFrame = PacmanFrames.LivesIcon(source);
        UnityEngine.Object.DestroyImmediate(source);

        var chomp = new Sprite[chompFrames.Length];
        for (int i = 0; i < chompFrames.Length; i++)
            chomp[i] = ImportSprite(Folder + "/player" + i + ".png", Encode(chompFrames[i]), ppu);

        var death = new Sprite[deathFrames.Length];
        for (int i = 0; i < deathFrames.Length; i++)
            death[i] = ImportSprite(Folder + "/out" + i + ".png", Encode(deathFrames[i]), ppu);

        Sprite life = ImportSprite(Folder + "/life.png", Encode(lifeFrame), ppu);

        var root = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Pacman.prefab");
        SpriteRenderer body = root.GetComponent<SpriteRenderer>();
        AnimatedSprite chompAnim = root.GetComponent<AnimatedSprite>();
        if (body != null && chomp.Length > 0)
            body.sprite = chomp[0];
        if (chompAnim != null)
            chompAnim.sprites = chomp;

        foreach (var animated in root.GetComponentsInChildren<AnimatedSprite>(true))
        {
            if (animated.gameObject == root)
                continue;
            animated.sprites = death;
            SpriteRenderer renderer = animated.GetComponent<SpriteRenderer>();
            if (renderer != null && death.Length > 0)
                renderer.sprite = death[0];
            break;
        }

        PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Pacman.prefab");
        PrefabUtility.UnloadPrefabContents(root);
    }

    static void ApplySceneImages(Scene scene, Dictionary<string, byte[]> entries)
    {
        Sprite logo = entries.ContainsKey("logo")
            ? ImportSprite(Folder + "/mark.png", entries["logo"], 100f)
            : null;

        var powers = new Sprite[4];
        for (int i = 0; i < 4; i++)
        {
            if (entries.ContainsKey("power" + i))
                powers[i] = AssetDatabase.LoadAssetAtPath<Sprite>(Folder + "/item" + i + ".png");
        }

        Sprite life = AssetDatabase.LoadAssetAtPath<Sprite>(Folder + "/life.png");

        foreach (var image in UnityEngine.Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (image.gameObject.scene != scene)
                continue;

            if (logo != null && (image.gameObject.name == "Client Logo" || image.gameObject.name == "ClientLogo"))
            {
                image.sprite = logo;
                image.preserveAspect = true;
                EditorUtility.SetDirty(image);
                continue;
            }

            if (life != null && image.gameObject.name == "LivesIndicator")
            {
                image.sprite = life;
                EditorUtility.SetDirty(image);
                continue;
            }

            Transform parent = image.transform.parent;
            if (parent == null || parent.name != "Collectables")
                continue;
            int index = image.transform.GetSiblingIndex();
            if (index < 0 || index >= powers.Length || powers[index] == null)
                continue;
            image.sprite = powers[index];
            image.preserveAspect = true;
            EditorUtility.SetDirty(image);
        }
    }

    static void BakeGhostPrefabs(string json)
    {
        SkinApplier.SkinColorSet colors = JsonUtility.FromJson<SkinApplier.SkinColorSet>(json);
        if (colors == null || colors.ghosts == null)
            return;

        string[] paths =
        {
            "Assets/Prefabs/Ghost_Blinky.prefab",
            "Assets/Prefabs/Ghost_Pinky.prefab",
            "Assets/Prefabs/Ghost_Inky.prefab",
            "Assets/Prefabs/Ghost_Clyde.prefab"
        };

        for (int i = 0; i < paths.Length && i < colors.ghosts.Length; i++)
        {
            if (!TryParse(colors.ghosts[i], out Color color))
                continue;
            var root = PrefabUtility.LoadPrefabContents(paths[i]);
            Transform body = FindNamed(root.transform, "Body");
            if (body != null)
            {
                SpriteRenderer renderer = body.GetComponent<SpriteRenderer>();
                if (renderer != null)
                    renderer.color = color;
            }
            PrefabUtility.SaveAsPrefabAsset(root, paths[i]);
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void RecolorPrevious(Scene scene, string json, string previousPrimary, string previousSecondary)
    {
        SkinApplier.SkinColorSet colors = JsonUtility.FromJson<SkinApplier.SkinColorSet>(json);
        if (colors == null)
            return;

        bool hasPrimary = TryParse(colors.textPrimary, out Color primary);
        bool hasSecondary = TryParse(colors.textSecondary, out Color secondary);
        bool hadPrimary = TryParse(previousPrimary, out Color oldPrimary);
        bool hadSecondary = TryParse(previousSecondary, out Color oldSecondary);
        if (!hasPrimary && !hasSecondary)
            return;

        foreach (var text in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (text.gameObject.scene != scene)
                continue;
            string objectName = text.gameObject.name;
            if (objectName == "Timer" || objectName == "CollectablesText")
                continue;
            if (hasPrimary && hadPrimary && Near(text.color, oldPrimary))
                text.color = primary;
            else if (hasSecondary && hadSecondary && Near(text.color, oldSecondary))
                text.color = secondary;
        }
    }

    static bool SceneReady()
    {
        Scene scene = EditorSceneManager.GetSceneByPath(ScenePath);
        if (scene.IsValid() && scene.isLoaded)
            return true;
        return !EditorSceneManager.GetActiveScene().isDirty;
    }

    static Scene LoadedScene()
    {
        Scene scene = EditorSceneManager.GetSceneByPath(ScenePath);
        if (scene.IsValid() && scene.isLoaded)
            return scene;
        return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    static int DeathFrameCount()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pacman.prefab");
        if (prefab == null)
            return 11;
        foreach (var animated in prefab.GetComponentsInChildren<AnimatedSprite>(true))
        {
            if (animated.gameObject == prefab)
                continue;
            int count = 0;
            if (animated.sprites != null)
            {
                for (int i = 0; i < animated.sprites.Length; i++)
                {
                    if (animated.sprites[i] != null)
                        count++;
                }
            }
            return count >= 2 ? count : 11;
        }
        return 11;
    }

    static void AssignPrefabSprite(string path, Sprite sprite, bool singleFrame)
    {
        if (sprite == null)
            return;
        var root = PrefabUtility.LoadPrefabContents(path);
        SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.sprite = sprite;
        if (singleFrame)
        {
            AnimatedSprite animated = root.GetComponent<AnimatedSprite>();
            if (animated != null)
                animated.sprites = new[] { sprite };
        }
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    static Sprite PrefabSprite(string path)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
            return null;
        SpriteRenderer renderer = prefab.GetComponent<SpriteRenderer>();
        return renderer != null ? renderer.sprite : null;
    }

    static void PngSize(byte[] png, out int width, out int height)
    {
        width = 0;
        height = 0;
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(png))
        {
            UnityEngine.Object.DestroyImmediate(texture);
            return;
        }
        width = texture.width;
        height = texture.height;
        UnityEngine.Object.DestroyImmediate(texture);
    }

    static Sprite ImportSprite(string assetPath, byte[] png, float ppu)
    {
        return ImportSprite(assetPath, png, ppu, new Vector2(0.5f, 0.5f));
    }

    static Sprite ImportSprite(string assetPath, byte[] png, float ppu, Vector2 pivot)
    {
        string full = Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        File.WriteAllBytes(full, png);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsToUnits = ppu;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 4096;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        if (pivot == new Vector2(0.5f, 0.5f))
            settings.spriteAlignment = (int)SpriteAlignment.Center;
        else
        {
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
        }
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    static float PixelsPerUnit(Sprite existing, byte[] png, float fallback)
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(png))
        {
            UnityEngine.Object.DestroyImmediate(texture);
            return fallback;
        }
        int width = texture.width;
        UnityEngine.Object.DestroyImmediate(texture);
        if (existing == null || existing.rect.width <= 0f || width <= 0)
            return fallback;
        return width / (existing.rect.width / existing.pixelsPerUnit);
    }

    static byte[] Encode(Texture2D texture)
    {
        byte[] png = texture.EncodeToPNG();
        UnityEngine.Object.DestroyImmediate(texture);
        return png;
    }

    static Transform FindNamed(Transform root, string name)
    {
        if (root.name == name)
            return root;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindNamed(root.GetChild(i), name);
            if (found != null)
                return found;
        }
        return null;
    }

    static string PackPath()
    {
        string external = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "data.bin");
        if (File.Exists(external))
            return external;
        string bundled = Path.Combine(Application.streamingAssetsPath, "data.bin");
        if (File.Exists(bundled))
            return bundled;
        return null;
    }

    static string StampPath()
    {
        return Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Assets", "Sprites", "Default", "stamp.txt");
    }

    static string ReadStampHash()
    {
        return ReadStampValue(0);
    }

    static string ReadStampValue(int line)
    {
        string path = StampPath();
        if (!File.Exists(path))
            return null;
        string[] lines = File.ReadAllLines(path);
        if (line < 0 || line >= lines.Length)
            return null;
        return lines[line];
    }

    static void WriteStamp(string hash, Dictionary<string, byte[]> entries)
    {
        string primary = "";
        string secondary = "";
        if (entries.TryGetValue("colors", out byte[] colorBytes))
        {
            SkinApplier.SkinColorSet colors = JsonUtility.FromJson<SkinApplier.SkinColorSet>(System.Text.Encoding.UTF8.GetString(colorBytes));
            if (colors != null)
            {
                primary = colors.textPrimary ?? "";
                secondary = colors.textSecondary ?? "";
            }
        }
        File.WriteAllText(StampPath(), hash + "\n" + primary + "\n" + secondary + "\nboard4\n");
        AssetDatabase.ImportAsset(Folder + "/stamp.txt");
    }

    static string Hash(byte[] file)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] hash = sha.ComputeHash(file);
            return BitConverter.ToString(hash).Replace("-", "");
        }
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

    static bool Near(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.03f
            && Mathf.Abs(a.g - b.g) < 0.03f
            && Mathf.Abs(a.b - b.b) < 0.03f;
    }
}
