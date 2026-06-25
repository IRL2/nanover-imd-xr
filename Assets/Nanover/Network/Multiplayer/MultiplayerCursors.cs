using System.Collections.Generic;
using System.Linq;

namespace Nanover.Network.Multiplayer
{
    /// <summary>
    /// A collection of multiplayer avatars stored in the shared state.
    /// </summary>
    public class MultiplayerCursors : MultiplayerCollection<MultiplayerCursor>
    {
        internal MultiplayerCursors(MultiplayerSession session) : base(session)
        {
            Multiplayer.MultiplayerJoined += OnMultiplayerJoined;
        }

        private void OnMultiplayerJoined()
        {
            LocalCursorLeft = new MultiplayerCursor()
            {
                OwnerID = Multiplayer.AccessToken
            };
        }

        /// <inheritdoc cref="MultiplayerCollection{TItem}.KeyPrefix"/>
        protected override string KeyPrefix => "cursor.";
        
        /// <inheritdoc cref="MultiplayerCollection{TItem}.ParseItem"/>
        protected override bool ParseItem(string key, object value, out MultiplayerCursor parsed)
        {
            if (value is Dictionary<string, object> dict)
            {
                parsed = Core.Serialization.Serialization.FromDataStructure<MultiplayerCursor>(dict);
                parsed.OwnerID = key.Remove(0, KeyPrefix.Length);
                return true;
            }

            parsed = default;
            return false;
        }

        /// <inheritdoc cref="MultiplayerCollection{TItem}.SerializeItem"/>
        protected override object SerializeItem(MultiplayerCursor item)
        {
            return Core.Serialization.Serialization.ToDataStructure(item);
        }
        
        public MultiplayerCursor LocalCursorLeft = new MultiplayerCursor();
        public MultiplayerCursor LocalCursorRight = new MultiplayerCursor();

        private string LocalCursorLeftId => $"{Multiplayer.AccessToken}.left";
        private string LocalCursorRightId => $"{Multiplayer.AccessToken}.right";

        public void FlushLocalCursors()
        {
            if (LocalCursorLeft != null)
                UpdateValue(LocalCursorLeftId, LocalCursorLeft);
            else
                RemoveValue(LocalCursorLeftId);

            if (LocalCursorRight != null)
                UpdateValue(LocalCursorRightId, LocalCursorRight);
            else
                RemoveValue(LocalCursorRightId);
        }
    }
}
