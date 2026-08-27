using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MaleFashion.Infrastructure.Extensions
{
    public static class TempDataExtensions
    {
        public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class
        {
            tempData[key] = JsonSerializer.Serialize(value);
        }

        public static T? Get<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            object o;
            tempData.TryGetValue(key, out o);
            return o == null ? default(T) : JsonSerializer.Deserialize<T>((string)o);
        }

        public static T? Peek<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            object o = tempData.Peek(key);
            return o == null ? default(T) : JsonSerializer.Deserialize<T>((string)o);
        }
    }
}
