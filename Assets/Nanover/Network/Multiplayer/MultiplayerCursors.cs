using System.Collections.Generic;

namespace Nanover.Network.Multiplayer
{
    /// <summary>
    /// A collection of multiplayer avatars stored in the shared state.
    /// </summary>
    public class MultiplayerCursors : MultiplayerCollectionDictionary<MultiplayerCursor>
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
            if (base.ParseItem(key, value, out parsed))
            {
                parsed.OwnerID = key.Remove(0, KeyPrefix.Length);
                return true;
            }

            return false;
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
