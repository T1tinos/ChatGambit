using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace Chess.NET.Shared.Model.Chat
{
    /// <summary>
    /// Implémentation de IChatConnection pour Twitch.
    /// Gère UNIQUEMENT la connexion et la communication avec Twitch.
    /// </summary>
    public class TwitchConnection : IChatConnection
    {
        private readonly TwitchClient _client;
        private readonly string _channelToJoin;

        /// <summary>
        /// Événement déclenché quand un message est reçu du chat Twitch.
        /// </summary>
        public event Action<ChatMessage>? OnMessageReceived;

        public TwitchConnection(string botUsername, string oauthToken, string channelToJoin)
        {
            _channelToJoin = channelToJoin;

            // Configuration de la connexion
            ConnectionCredentials credentials = new ConnectionCredentials(botUsername, oauthToken);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);

            _client = new TwitchClient(customClient);
            _client.Initialize(credentials, channelToJoin);

            // Abonnement à l'événement de réception de message
            _client.OnMessageReceived += (sender, args) =>
            {
                OnTiwtchMessageReceived(sender, args);
            };
        }

        

        /// <summary>
        /// Établit la connexion avec Twitch.
        /// </summary>
        public void Connect()
        {
            _client.Connect();
            System.Diagnostics.Debug.WriteLine("[TwitchConnection] Connecté à Twitch");
        }

        /// <summary>
        /// Ferme la connexion avec Twitch.
        /// </summary>
        public void Disconnect()
        {
            _client.Disconnect();
            System.Diagnostics.Debug.WriteLine("[TwitchConnection] Déconnecté de Twitch");
        }

        /// <summary>
        /// Envoie un message dans le canal Twitch.
        /// </summary>
        public void SendMessage(string message)
        {
            if (_client.IsConnected)
            {
                _client.SendMessage(_channelToJoin, message);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[TwitchConnection] Erreur: Non connecté à Twitch. Message non envoyé.");
            }
        }

        /// <summary>
        /// Réception du message chat Twitch, conversion en ChatMessage et déclenchement de l'événement OnMessageReceived.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnTiwtchMessageReceived(object sender, OnMessageReceivedArgs args)
        {
            var twitchMsg = args.ChatMessage;

            var chatMessage = ChatMessage.CreateBuilder()
                .WithUsername(twitchMsg.Username)
                .WithDisplayName(twitchMsg.DisplayName)
                .WithUserId(twitchMsg.UserId)
                .WithChannel(twitchMsg.Channel)
                .WithMessage(twitchMsg.Message)
                .WithSubscriber(twitchMsg.IsSubscriber, twitchMsg.SubscribedMonthCount)
                .WithModerator(twitchMsg.IsModerator)
                .WithBroadcaster(twitchMsg.IsBroadcaster)
                .WithVip(twitchMsg.IsVip)
                .WithColorHex(twitchMsg.ColorHex)
                .WithHighlightedMessage(twitchMsg.IsHighlighted)
                .WithBadges(twitchMsg.Badges.Select(b => b.Key))
                .Build();

            OnMessageReceived?.Invoke(chatMessage);
        }

        /// <summary>
        /// Vérifie si la connexion est active.
        /// </summary>
        public bool IsConnected => _client.IsConnected;
    }
}
