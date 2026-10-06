using UnityEngine;

public static class PacmanFrames
{
    public static Texture2D[] Chomp(Texture2D source)
    {
        return new[]
        {
            Copy(source),
            CutWedge(source, 22f),
            Copy(source),
            CutWedge(source, 55f)
        };
    }

    public static Texture2D LivesIcon(Texture2D source)
    {
        return CutWedge(source, 27.5f);
    }

    public static Texture2D[] Shrink(Texture2D source, int frameCount)
    {
        if (frameCount < 2)
            frameCount = 11;

        var frames = new Texture2D[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            float scale = Mathf.Lerp(1f, 0.05f, i / (float)(frameCount - 1));
            frames[i] = Scale(source, scale);
        }
        return frames;
    }

    static Texture2D CutWedge(Texture2D source, float halfAngleDegrees)
    {
        int w = source.width;
        int h = source.height;
        Color32[] pixels = source.GetPixels32();
        float half = halfAngleDegrees * Mathf.Deg2Rad;
        float cx = (w - 1) * 0.5f;
        float cy = (h - 1) * 0.5f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float angle = Mathf.Atan2(dy, dx);
                if (Mathf.Abs(angle) <= half)
                {
                    int index = y * w + x;
                    Color32 pixel = pixels[index];
                    pixel.a = 0;
                    pixels[index] = pixel;
                }
            }
        }

        return FromPixels(w, h, pixels);
    }

    static Texture2D Scale(Texture2D source, float scale)
    {
        int w = source.width;
        int h = source.height;
        Color32[] sourcePixels = source.GetPixels32();
        var pixels = new Color32[w * h];
        float cx = (w - 1) * 0.5f;
        float cy = (h - 1) * 0.5f;
        float inv = scale <= 0.0001f ? 0f : 1f / scale;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float sx = cx + (x - cx) * inv;
                float sy = cy + (y - cy) * inv;
                int ix = Mathf.RoundToInt(sx);
                int iy = Mathf.RoundToInt(sy);
                if (ix < 0 || iy < 0 || ix >= w || iy >= h)
                    continue;
                pixels[y * w + x] = sourcePixels[iy * w + ix];
            }
        }

        return FromPixels(w, h, pixels);
    }

    static Texture2D Copy(Texture2D source)
    {
        return FromPixels(source.width, source.height, source.GetPixels32());
    }

    static Texture2D FromPixels(int width, int height, Color32[] pixels)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixels32(pixels);
        texture.Apply(false, false);
        return texture;
    }
}
