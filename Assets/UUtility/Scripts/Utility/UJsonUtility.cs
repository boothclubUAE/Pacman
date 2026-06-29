using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;

namespace UTool.Utility
{
    public static partial class UUtility
    {
        public static string ToJsonString(this object obj, bool readable = false)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();

            if (readable)
                settings.Formatting = Formatting.Indented;

            return obj.ToJsonString(settings);
        }

        public static string ToJsonString(this object obj, JsonSerializerSettings jsonSettings)
        {
            return JsonConvert.SerializeObject(obj, jsonSettings);
        }

        public static bool FromJsonString<T>(this string jsonString, out T obj)
        {
            try
            {
                obj = JsonConvert.DeserializeObject<T>(jsonString);
                return true;
            }
            catch
            {
                obj = default(T);
                return false;
            }
        }
    }
}