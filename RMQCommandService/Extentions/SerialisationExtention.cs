using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace RMQCommandService.Extentions
{
    public static class SerialisationExtention
    {
        private static readonly BinaryFormatter Formatter = new BinaryFormatter();

        public static byte[] SerializeToBinary<T>(this T obj)
        {
            var json = JsonConvert.SerializeObject(
                obj,
                new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });

            using var ms = new MemoryStream();

            Formatter.Serialize(ms, json);
            return ms.ToArray();
        }

        public static T DeserializeFromBinary<T>(this byte[] data)
        {
            using var ms = new MemoryStream(data);

            var obj = Formatter.Deserialize(ms) as string;

            return JsonConvert.DeserializeObject<T>(obj ?? throw new InvalidOperationException(),
                new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                }) ?? throw new InvalidOperationException();
        }
    }
}
