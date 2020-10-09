using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace BulkyBook.Utility
{
    public static class SessionExtension
    {
        public static void SetObj(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObj<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}