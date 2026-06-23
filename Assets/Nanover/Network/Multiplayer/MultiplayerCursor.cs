using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Nanover.Core;
using Nanover.Core.Math;
using UnityEngine;

namespace Nanover.Network.Multiplayer
{
    [DataContract]
    public class MultiplayerCursor
    {
        [DataMember(Name = "ownerid")]
        public string OwnerID { get; set; }

        [DataMember(Name = "position")]
        public Vector3 Position;

        [DataMember(Name = "rotation")]
        public Quaternion Rotation;

        [DataMember(Name = "ispressed")]
        public bool IsPressed;
    }
}
