using Chess.NET.Shared.Model.Chat;
namespace Chess.NET.Netcode
{
    /// <summary>
    /// Implémentation de IChatConnection pour des tests.
    /// </summary>
    public class FakeConnection : IChatConnection
    {
        public event Action<ChatMessage>? OnMessageReceived;

        public FakeConnection(string botUsername, string oauthToken, string channelToJoin)
        {
        }

        /// <summary>
        /// Pas de connexion à établir.
        /// </summary>
        public void Connect()
        {
        }

        /// <summary>
        /// Pas de connexion à fermer.
        /// </summary>
        public void Disconnect()
        {
        }

        /// <summary>
        /// Simule la réception d'un message entrant en générant un événement de message reçu avec le nom d'utilisateur
        /// et le contenu du message spécifiés.
        /// </summary>
        /// <remarks>Cette méthode est principalement destinée aux scénarios de test ou de développement
        /// pour déclencher manuellement l'événement OnMessageReceived comme si un message réel avait été
        /// reçu.</remarks>
        /// <param name="username">Le nom d'utilisateur à associer au message simulé. Ne peut pas être null.</param>
        /// <param name="message">Le contenu du message à simuler. Ne peut pas être null.</param>
        public void SimulateIncomingMessage(string username, string message)
        {
            var msg = ChatMessage.CreateBuilder()
                .WithUsername(username)
                .WithDisplayName(username)
                .WithMessage(message)
                .Build();

            OnMessageReceived?.Invoke(msg);
        }

        /// <summary>
        /// Envoie un message dans le canal indiqué.
        /// </summary>
        public void SendMessage(string message)
        {
            //[TODO]
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Vérifie si la connexion est active.
        /// </summary>
        public bool IsConnected => true;
    }
}
