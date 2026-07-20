using System;
using System.Linq;
using Nanover.Core.Math;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Nanover.Core.Serialization
{
    /// <summary>
    /// <see cref="JsonConverter{T}" /> for serializing a <see cref="Vector2" /> as a
    /// list of two floats.
    /// </summary>
    public class TransformationConverter : JsonConverter
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
                if (arr.Count == 10 && arr.All(token => token.Type == JTokenType.Float || token.Type == JTokenType.Integer))
                {
                    return new Transformation(
                        new Vector3(
                            arr[0].Value<float>(), 
                            arr[1].Value<float>(), 
                            arr[2].Value<float>()
                        ),
                        new Quaternion(
                            arr[3].Value<float>(), 
                            arr[4].Value<float>(), 
                            arr[5].Value<float>(), 
                            arr[6].Value<float>()
                        ),
                        new Vector3(
                            arr[7].Value<float>(), 
                            arr[8].Value<float>(), 
                            arr[9].Value<float>()
                        )
                    );
                }
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var transformation = (Transformation) value;

            writer.WriteStartArray();
            writer.WriteValue(transformation.Position.x);
            writer.WriteValue(transformation.Position.y);
            writer.WriteValue(transformation.Position.z);
            writer.WriteValue(transformation.Rotation.x);
            writer.WriteValue(transformation.Rotation.y);
            writer.WriteValue(transformation.Rotation.z);
            writer.WriteValue(transformation.Rotation.w);
            writer.WriteValue(transformation.Scale.x);
            writer.WriteValue(transformation.Scale.y);
            writer.WriteValue(transformation.Scale.z);
            writer.WriteEndArray();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Transformation);
        }
    }
}