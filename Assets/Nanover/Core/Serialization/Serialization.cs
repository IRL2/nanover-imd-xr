using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nanover.Core.Serialization
{
    /// <summary>
    /// Serialization methods for converting to and from serializable C# objects (consisting of
    /// dictionaries, lists and primitives) using JSON.NET.
    /// </summary>
    public static class Serialization
    {
        private static JsonSerializer Serializer = new JsonSerializer()
        {
            Converters =
            {
                new Vector2Converter(),
                new Vector3Converter(),
                new QuaternionConverter(),
                new ColorConverter(),
            },
        };
        
        /// <summary>
        /// Serialize an object from a data structure consisting of
        /// <see cref="Dictionary{TKey,TValue}" />,
        /// <see cref="List{Object}" />, <see cref="string" />, <see cref="float" />,
        /// <see cref="string" /> and <see cref="bool" />, using a
        /// <see cref="JsonSerializer" />.
        /// </summary>
        public static T FromDataStructure<T>(object data)
        {
            using (var reader = new CSharpObjectReader(data))
            {
                return Serializer.Deserialize<T>(reader);
            }
        }
        
        /// <summary>
        /// Update an object from a data structure consisting of
        /// <see cref="Dictionary{TKey,TValue}" />,
        /// <see cref="List{Object}" />, <see cref="string" />, <see cref="float" />,
        /// <see cref="string" /> and <see cref="bool" />, using a
        /// <see cref="JsonSerializer" />.
        /// </summary>
        public static void UpdateFromDataStructure(object data, object target)
        {
            using (var reader = new CSharpObjectReader(data))
            {
                Serializer.Populate(reader, target);
            }
        }

        /// <summary>
        /// Deserialize an object from a data structure consisting of
        /// <see cref="Dictionary{TKey,TValue}" />,
        /// <see cref="List{Object}" />, <see cref="string" />, <see cref="float" />,
        /// <see cref="string" /> and <see cref="bool" />, using a
        /// <see cref="JsonSerializer" />.
        /// </summary>
        /// <remarks>
        /// If dictionaries or lists are present in data, then these will be deserialized
        /// into <see cref="JObject" /> and <see cref="JArray" />.
        /// </remarks>
        public static object FromDataStructure(object data)
        {
            using (var reader = new CSharpObjectReader(data))
            {
                return Serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Deserialize an object of a specific type from a data structure consisting of
        /// <see cref="Dictionary{TKey,TValue}" />,
        /// <see cref="List{Object}" />, <see cref="string" />, <see cref="float" />,
        /// <see cref="string" /> and <see cref="bool" />, using a
        /// <see cref="JsonSerializer" />.
        /// </summary>
        /// <remarks>
        /// If raw dictionaries or lists are present in data, then these will be
        /// deserialized into <see cref="JObject" /> and <see cref="JArray" />.
        /// </remarks>
        public static object ToDataStructure(object data)
        {
            using (var writer = new CSharpObjectWriter())
            {
                Serializer.Serialize(writer, data);
                return writer.Object;
            }
        }
    }
}
