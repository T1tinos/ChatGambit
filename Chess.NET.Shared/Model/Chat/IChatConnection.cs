using TwitchLib.Client.Events;

namespace Chess.NET.Shared.Model.Chat
{
    /// <summary>
    /// Interface abstraite pour la connexion à une plateforme de streaming de chat.
    /// Permet de supporter Twitch, Discord, YouTube, etc.
    /// </summary>
    public interface IChatConnection
    {
        /// <summary>
        /// Événement déclenché quand un message est reçu du chat.
        /// </summary>
        public event Action<ChatMessage>? OnMessageReceived;

        /// <summary>
        /// Établit la connexion avec la plateforme.
        /// </summary>
        void Connect();

        /// <summary>
        /// Ferme la connexion avec la plateforme.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Envoie un message dans le canal.
        /// </summary>
        void SendMessage(string message);

        /// <summary>
        /// Vérifie si la connexion est active.
        /// </summary>
        bool IsConnected { get; }
    }
}
