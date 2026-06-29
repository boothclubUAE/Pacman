using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTool.Utility
{
    public static partial class UUtility
    {
        public static string ColorCoat(this string text, Color color)
        {
            return text.ColorCoat(ColorUtility.ToHtmlStringRGB(color));
        }

        public static string ColorCoat(this string text, string htmlStringRGB)
        {
            return $"<color=#{htmlStringRGB}>{text}</color>";
        }

        public static string ToHourMinSec(this float time)
        {
            TimeSpan ts = TimeSpan.FromSeconds(time);
            return ts.ToHourMinSec();
        }

        public static string ToMinSec(this float time)
        {
            TimeSpan ts = TimeSpan.FromSeconds(time);
            return ts.ToMinSec();
        }

        public static string ToSecMill(this float time)
        {
            TimeSpan ts = TimeSpan.FromSeconds(time);
            return ts.ToSecMill();
        }

        public static string ToMinSecMill(this float time)
        {
            TimeSpan ts = TimeSpan.FromSeconds(time);
            return ts.ToMinSecMill();
        }

        public static string ToddMMyyyyhhmmss(this DateTime dateTime)
        {
            return dateTime.ToString("dd_MM_yyyy_hh_mm_ss");
        }

        public static string Tohhmmss(this DateTime dateTime, string spacer = "_")
        {
            return dateTime.ToString($"hh{spacer}mm{spacer}ss");
        }

        public static string ToddMMyyyy(this DateTime dateTime, string spacer = "_")
        {
            return dateTime.ToString($"dd{spacer}MM{spacer}yyyy");
        }

        ///https://github.com/ManeFunction/unity-mane/tree/master
        /// <summary>
        /// Tries to parse a string to a float. If the parsing fails, returns a default value.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="defaultValue">The default value to return if the parsing fails.</param>
        /// <returns>The parsed float value, or the default value if the parsing fails.</returns>
        public static float ParseFloat(this string str, float defaultValue = 0f)
        {
            if (!float.TryParse(str, out float result))
                result = defaultValue;

            return result;
        }

        ///https://github.com/ManeFunction/unity-mane/tree/master
        /// <summary>
        /// Tries to parse a string to an integer. If the parsing fails, returns a default value.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="defaultValue">The default value to return if the parsing fails.</param>
        /// <returns>The parsed integer value, or the default value if the parsing fails.</returns>
        public static int ParseInt(this string str, int defaultValue = 0)
        {
            if (!int.TryParse(str, out int result))
                result = defaultValue;

            return result;
        }
    }
}