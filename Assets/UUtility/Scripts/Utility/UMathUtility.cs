using System;
using UnityEngine;

namespace UTool.Utility
{
    public static partial class UUtility
    {
        public static bool RoughlyEqual(this float a, float b, float treshold)
        {
            return (Mathf.Abs(a - b) < treshold);
        }

        public static float Invert(this float value, float maxValue = 1f) => value.RangedMapUnClamp(0f, maxValue, maxValue, 0f);

        public static float RangedMapUnClamp(this float value, float InMinimum, float InMaximum, float OutMinimum, float OutMaximum, bool clampMin = false, bool clampMax = false)
        {
            var InRange = InMaximum - InMinimum;
            var OutRange = OutMaximum - OutMinimum;
            var finalValue = ((value - InMinimum) * OutRange / InRange) + OutMinimum;

            bool isOutMinimumMaximum = OutMinimum > OutMaximum;

            if (clampMin)
                finalValue = isOutMinimumMaximum ? finalValue.ClampMin(OutMaximum) : finalValue.ClampMin(OutMinimum);

            if (clampMax)
                finalValue = isOutMinimumMaximum ? finalValue.ClampMax(OutMinimum) : finalValue.ClampMax(OutMaximum);

            return finalValue;
        }

        public static float RangedMapClamp(this float value, float InMinimum, float InMaximum, float OutMinimum, float OutMaximum)
        {
            return value.RangedMapUnClamp(InMinimum, InMaximum, OutMinimum, OutMaximum, clampMin: true, clampMax: true);
        }

        public static int Clamp(this int value, int min, int max)
        {
            return Mathf.Clamp(value, min, max);
        }

        public static float Clamp(this float value, float min, float max)
        {
            return Mathf.Clamp(value, min, max);
        }

        public static int ClampMin(this int value, int min)
        {
            if (value < min)
                value = min;

            return value;
        }

        public static float ClampMin(this float value, float min)
        {
            if (value < min)
                value = min;

            return value;
        }

        public static int ClampMax(this int value, int max)
        {
            if (value > max)
                value = max;

            return value;
        }

        public static float ClampMax(this float value, float max)
        {
            if (value > max)
                value = max;

            return value;
        }

        ///https://github.com/ManeFunction/unity-mane/tree/master
        /// <summary>
        /// Truncates a float value to a specified number of decimal places.
        /// </summary>
        /// <param name="value">The float value to truncate.</param>
        /// <param name="tail">The number of decimal places to keep.</param>
        /// <returns>The truncated float value.</returns>
        public static float Cut(this float value, int tail)
        {
            float t = Mathf.Pow(10, tail);
            int intValue = (int)(value * t);

            return intValue / t;
        }

        public static Vector2Int Round(this Vector2 vector)
        {
            return new Vector2Int((int)vector.x, (int)vector.y);
        }

        public static Vector3Int Round(this Vector3 vector)
        {
            return new Vector3Int((int)vector.x, (int)vector.y, (int)vector.z);
        }

        public static float Calculate3PointAngle(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return Calculate3PointAngle((Vector3)p1, (Vector3)p2, (Vector3)p3);
        }

        public static float Calculate3PointAngle(Vector3 a, Vector3 b, Vector3 c)
        {
            float num = (a.x - b.x) * (c.x - b.x) +
                        (a.y - b.y) * (c.y - b.y) +
                        (a.z - b.z) * (c.z - b.z);

            double den = Math.Sqrt(Math.Pow(a.x - b.x, 2) +
                                   Math.Pow(a.y - b.y, 2) +
                                   Math.Pow(a.z - b.z, 2))
                                             *
                         Math.Sqrt(Math.Pow(c.x - b.x, 2) +
                                   Math.Pow(c.y - b.y, 2) +
                                   Math.Pow(c.z - b.z, 2));

            double angle = Math.Acos(num / den) * (180.0 / MathF.PI);
            return (float)angle;
        }

        public static Vector2 GetCircleVector(float angle)
        {
            angle = angle * MathF.PI / 180;
            float x = MathF.Sin(angle);
            float y = MathF.Cos(angle);
            return new Vector2(x, y);
        }

        public static int Signum(this int value) => Signum((float)value);
        public static int Signum(this float value)
        {
            if (value == 0)
                return 0;
            return value > 0 ? 1 : -1;
        }

        public static float RoundOff(this float value, float roundValue)
        {
            return (float)Math.Round(value / roundValue) * roundValue;
        }

        public static Vector2 Multiple(this Vector2 vector1, Vector2 vector2)
        {
            return new Vector2(vector1.x * vector2.x, vector1.y * vector2.y);
        }

        public static Vector2 Divide(this Vector2 vector1, Vector2 vector2)
        {
            return new Vector2(vector1.x / vector2.x, vector1.y / vector2.y);
        }

        public static Vector3 Multiple(this Vector3 vector1, Vector3 vector2)
        {
            return new Vector3(vector1.x * vector2.x, vector1.y * vector2.y, vector1.z * vector2.z);
        }

        public static Vector3 Divide(this Vector3 vector1, Vector3 vector2)
        {
            return new Vector3(vector1.x / vector2.x, vector1.y / vector2.y, vector1.z / vector2.z);
        }
    }
}