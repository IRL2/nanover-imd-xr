using Nanover.Core.Math;
using Nanover.Core.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Nanover.Network.Multiplayer
{
    public class MultiplayerObjects<TObject> : MultiplayerCollection<TObject>
    {
        public MultiplayerObjects(MultiplayerSession session) : base(session)
        {
        }

        protected override string KeyPrefix => "object.";

        protected override bool ParseItem(string key, object value, out TObject parsed)
        {
            if (value is Dictionary<string, object> dict)
            {
                parsed = Serialization.FromDataStructure<TObject>(dict);
                return true;
            }

            parsed = default;
            return false;
        }

        protected override object SerializeItem(TObject item)
        {
            return Serialization.ToDataStructure(item);
        }
    }

    public class MultiplayerObjectsShape : MultiplayerObjects<MultiplayerObjectShape>
    {
        public MultiplayerObjectsShape(MultiplayerSession session) : base(session) {}
        protected override string KeyPrefix => "object.shape";
    }

    public class MultiplayerObjectsLine : MultiplayerObjects<MultiplayerObjectLine>
    {
        public MultiplayerObjectsLine(MultiplayerSession session) : base(session) { }
        protected override string KeyPrefix => "object.line";
    }

    public class MultiplayerObjectsLabel : MultiplayerObjects<MultiplayerObjectLabel>
    {
        public MultiplayerObjectsLabel(MultiplayerSession session) : base(session) { }
        protected override string KeyPrefix => "object.label";
    }

    [DataContract]
    public class MultiplayerObjectShape
    {
        [DataMember(Name = "parent")]
        public string Parent;
        [DataMember(Name = "shape")]
        public string Shape;
        [DataMember(Name = "position")]
        public Vector3 Position;
        [DataMember(Name = "size")]
        public float Size;
        [DataMember(Name = "color")]
        public Color Color;
    }

    [DataContract]
    public class MultiplayerObjectLine
    {
        [DataMember(Name = "parent")]
        public string Parent;
        [DataMember(Name = "positions")]
        public Vector3[] Positions;
        [DataMember(Name = "colors")]
        public Color[] Colors;
        [DataMember(Name = "sizes")]
        public float[] Sizes;
        [DataMember(Name = "size")]
        public float Size;
        [DataMember(Name = "color")]
        public Color Color;
        [DataMember(Name = "type")]
        public string Type;
    }

    [DataContract]
    public class MultiplayerObjectLabel
    {
        [DataMember(Name = "parent")]
        public string Parent;
        [DataMember(Name = "text")]
        public string Text;
        [DataMember(Name = "position")]
        public Vector3 Position;
        [DataMember(Name = "size")]
        public float Size;
        [DataMember(Name = "color")]
        public Color Color;
    }
}
