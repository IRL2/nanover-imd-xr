using Nanover.Core.Math;
using System.Runtime.Serialization;

namespace Nanover.Network.Multiplayer
{
    public class MultiplayerTransforms : MultiplayerCollectionDictionary<MultiplayerTransform>
    {
        public MultiplayerTransforms(MultiplayerSession session) : base(session)
        {
        }

        protected override string KeyPrefix => "transform.";

        protected override bool ParseItem(string key, object value, out MultiplayerTransform parsed)
        {
            if (base.ParseItem(key, value, out parsed))
            {
                parsed.ID = key.Remove(0, KeyPrefix.Length);
                return true;
            }

            return false;
        }
    }

    [DataContract]
    public class MultiplayerTransform
    {
        public string ID;
        [DataMember(Name = "transform")]
        public Transformation Transformation;
        [DataMember(Name = "parent")]
        public string Parent;
    }
}
