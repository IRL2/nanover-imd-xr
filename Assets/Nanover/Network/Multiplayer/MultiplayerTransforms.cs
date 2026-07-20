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
    }

    [DataContract]
    public class MultiplayerTransform
    {
        [DataMember(Name = "transform")]
        public Transformation Transformation;
        [DataMember(Name = "parent")]
        public string Parent;
    }
}
