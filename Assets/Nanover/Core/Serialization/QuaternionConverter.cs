using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Nanover.Core.Serialization
{
    /// <summary>
    /// <see cref="JsonConverter{T}" /> for serializing a <see cref="Quaternion" /> as a
    /// list of four floats.
    /// </summary>
    public class QuaternionConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader,
                                        Type objectType,
                                        object existingValue,
                                        JsonSerializer serializer)
        {
            var obj = JToken.Load(reader);
            if (obj.Type == JTokenType.Array)
            {
                var arr = (JArray) obj;
                if (arr.Count == 4 && arr.All(token => token.Type == JTokenType.Float || token.Type == JTokenType.Integer))
                {
                    return new Quaternion(arr[0].Value<float>(), arr[1].Value<float>(),
                                          arr[2].Value<float>(), arr[3].Value<float>());
                }
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vector = (Quaternion) value;
            writer.WriteStartArray();
            writer.WriteValue(vector.x);
            writer.WriteValue(vector.y);
            writer.WriteValue(vector.z);
            writer.WriteValue(vector.w);
            writer.WriteEndArray();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Quaternion);
        }
    }
}