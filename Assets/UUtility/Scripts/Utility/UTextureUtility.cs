using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UTool.Utility
{
    public static partial class UUtility
    {
        public static Texture2D ToTexture2D(this RenderTexture renderTexture)
        {
            Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            RenderTexture.active = renderTexture;
            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex.Apply();
            return tex;
        }

        //public static Vector2Int GetSize(this Texture2D texture) => new Vector2Int(texture.width, texture.height);
        public static Vector2Int GetSize(this Texture texture) => new Vector2Int(texture.width, texture.height);

        public static Sprite CreateSprite(this Texture2D texture)
            => Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

        public static void Scale(this Texture2D texture, Vector2Int newSize, Action<Texture2D> callback, bool returnNewTexture = false, bool useBilinear = false)
            => texture.Scale(newSize.x, newSize.y, callback, returnNewTexture: returnNewTexture, useBilinear: useBilinear);

        public static void Scale(this Texture2D texture, int newWidth, int newHeight, Action<Texture2D> callback, bool returnNewTexture = false, bool useBilinear = false)
        {
            TextureScaler scaler = new TextureScaler();
            scaler.Scale(texture, newWidth, newHeight, callback, returnNewTexture: returnNewTexture, useBilinear: useBilinear);
        }

        public static Texture2D Crop(this Texture2D texture, int xPos, int yPos, int newWidth, int newHeight, bool returnNewTexture = false)
            => texture.Crop(new Vector2Int(xPos, yPos), new Vector2Int(newWidth, newHeight), returnNewTexture);

        public static Texture2D Crop(this Texture2D texture, Vector2Int position, Vector2Int size, bool returnNewTexture = false)
        {
            return texture.Crop(Vector2.one * 0.5f, position, Vector2.one * 0.5f, size, returnNewTexture);
        }

        public static Texture2D Crop(this Texture2D texture, Vector2 positionPivot, Vector2Int position, Vector2 sizePivot, Vector2Int size, bool returnNewTexture = false)
        {
            return CropTexture(texture, positionPivot, position, sizePivot, size, returnNewTexture);
        }

        //https://forum.unity.com/threads/code-snippet-size-rawimage-to-parent-keep-aspect-ratio.381616/
        public static Vector2 SizeToParent(this RawImage image, bool preserveHeight = true, float padding = 0)
        {
            var imageRect = image.rectTransform;
            if (!imageRect)
                return imageRect.sizeDelta; //if we don't have a parent, just return our current width;

            padding = 1 - padding;
            float w = 0, h = 0;

            float wRatio = image.texture.width / (float)image.texture.height;
            float hRratio = image.texture.height / (float)image.texture.width;

            var bounds = new Rect(0, 0, imageRect.rect.width, imageRect.rect.height);
            if (Mathf.RoundToInt(imageRect.eulerAngles.z) % 180 == 90)
            {
                //Invert the bounds if the image is rotated
                bounds.size = new Vector2(bounds.height, bounds.width);
            }

            if (preserveHeight)
            {
                //Size by height first
                h = bounds.height * padding;
                w = h * wRatio;

                //if (w > bounds.width * padding)
                //{ //If it doesn't fit, fallback to width;
                //    w = bounds.width * padding;
                //    h = w / wRatio;
                //}
            }
            else
            {
                w = bounds.width * padding;
                h = w * hRratio;

                //if (h > bounds.height * padding)
                //{ //If it doesn't fit, fallback to width;
                //    h = bounds.height * padding;
                //    w = h / hRratio;
                //}
            }

            imageRect.sizeDelta = new Vector2(w, h);
            return imageRect.sizeDelta;
        }

        //By ChatGPT 28/03/2023 and Modified
        public static Vector2Int ResizeWithAspectRatio(int originalWidth, int originalHeight, int targetWidth, int targetHeight)
        {
            float ratioX = (float)targetWidth / (float)originalWidth;
            float ratioY = (float)targetHeight / (float)originalHeight;
            float ratio = Mathf.Min(ratioX, ratioY);

            Vector2Int newSize = Vector2Int.zero;

            newSize.x = (int)(originalWidth * ratio);
            newSize.y = (int)(originalHeight * ratio);

            return newSize;
        }

        private class TextureScaler
        {
            private Color32[] texColors;
            private Color32[] newColors;

            private float widthRatio;
            private float heightRatio;

            private int texWidth;
            private int texHeight;
            private int newTexWidth;
            private int newTexHeight;

            public async void Scale(Texture2D tex, int newWidth, int newHeight,
                                    Action<Texture2D> callback, bool returnNewTexture = false, bool useBilinear = false)
            {
                texWidth = tex.width;
                texHeight = tex.height;

                Vector2Int newSize = ResizeWithAspectRatio(texWidth, texHeight, newWidth, newHeight);
                newTexWidth = newSize.x;
                newTexHeight = newSize.y;

                texColors = tex.GetPixels32();

                await Task.Run(() =>
                {
                    newColors = new Color32[newTexWidth * newTexHeight];

                    if (useBilinear)
                    {
                        widthRatio = 1.0f / ((float)newTexWidth / (texWidth - 1));
                        heightRatio = 1.0f / ((float)newTexHeight / (texHeight - 1));
                    }
                    else
                    {
                        widthRatio = ((float)texWidth) / newTexWidth;
                        heightRatio = ((float)texHeight) / newTexHeight;
                    }
                });

                int coreCount = (int)((float)SystemInfo.processorCount / 3);
                if (coreCount <= 0)
                    coreCount = 1;

                var slice = newTexHeight / coreCount;

                List<Task> scalingTaskList = new List<Task>();

                for (int i = 0; i < coreCount; i++)
                {
                    int startIndex = slice * i;
                    int endIndex = slice * (i + 1);

                    Task scalingTask = new Task(() =>
                    {
                        if (useBilinear)
                            BilinearScale(startIndex, endIndex);
                        else
                            PointScale(startIndex, endIndex);
                    });

                    scalingTaskList.Add(scalingTask);
                }

                Parallel.ForEach(scalingTaskList, x => x.Start());
                await Task.WhenAll(scalingTaskList);

                Texture2D newTexture = null;

                if (returnNewTexture)
                {
                    newTexture = new Texture2D(newTexWidth, newTexHeight, textureFormat: TextureFormat.RGBA32, false);
                    UT._instance.StartCoroutine(ApplyTexture(newTexture, newColors, () =>
                    {
                        InvokeCallback(newTexture);
                    }));
                }
                else
                {
                    tex.Reinitialize(newTexWidth, newTexHeight);
                    UT._instance.StartCoroutine(ApplyTexture(tex, newColors, () =>
                    {
                        InvokeCallback(tex);
                    }));
                }

                void InvokeCallback(Texture2D texture)
                {
                    texColors = null;
                    newColors = null;

                    callback?.Invoke(texture);
                }
            }

            private void BilinearScale(int startIndex, int endIndex)
            {
                Parallel.For(startIndex, endIndex, y =>
                {
                    int yFloor = (int)Mathf.Floor(y * heightRatio);
                    var y1 = yFloor * texWidth;
                    var y2 = (yFloor + 1) * texWidth;
                    var yw = y * newTexWidth;

                    Parallel.For(0, newTexWidth, x =>
                    {
                        int xFloor = (int)Mathf.Floor(x * widthRatio);
                        var xLerp = x * widthRatio - xFloor;
                        newColors[yw + x] = Color32.LerpUnclamped(Color32.LerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor + 1], xLerp),
                                                               Color32.LerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor + 1], xLerp),
                                                               y * heightRatio - yFloor);
                    });
                });
            }

            private void PointScale(int startIndex, int endIndex)
            {
                Parallel.For(startIndex, endIndex, y =>
                {
                    var thisY = (int)(heightRatio * y) * texWidth;
                    var yw = y * newTexWidth;

                    Parallel.For(0, newTexWidth, x =>
                    {
                        newColors[yw + x] = texColors[(int)(thisY + widthRatio * x)];
                    });
                });
            }
        }

        public static Texture2D CropTexture(Texture2D sourceTexture, Vector2 positionPivot, Vector2Int position,
                                                                Vector2 sizePivot, Vector2Int size, bool returnNewTexture = false)
        {
            int sourceWidth = sourceTexture.width;
            int sourceHeight = sourceTexture.height;

            int width = size.x.Clamp(1, sourceWidth);
            int height = size.y.Clamp(1, sourceHeight);

            int xPivotOffset = (int)Mathf.Floor(sourceWidth * positionPivot.x);
            int yPivotOffset = (int)Mathf.Floor(sourceHeight * positionPivot.y);

            int xSizePivotOffset = (int)Mathf.Floor(width * sizePivot.x);
            int ySizePivotOffset = (int)Mathf.Floor(height * sizePivot.y);

            int xPos = (xPivotOffset + position.x) - xSizePivotOffset;
            int yPos = (yPivotOffset + position.y) - ySizePivotOffset;

            xPos = xPos.Clamp(0, sourceWidth);
            yPos = yPos.Clamp(0, sourceHeight);

            int xExcessSize = sourceWidth - (width + xPos);
            int yExcessSize = sourceHeight - (height + yPos);

            xExcessSize = xExcessSize.ClampMax(0);
            yExcessSize = yExcessSize.ClampMax(0);

            width += xExcessSize;
            height += yExcessSize;

            width = width.ClampMin(1);
            height = height.ClampMin(1);

            Color[] croppedPixelsColors = sourceTexture.GetPixels(xPos, yPos, width, height);

            Texture2D croppedTexture = null;
            if (returnNewTexture)
            {
                croppedTexture = new Texture2D(width, height, textureFormat: TextureFormat.RGBA32, false);
                croppedTexture.SetPixels(croppedPixelsColors);
                croppedTexture.Apply();

                return croppedTexture;
                //UT._instance.StartCoroutine(ApplyTexture(croppedTexture, croppedPixelsColors, () =>
                //{
                //    callback?.Invoke(croppedTexture);
                //}));
            }
            else
            {
                sourceTexture.Reinitialize(width, height);
                sourceTexture.SetPixels(croppedPixelsColors);
                sourceTexture.Apply();

                return sourceTexture;
                //UT._instance.StartCoroutine(ApplyTexture(sourceTexture, croppedPixelsColors, () =>
                //{
                //    callback?.Invoke(sourceTexture);
                //}));
            }
        }

        //By ChatGPT 28/03/2023 and Modified
        public static async void Rotate(this Texture2D texture, float angle, Action<Texture2D> callback, bool returnNewTexture = false)
        {
            // Get the pixels of the input texture
            Color32[] pixels = texture.GetPixels32();

            // Calculate the new width and height of the rotated texture
            int width = texture.width;
            int height = texture.height;
            float angleRad = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);
            int newWidth = Mathf.RoundToInt(Mathf.Abs(width * cos) + Mathf.Abs(height * sin));
            int newHeight = Mathf.RoundToInt(Mathf.Abs(height * cos) + Mathf.Abs(width * sin));

            Color32[] rotatedPixels = new Color32[newWidth * newHeight];

            // Create a new texture to hold the rotated pixels
            Texture2D outputTexture = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
            int texWidth = outputTexture.width;
            int texHeight = outputTexture.height;

            await Task.Run(() =>
            {
                // Loop through each pixel in the rotated texture
                Parallel.For(0, texHeight, y =>
                {
                    Parallel.For(0, texWidth, x =>
                        {
                            // Calculate the coordinates of the pixel in the original texture
                            float oldX = ((x - texWidth / 2f) * cos - (y - texHeight / 2f) * sin) + (width / 2f);
                            float oldY = ((x - texWidth / 2f) * sin + (y - texHeight / 2f) * cos) + (height / 2f);
                            int pixelX = Mathf.FloorToInt(oldX);
                            int pixelY = Mathf.FloorToInt(oldY);

                            int rotatedIndex = y * texWidth + x;

                            if (pixelX <= 0 || pixelX > width)
                            {
                                rotatedPixels[rotatedIndex] = Color.clear;
                                return;
                            }

                            if (pixelY <= 0 || pixelY > height)
                            {
                                rotatedPixels[rotatedIndex] = Color.clear;
                                return;
                            }

                            // Get the color of the pixel from the original texture
                            int index = pixelY * width + pixelX;
                            if (index >= 0 && index < pixels.Length)
                                rotatedPixels[rotatedIndex] = pixels[index];
                            else
                                rotatedPixels[rotatedIndex] = Color.clear;
                        });
                });
            });

            if (returnNewTexture)
            {
                UT._instance.StartCoroutine(ApplyTexture(outputTexture, rotatedPixels, () =>
                {
                    callback?.Invoke(outputTexture);
                }));
            }
            else
            {
                GameObject.Destroy(outputTexture);
                texture.Reinitialize(texWidth, texHeight);

                UT._instance.StartCoroutine(ApplyTexture(texture, rotatedPixels, () =>
                {
                    callback?.Invoke(texture);
                }));
            }
        }

        public static IEnumerator ApplyTexture(Texture2D texture, Color[] colorData, Action callback)
        {
            yield return null;

            texture.SetPixels(colorData);

            yield return null;

            texture.Apply();
            callback?.Invoke();
        }

        public static IEnumerator ApplyTexture(Texture2D texture, Color32[] colorData, Action callback)
        {
            yield return null;

            texture.SetPixels32(colorData);

            yield return null;

            texture.Apply();
            callback?.Invoke();
        }
    }
}