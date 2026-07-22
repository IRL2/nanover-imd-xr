using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Nanover.Core.Serialization
{
    /// <summary>
    /// <see cref="JsonConverter{T}" /> for serializing a <see cref="Color" /> as a
    /// list of four floats.
    /// </summary>
    public class ColorConverter : JsonConverter
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
                    var color = Color.black;
                    color.r = arr[0].Value<float>();
                    color.g = arr[1].Value<float>();
                    color.b = arr[2].Value<float>();
                    color.a = arr[3].Value<float>();
                    return color;
                }
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var color = (Color) value;
            writer.WriteStartArray();
            writer.WriteValue(color.r);
            writer.WriteValue(color.g);
            writer.WriteValue(color.b);
            writer.WriteValue(color.a);
            writer.WriteEndArray();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Color);
        }
    }
}