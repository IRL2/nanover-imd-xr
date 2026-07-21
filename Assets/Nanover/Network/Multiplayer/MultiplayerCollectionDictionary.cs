using Nanover.Core.Serialization;
using System.Collections.Generic;

namespace Nanover.Network.Multiplayer
{
    public class MultiplayerCollectionDictionary<TObject> : MultiplayerCollection<TObject>
    {
        public MultiplayerCollectionDictionary(MultiplayerSession session) : base(session)
        {
        }

        protected override string KeyPrefix => "";

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
}
