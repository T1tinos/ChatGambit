using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.NET.Shared.Model.Chat
{
    public class ChatMessage
    {
        // Identité
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string UserId { get; set; }
        public string Channel { get; set; }

        // Contenu
        public string Message { get; set; }
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        // Statut utilisateur
        public bool IsSubscriber { get; set; }
        public int SubscriberMonths { get; set; }
        public bool IsModerator { get; set; }
        public bool IsBroadcaster { get; set; }
        public bool IsVip { get; set; }

        // Twitch extras
        public string ColorHex { get; set; }
        public bool IsHighlightedMessage { get; set; }
        public List<string> Badges { get; set; } = new();

        // Constructeur privé => passage par le Builder
        private ChatMessage() { }

        public static Builder CreateBuilder() => new Builder();

        // ────────────────────────────────
        public class Builder
        {
            private readonly ChatMessage _msg = new();

            public Builder WithUsername(string username)
            {
                _msg.Username = username ?? throw new ArgumentNullException(nameof(username));
                return this;
            }

            public Builder WithDisplayName(string displayName)
            {
                _msg.DisplayName = displayName;
                return this;
            }

            public Builder WithUserId(string userId)
            {
                _msg.UserId = userId;
                return this;
            }

            public Builder WithChannel(string channel)
            {
                _msg.Channel = channel;
                return this;
            }

            public Builder WithMessage(string message)
            {
                _msg.Message = message ?? throw new ArgumentNullException(nameof(message));
                return this;
            }

            public Builder WithSubscriber(bool isSubscriber, int months = 0)
            {
                _msg.IsSubscriber = isSubscriber;
                _msg.SubscriberMonths = months;
                return this;
            }

            public Builder WithModerator(bool isModerator)
            {
                _msg.IsModerator = isModerator;
                return this;
            }

            public Builder WithBroadcaster(bool isBroadcaster)
            {
                _msg.IsBroadcaster = isBroadcaster;
                return this;
            }

            public Builder WithVip(bool isVip)
            {
                _msg.IsVip = isVip;
                return this;
            }

            public Builder WithColorHex(string colorHex)
            {
                _msg.ColorHex = colorHex;
                return this;
            }

            public Builder WithHighlightedMessage(bool isHighlighted)
            {
                _msg.IsHighlightedMessage = isHighlighted;
                return this;
            }

            public Builder WithBadges(IEnumerable<string> badges)
            {
                _msg.Badges.AddRange(badges);
                return this;
            }

            public ChatMessage Build()
            {
                if (string.IsNullOrWhiteSpace(_msg.Username))
                    throw new InvalidOperationException("Username est obligatoire.");
                if (string.IsNullOrWhiteSpace(_msg.Message))
                    throw new InvalidOperationException("Message est obligatoire.");

                return _msg;
            }
        }
    }
}
