using Newtonsoft.Json;

namespace Tangram.Json
{
    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings;

        public JsonSerializer()
        {
            _settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        }

        string ISerializer.Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, _settings);
        }

        public T Deserialize<T>(string data) where T : class
        {
            return JsonConvert.DeserializeObject<T>(data, _settings) as T;
        }
    }
}
