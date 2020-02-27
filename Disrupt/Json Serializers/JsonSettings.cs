using Newtonsoft.Json;

namespace RavelTek.Disrupt.Custom_Serializers
{
    public class JsonSettings
    {
        private static readonly object locket = new object();
        public static readonly JsonSettings instance = new JsonSettings();
        public static JsonSettings Instance
        {
            get
            {
                return instance;
            }
        }
        public void Add(JsonConverter converter)
        {
            lock (locket)
            {
                var contains = false;
                if (Settings.Converters.Count == 0)
                {
                    Settings.Converters.Add(converter);
                    return;
                }
                foreach (var cnvtr in Settings.Converters)
                {
                    if (cnvtr.ToString() == converter.ToString())
                    {
                        contains = true;
                        break;
                    }
                }
                if (contains) return;
                Settings.Converters.Add(converter);
            }
        }
        public JsonSerializerSettings Settings { get; } = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
        };
    }
}
