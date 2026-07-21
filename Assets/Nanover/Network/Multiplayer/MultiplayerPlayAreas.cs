using Nanover.Core.Math;
using Nanover.Core.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Nanover.Network.Multiplayer
{
    /// <summary>
    /// Tracks play areas of a multiuser session i.e user-reported corners for
    /// their VR bounds in the shared space.
    /// </summary>
    public class PlayAreaCollection : MultiplayerCollectionDictionary<PlayArea>
    {
        public PlayAreaCollection(MultiplayerSession session) : base(session)
        {
        }

        protected override string KeyPrefix => "playarea.";
    }

    /// <summary>
    /// Tracks server-suggested transform origin for clients.
    /// </summary>
    public class PlayOriginCollection : MultiplayerCollectionDictionary<PlayOrigin>
    {
        public PlayOriginCollection(MultiplayerSession session) : base(session)
        {
        }

        protected override string KeyPrefix => "user-origin.";
    }

    /// <summary>
    /// Four corners of a VR play area.
    /// </summary>
    [DataContract]
    public class PlayArea
    {
        [DataMember]
        public Vector3 A;
        [DataMember]
        public Vector3 B;
        [DataMember]
        public Vector3 C;
        [DataMember]
        public Vector3 D;
    }

    /// <summary>
    /// A UnitScaleTransformation for user origins
    /// </summary>
    [DataContract]
    public class PlayOrigin
    {
        /// <summary>
        /// The position of the component.
        /// </summary>
        [DataMember(Name = "position")]
        public Vector3 Position;

        /// <summary>
        /// The rotation of the component.
        /// </summary>
        [DataMember(Name = "rotation")]
        public Quaternion Rotation;

        /// <summary>
        /// The component as a <see cref="UnitScaleTransformation"/>
        /// </summary>
        [IgnoreDataMember]
        public UnitScaleTransformation Transformation =>
            new UnitScaleTransformation(Position, Rotation);
    }
}
